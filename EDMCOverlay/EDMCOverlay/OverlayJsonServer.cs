using System;
using System.Collections.Generic;
using System.IO;
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
        private OverlayRenderer _renderer;

        public const int MaxClients = 5;

        public int Port { get; private set; }

        private readonly TcpListener _listener;

        private List<Thread> _threads = new List<Thread>();

        private Dictionary<String, Graphic> _graphics = new Dictionary<string, Graphic>();

        public Dictionary<String, Graphic> Graphics { get { return _graphics; }}

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

        public void ServerThread(object obj)
        {
            List<String> graphicIDs = new List<string>();
            try
            {
                using (TcpClient client = (TcpClient) obj)
                {
                    StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                    while (client.Connected)
                    {
                        var line = reader.ReadLine();
                        Graphic request = JsonConvert.DeserializeObject<Graphic>(line);
                        lock (_graphics)
                        {
                            if (request.Id != null)
                            {
                                if (String.IsNullOrWhiteSpace(request.Text))
                                {
                                    if (_graphics.ContainsKey(request.Id))
                                    {
                                        _graphics.Remove(request.Id);
                                    }
                                    if (graphicIDs.Contains(request.Id))
                                    {
                                        graphicIDs.Remove(request.Id);
                                    }
                                }
                                else
                                {
                                    if (_graphics.ContainsKey(request.Id))
                                    {
                                        _graphics[request.Id] = request;
                                    }
                                    else
                                    {
                                        graphicIDs.Add(request.Id);
                                        _graphics.Add(request.Id, request);
                                    }
                                }
                            }
                        }
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
                    foreach (string id in graphicIDs)
                    {
                        if (_graphics.ContainsKey(id))
                        {
                            _graphics.Remove(id);
                        }
                    }
                }
            }
        }
    }
}