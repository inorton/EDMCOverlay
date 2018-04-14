using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;


namespace EDMCOverlay
{
    /// <summary>
    /// Listen for TCP connections, maintain a set of graphics for each connection
    /// </summary>
    public class OverlayJsonServer
    {
        private readonly OverlayRenderer _renderer;
        public const int DefaultTtl = 5;
        public const int MaxClients = 5;

        private int messageCount = 0;
        private int messageUnknownCount = 0;
        private int messageErrorCount = 0;

        public int MessageCount { get { return this.messageCount; } }
        public int MessageUnknownCount { get { return this.messageUnknownCount; } }
        public int MessageErrorCount { get { return this.messageErrorCount; } }

        public Logger Logger = Logger.GetInstance(typeof(OverlayJsonServer));

        public int Port { get; private set; }

        private readonly TcpListener _listener;

        private List<Thread> _threads = new List<Thread>();

        private readonly Dictionary<String, InternalGraphic> _graphics = new Dictionary<string, InternalGraphic>();

        private int nextClientId = 1;

        public Dictionary<String, InternalGraphic> Graphics => _graphics;

        public OverlayJsonServer(int port, OverlayRenderer renderer)
        {
            this.Port = port;            
            this._renderer = renderer;
            this._listener = new TcpListener(IPAddress.Loopback, this.Port);
        }

        public void Start()
        {
            this._listener.Start();
            this._renderer.Start(this);

            lock (_graphics)
            {
                Logger.LogMessage("JSON server thread startup");
                var banner = new Graphic
                {
                    TTL = 5,
                    Id = "_",
                    Color = "yellow",
                    Size = GraphicType.FONT_LARGE,
                    X = 30,
                    Y = 130,
                    Text = "/EDMC Overlay/"
                };
                var intro = new InternalGraphic(banner, -1);
                _graphics.Add(banner.Id, intro);
            }

            while (true)
            {
                foreach (var thread in _threads.ToArray())
                {
                    if (thread.Join(10))
                    {
                        _threads.Remove(thread);
                    }
                }

                var client = _listener.AcceptTcpClient();
                Logger.LogMessage(String.Format("New connection from {0}", client.Client.RemoteEndPoint));
                Thread conn = new Thread(new ParameterizedThreadStart(ServerThread));
                _threads.Add(conn);
                conn.Start(client);
            }
        }

        public void SendGraphic(Graphic request, int clientId)
        {
            lock (_graphics)
            {
                if (String.IsNullOrWhiteSpace(request.Text) && String.IsNullOrEmpty(request.Shape) )
                {
                    if (_graphics.ContainsKey(request.Id))
                    {
                        _graphics.Remove(request.Id);
                    }
                }
                else
                {
                    if (_graphics.ContainsKey(request.Id))
                    {
                        _graphics[request.Id].Update(request);
                    }
                    else
                    {
                        _graphics.Add(request.Id,
                            new InternalGraphic(request, clientId));
                    }
                }
            }
        }

        public void ProcessCommand(Graphic request, TcpClient client)
        {
            Interlocked.Increment(ref this.messageCount);            

            if (!String.IsNullOrEmpty(request.Command))
            {
                Logger.LogMessage("Got command: " + request.Command);
                if (request.Command.Equals("exit"))
                {
                    System.Environment.Exit(0);
                }

                if (request.Command.Equals("noop"))
                {
                    return;
                }
                Interlocked.Increment(ref this.messageUnknownCount);
                Logger.LogMessage("Unknown command: " + request.Command);
            }
        }

        public void ServerThread(object obj)
        {
            var clientId = 0;
            lock (_graphics)
            {
                clientId = nextClientId++;
            }

            try
            {
                using (TcpClient client = (TcpClient) obj)
                {
                    StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);                    
                    while (client.Connected)
                    {                        
                        if (client.Client.Poll(100 * 1000, SelectMode.SelectRead))
                        {
                            // poll returned true, we either have some data or the connection was closed by the client
                            String line;                            
                            do
                            {
                                // the connection should block here if the client is still alive
                                line = reader.ReadLine();
                                
                                if (line == null)
                                {
                                    // 
                                    Logger.LogMessage(String.Format("client {0} disconnected..", clientId));
                                    break;  // client disconnected
                                }

                                if (!String.IsNullOrWhiteSpace(line))
                                {
                                    // try and deserialize the buffer.
                                    Logger.LogMessage("got message..");
                                    Graphic request = JsonConvert.DeserializeObject<Graphic>(line);

                                    ProcessCommand(request, client);

                                    SendGraphic(request, clientId);
                                    Logger.LogMessage("sent graphic..");
                                }
                            } while (line != null);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                // maybe log stuff here..
                Logger.LogMessage(String.Format("Exception: {0}", err));
                Interlocked.Increment(ref this.messageErrorCount);
                return;
            }
            finally
            {
                lock (_graphics)
                {
                    // lost connection, remove this connection's graphics
                    foreach (var gid in _graphics.Keys.ToArray())
                    {
                        InternalGraphic g = _graphics[gid];
                        if (g.ClientId == clientId)
                        {
                            _graphics.Remove(gid);
                        }
                    }
                }
            }
        }
    }
}