using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowServer
{
    public class Server
    {
        private const int Port = 12345;

        private TcpListener _server;
        private Thread      _listenThread;
        private Client      _client;
        private bool        _isShuttingDown;

        public Server()
        {
            _isShuttingDown = false;
        }

        public bool Start()
        {
            try
            {
                var ip  = IPAddress.Parse("0.0.0.0");
                _server = new TcpListener(ip, Port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start WindowServer");
                Console.WriteLine(e);
                return false;
            }

            StartListen();
            return true;
        }

        public void Stop()
        {
            try
            {
                _server.Stop();
                _client.Stop();
                _isShuttingDown = true;
            }
            catch (Exception)
            {
            }
        }

        private void StartListen()
        {
            _listenThread = new Thread(Listen);
            _listenThread.Start();
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
                        _client = new Client(_server.AcceptTcpClient());
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

            _server.Stop();
        }
    }
}
