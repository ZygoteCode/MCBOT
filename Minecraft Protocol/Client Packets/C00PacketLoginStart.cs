namespace MCBOT
{
    public class C00PacketLoginStart : MinecraftPacket
    {
        public string Username { get; private set; }

        public C00PacketLoginStart(string username)
        {
            Username = username;

            WriteVarInt(0x00);
            WriteString(username);
        }
    }
}