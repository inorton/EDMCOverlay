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
                var banner = new Graphic
                {
                    TTL = 5,
                    Id = "_",
                    Color = "yellow",
                    Size = "large",
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

                Thread conn = new Thread(new ParameterizedThreadStart(ServerThread));
                _threads.Add(conn);
                conn.Start(client);
            }
        }

        public void SendGraphic(Graphic request, int clientId)
        {
            lock (_graphics)
            {
                if (String.IsNullOrWhiteSpace(request.Text))
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
                        var line = reader.ReadLine();

                        if (!_renderer.Attached)
                        {
                            // game quit, bail out and let someone restart us
                            return;
                        }

                        Graphic request = JsonConvert.DeserializeObject<Graphic>(line);
                        SendGraphic(request, clientId);
                    }
                }
            }
            catch (Exception err)
            {
                // maybe log stuff here..
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