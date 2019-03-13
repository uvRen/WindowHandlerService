using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowServer
{
    public class Server
    {
        private readonly int          _port;
        private readonly Thread       _listenThread;
        private readonly List<Client> _clients;

        private TcpListener           _server;
        private bool                  _isShuttingDown;

        public Server(int port = 12345)
        {
            _isShuttingDown = false;
            _port           = port;
            _listenThread   = new Thread(Listen);
            _clients        = new List<Client>();
        }

        public bool Start()
        {
            try
            {
                var ip  = IPAddress.Parse("0.0.0.0");
                _server = new TcpListener(ip, _port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start WindowServer");
                Console.WriteLine(e);
                return false;
            }

            _listenThread.Start();

            return true;
        }

        public void Stop()
        {
            try
            {
                _server.Stop();
                _isShuttingDown = true;
                StopAllClients();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Listen for incoming connections
        /// </summary>
        private void Listen()
        {
            _server.Start();

            while (!_isShuttingDown)
            {
                try
                {
                    if (_server.Pending())
                    {
                        _clients.Add(new Client(_server.AcceptTcpClient()));
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

            _server.Stop();
        }

        private void StopAllClients()
        {
            foreach (var client in _clients)
            {
                client.Stop();
            }
        }
    }
}
