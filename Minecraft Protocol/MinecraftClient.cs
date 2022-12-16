using System.Data;
using System.Net;
using System.Net.Sockets;

namespace MCBOT
{
    public class MinecraftClient
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private Thread? _receiveThread;
        private bool _askingForStatus_Handshake;
        public int VersionProtocol { get; set; }

        public MinecraftClient(string ip, ushort port)
        {
            VersionProtocol = -1;
            _client = new TcpClient();
            _client.Connect(ip, port);
            _stream = _client.GetStream();
        }

        public MinecraftClient(TcpClient client)
        {
            VersionProtocol = -1;
            _client = client;
            _stream = _client.GetStream();
        }

        public MinecraftClient(TcpClient client, NetworkStream stream)
        {
            VersionProtocol = -1;
            _client = client;
            _stream = stream;
        }

        public MinecraftClient(IPEndPoint endPoint)
        {
            VersionProtocol = -1;
            _client = new TcpClient();
            _client.Connect(endPoint);
            _stream = _client.GetStream();
        }

        public MinecraftClient(IPAddress address, ushort port)
        {
            VersionProtocol = -1;
            _client = new TcpClient();
            _client.Connect(address, port);
            _stream = _client.GetStream();
        }

        public void Close()
        {
            if (_client != null)
            {
                _client.Close();
            }

            if (_stream != null)
            {
                _stream.Close();
            }
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }

            if (_stream != null)
            {
                _stream.Dispose();
            }
        }

        public void StartReceiving()
        {
            if (_receiveThread == null)
            {
                _receiveThread = new Thread(ReceivePacket);
                _receiveThread.Priority = ThreadPriority.Highest;
                _receiveThread.Start();
            }
        }

        public void StopReceiving()
        {
            if (_receiveThread != null)
            {
                _receiveThread.Interrupt();
                _receiveThread = null;
            }
        }

        private void ReceivePacket()
        {
            while (true)
            {
                try
                {
                    if (!IsConnected())
                    {
                        return;
                    }

                    Thread.Sleep(10);

                    if (_stream.DataAvailable && _stream.CanRead)
                    {
                        Thread.Sleep(100);

                        var batch = new byte[Int16.MaxValue];
                        var bufferTemp = new List<byte>();

                        while (true)
                        {
                            _stream.Read(batch, 0, batch.Length);
                            bufferTemp.AddRange(batch);

                            if (!_stream.DataAvailable || !_stream.CanRead)
                            {
                                break;
                            }
                        }

                        MinecraftPacket packet = new MinecraftPacket(bufferTemp.ToArray());
                        int size = packet.ReadVarInt(), packetId = packet.ReadVarInt();

                        if (packetId == 0x00 && _askingForStatus_Handshake)
                        {
                            _askingForStatus_Handshake = false;
                            int jsonLength = packet.ReadVarInt();
                            string obtainedJson = packet.ReadString(jsonLength);
                            string[] splitted = Microsoft.VisualBasic.Strings.Split(obtainedJson, "\"protocol\":");
                            string theProtocol = splitted[1];
                            string entireProtocol = "";
                            int index = 0;
                            string c = theProtocol[index].ToString();

                            while (Microsoft.VisualBasic.Information.IsNumeric(c))
                            {
                                entireProtocol += c;
                                index++;
                                c = theProtocol[index].ToString();
                            }

                            VersionProtocol = int.Parse(entireProtocol);
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

            return ret;
        }

        public void SendPacket(MinecraftPacket packet)
        {
            try
            {
                if (!IsConnected())
                {
                    return;
                }

                if (packet.GetType() == typeof(C00Handshake))
                {
                    C00Handshake pPacket = (C00Handshake)packet;

                    if (pPacket.ConnectionState == ConnectionState.STATUS)
                    {
                        _askingForStatus_Handshake = true;
                    }
                }

                byte[] thePacket = packet.CompletePacket();
                int length = thePacket.Length;
                thePacket = Combine(packet.GetVarInt(length), thePacket);

                _stream.Write(thePacket, 0, thePacket.Length);
                _stream.Flush();
            }
            catch
            {

            }
        }

        public bool IsConnected()
        {
            return _client != null && _client.Connected;
        }
    }
}