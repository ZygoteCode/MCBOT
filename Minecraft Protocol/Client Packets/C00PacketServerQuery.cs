namespace MCBOT
{
    public class C00PacketServerQuery : MinecraftPacket
    {
        public C00PacketServerQuery()
        {
            WriteVarInt(0x00);
        }
    }
}