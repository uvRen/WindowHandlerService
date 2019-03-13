using ApiCommon.WindowHandling;
using System;
using System.Net.Sockets;
using System.Threading;
using static ApiCommon.WindowHandling.CommandType;

namespace WindowServer
{
    public class Client
    {
        private const int BufferSize = 1024 * 1024; // 1Mb

        private Thread                 _listenThread;
        private readonly TcpClient     _client;
        private readonly NetworkStream _io;
        private readonly byte[]        _buffer;
        private readonly WindowHandler _windowHandler;

        public Client(TcpClient client)
        {
            _client        = client;
            _io            = _client.GetStream();
            _buffer        = new byte[BufferSize];
            _windowHandler = new WindowHandler();

            StartListen();
        }

        private void StartListen()
        {
            _listenThread = new Thread(Listen);
            _listenThread.Start();
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    _io.Read(_buffer, 0, BufferSize);
                    HandleMessage(_buffer);

                    Array.Clear(_buffer, 0, BufferSize);
                }
                catch (Exception)
                {
                    break;
                }
            }

            Stop();
        }

        private void HandleMessage(byte[] buffer)
        {
            Message message = Message.ToObject(buffer);

            switch (message.Command)
            {
                case Command.MoveWindow:
                    HandleMoveWindow(message);
                    break;
                case Command.FindWindow:
                    HandleFindWindow(message);
                    break;
                case Command.MouseClick:
                    HandleMouseClick(message);
                    break;
                case Command.KeyPress:
                    HandleKeyPress(message);
                    break;
                default:
                    break;
            }
        }

        private void HandleKeyPress(Message message)
        {
            var key = (string) message.GetPayload();
            _windowHandler.KeyPress(key);
        }

        private void HandleMouseClick(Message message)
        {
            var position = (Position)message.GetPayload();
            _windowHandler.MouseClick(position);
        }

        private void HandleFindWindow(Message message)
        {
            var windowName = (string) message.GetPayload();
            _windowHandler.FindWindow(windowName);
        }

        private void HandleMoveWindow(Message message)
        {
            var position = (Position) message.GetPayload();
            _windowHandler.MoveWindow(position);
        }

        public void Stop()
        {
            try
            {
                _io.Close();
                _client.Close();
            }
            catch (Exception)
            {
            }
            
        }
    }
}
