namespace MCBOT
{
    public class C00Handshake : MinecraftPacket
    {
        public uint ProtocolVersion { get; private set; }
        public string Ip { get; private set; }
        public ushort Port { get; private set; }
        public ConnectionState ConnectionState { get; private set; }

        public C00Handshake(uint protocolVersion, string ip, ushort port, ConnectionState connectionState)
        {
            ProtocolVersion = protocolVersion;
            Ip = ip;
            Port = port;
            ConnectionState = connectionState;

            WriteVarInt(0x00);
            WriteVarInt((int)protocolVersion);
            WriteString(ip);
            WriteShort((short)port);
            WriteVarInt((int)connectionState);
        }
    }
}