using ApiCommon.WindowHandling;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ApiCommon.Util.String;
using ApiCommon.WindowHandling.Request;
using static ApiCommon.WindowHandling.CommandType;

namespace WindowServer
{
    public class Client
    {
        private const int BufferSize = 50000; // 50 Kb

        private readonly Thread                   _listenThread;
        private readonly TcpClient                _client;
        private readonly NetworkStream            _io;
        private readonly byte[]                   _buffer;
        private readonly WindowHandler            _windowHandler;
        private readonly ConcurrentQueue<Message> _handleBuffer;
        private readonly CommandBuilder           _commandBuilder;

        private bool _isShuttingDown;

        public Client(TcpClient client)
        {
            _client         = client;
            _io             = _client.GetStream();
            _isShuttingDown = false;
            _buffer         = new byte[BufferSize];
            _windowHandler  = new WindowHandler();
            _handleBuffer   = new ConcurrentQueue<Message>();
            _listenThread   = new Thread(Listen);
            _commandBuilder = new CommandBuilder();

            StartListen();
        }

        private void StartListen()
        {
            _listenThread.Start();
        }

        private void Listen()
        {
            while (!_isShuttingDown)
            {
                try
                {
                    _io.Read(_buffer, 0, BufferSize);

                    HandleMessage(Message.ToObject(_buffer));

                    Array.Clear(_buffer, 0, BufferSize);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Server stopped listen for incoming data");
                    Console.WriteLine(e.Message);
                    var data = Encoding.Default.GetString(_buffer);
                    data = data.RemoveAll("\0");
                    Console.WriteLine(data);
                    break;
                }
            }

            Stop();
        }

        private void HandleMessage(Message message)
        {
            Console.WriteLine("Recieved: " + message.Command);

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
                case Command.MouseDown:
                    HandleMouseDown(message);
                    break;
                case Command.MouseUp:
                    HandleMouseUp(message);
                    break;
                case Command.KeyPress:
                    HandleKeyPress(message);
                    break;
                default:
                    break;
            }
        }

        private void HandleMouseUp(Message message)
        {
            var request = (MouseClickRequest)message.GetPayload();
            var result = _windowHandler.MouseUp(request.Position, request.IoSim);

            var response = result
                ? _commandBuilder.CreateAckResponse()
                : _commandBuilder.CreateNackResponse();

            Send(response);
        }

        private void HandleMouseDown(Message message)
        {
            var request = (MouseClickRequest)message.GetPayload();
            var result = _windowHandler.MouseDown(request.Position, request.IoSim);

            var response = result
                ? _commandBuilder.CreateAckResponse()
                : _commandBuilder.CreateNackResponse();

            Send(response);
        }

        private void HandleKeyPress(Message message)
        {
            var request = (KeyPressRequest) message.GetPayload();
            var result = _windowHandler.KeyPress(request.Key, request.IoSim);

            var response = result
                ? _commandBuilder.CreateAckResponse()
                : _commandBuilder.CreateNackResponse();

            Send(response);
        }

        private void HandleMouseClick(Message message)
        {
            var request = (MouseClickRequest)message.GetPayload();
            var result = _windowHandler.MouseClick(request.Position, request.IoSim);

            var response = result
                ? _commandBuilder.CreateAckResponse()
                : _commandBuilder.CreateNackResponse();

            Send(response);
        }

        private void HandleFindWindow(Message message)
        {
            var request = (FindWindowRequest) message.GetPayload();
            var result = _windowHandler.FindWindow(request.WindowName, request.IoSim);

            var response = result
                ? _commandBuilder.CreateAckResponse()
                : _commandBuilder.CreateNackResponse();

            Send(response);
        }

        private void HandleMoveWindow(Message message)
        {
            var request = (MoveWindowRequest) message.GetPayload();
            var result = _windowHandler.MoveWindow(request.Position, request.IoSim);

            var response = result 
                ? _commandBuilder.CreateAckResponse()
                : _commandBuilder.CreateNackResponse();

            Send(response);
        }

        private void Send(Message message)
        {
            var data = Message.ToByteArray(message);

            try
            {
                _io.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send response");
                Console.WriteLine(e.Message);
            }
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

            _isShuttingDown = true;
        }
    }
}
