using System;
using System.Net.Sockets;
using System.Threading;
using Telegram.Translation;

namespace Telegram.TransportLayer
{
    class TcpConnection : IDisposable
    {
        private readonly string _address;
        private readonly int _port;
        private readonly Formatter _formatter;
        private Socket _socket;

        public TcpConnection(string address, int port, Formatter formatter)
        {
            if (port < 0) throw new ArgumentException();

            _address = address;
            _port = port;
            _formatter = formatter;
        }

        public int PacketNumber { get; set; }

        public bool Connect(bool reconnect)
        {
            if (reconnect || _socket == null || !_socket.Connected)
            {
                if (_socket != null)
                    _socket.Dispose();

                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                _socket.Connect(_address, _port);

                PacketNumber = 0;

                return true;
            }

            return false;
        }

        public byte[] ExchangeWithServer(byte[] clientRequestBytes)
        {
            NetworkStream networkStream = GetStream();

            // Запрос
            networkStream.Write(clientRequestBytes, 0, clientRequestBytes.Length);

            // Ответ
            var buffer = new byte[4096];
            int byteCount = networkStream.Read(buffer, 0, buffer.Length);
            if (byteCount == 0) return null;

            return buffer;
        }

        public void Write(byte[] bytes)
        {
            int offset = 0;
            do
            {
                offset += _socket.Send(bytes, offset, bytes.Length - offset, SocketFlags.None);
            } while (offset < bytes.Length);
        }

        public byte[] Read()
        {
            var buffer = new byte[0];
            var current = new byte[1024];
            int lengthMark = 0;
            do
            {
                int offset = _socket.Receive(current);

                if (offset > 0)
                {
                    int start = buffer.Length;
                    Array.Resize(ref buffer, start + offset);
                    Array.Copy(current, 0, buffer, start, offset);

                    while (start + offset > lengthMark)
                    {
                        int position = lengthMark;
                        lengthMark += _formatter.ReadInt32(buffer, ref position);
                    }
                }
            } while (buffer.Length < lengthMark);
            return buffer;
        }

        public void Dispose()
        {
            _socket.Disconnect(true);
            _socket.Dispose();
        }

        private NetworkStream GetStream()
        {
            return new NetworkStream(_socket);
        }
    }
}