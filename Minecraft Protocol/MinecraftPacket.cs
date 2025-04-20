using System.Text;

namespace MCBOT
{
    public class MinecraftPacket
    {
        private byte[] _packetData;
        private const int SEGMENT_BITS = 0x7F;
        private const int CONTINUE_BIT = 0x80;

        public MinecraftPacket(byte[] packetData = null)
        {
            if (packetData == null)
            {
                _packetData = new byte[] { };
            }
            else
            {
                _packetData = packetData;
            }
        }

        public byte[] CompletePacket()
        {
            return _packetData;
        }

        private byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

            return ret;
        }

        public MinecraftPacket Clone()
        {
            return new MinecraftPacket(_packetData);
        }

        public void WriteBoolean(bool value)
        {
            if (value)
            {
                _packetData = Combine(_packetData, new byte[1] { 0x01 });
            }
            else
            {
                _packetData = Combine(_packetData, new byte[1] { 0x00 });
            }
        }

        public bool ReadBoolean()
        {
            byte firstByte = _packetData[0];
            _packetData = _packetData.Skip(1).ToArray();

            if (firstByte == 0x00)
            {
                return false;
            }

            return true;
        }

        public void WriteByte(sbyte value)
        {
            _packetData = Combine(_packetData, new byte[1] { (byte)value });
        }

        public sbyte ReadByte()
        {
            byte firstByte = _packetData[0];
            _packetData = _packetData.Skip(1).ToArray();
            return (sbyte)firstByte;
        }

        public void WriteUByte(byte value)
        {
            _packetData = Combine(_packetData, new byte[1] { value });
        }

        public byte ReadUByte()
        {
            byte firstByte = _packetData[0];
            _packetData = _packetData.Skip(1).ToArray();
            return firstByte;
        }

        public void WriteBytes(byte[] value)
        {
            _packetData = Combine(_packetData, value);
        }

        public byte[] ReadBytes(int length)
        {
            byte[] bytes = _packetData.Take(length).ToArray();
            _packetData = _packetData.Skip(length).ToArray();
            return bytes;
        }

        public void WriteShort(short value)
        {
            _packetData = Combine(_packetData, BitConverter.GetBytes(value));
        }

        public short ReadShort()
        {
            byte[] bytes = _packetData.Take(2).ToArray();
            _packetData = _packetData.Skip(2).ToArray();
            return BitConverter.ToInt16(bytes, 0);
        }

        public void WriteUShort(ushort value)
        {
            _packetData = Combine(_packetData, BitConverter.GetBytes(value));
        }

        public ushort ReadUShort()
        {
            byte[] bytes = _packetData.Take(2).ToArray();
            _packetData = _packetData.Skip(2).ToArray();
            return BitConverter.ToUInt16(bytes, 0);
        }

        public void WriteInt(int value)
        {
            _packetData = Combine(_packetData, BitConverter.GetBytes(value));
        }

        public int ReadInt()
        {
            byte[] bytes = _packetData.Take(4).ToArray();
            _packetData = _packetData.Skip(4).ToArray();
            return BitConverter.ToInt32(bytes, 0);
        }

        public void WriteUInt(uint value)
        {
            _packetData = Combine(_packetData, BitConverter.GetBytes(value));
        }

        public uint ReadUInt()
        {
            byte[] bytes = _packetData.Take(4).ToArray();
            _packetData = _packetData.Skip(4).ToArray();
            return BitConverter.ToUInt32(bytes, 0);
        }

        public void WriteLong(long value)
        {
            _packetData = Combine(_packetData, BitConverter.GetBytes(value));
        }

        public long ReadLong()
        {
            byte[] bytes = _packetData.Take(8).ToArray();
            _packetData = _packetData.Skip(8).ToArray();
            return BitConverter.ToInt64(bytes, 0);
        }

        public void WriteFloat(float value)
        {
            _packetData = Combine(_packetData, BitConverter.GetBytes(value));
        }

        public float ReadFloat()
        {
            byte[] bytes = _packetData.Take(4).ToArray();
            _packetData = _packetData.Skip(4).ToArray();
            return BitConverter.ToSingle(bytes, 0);
        }

        public void WriteDouble(double value)
        {
            _packetData = Combine(_packetData, BitConverter.GetBytes(value));
        }

        public double ReadDouble()
        {
            byte[] bytes = _packetData.Take(8).ToArray();
            _packetData = _packetData.Skip(8).ToArray();
            return BitConverter.ToDouble(bytes, 0);
        }

        public void WriteVarInt(int value)
        {
            while (true)
            {
                if ((value & ~SEGMENT_BITS) == 0)
                {
                    _packetData = Combine(_packetData, new byte[1] { (byte)value });
                    return;
                }

                _packetData = Combine(_packetData, new byte[1] { (byte)((value & SEGMENT_BITS) | CONTINUE_BIT) });
                value = value >>> 7;
            }
        }

        public byte[] GetVarInt(int value)
        {
            byte[] bytes = new byte[] { };

            while (true)
            {
                if ((value & ~SEGMENT_BITS) == 0)
                {
                    bytes = Combine(bytes, new byte[1] { (byte)value });
                    return bytes;
                }

                bytes = Combine(bytes, new byte[1] { (byte)((value & SEGMENT_BITS) | CONTINUE_BIT) });
                value = value >>> 7;
            }
        }

        public int ReadVarInt()
        {
            int value = 0;
            int position = 0;
            byte currentByte;

            while (true)
            {
                currentByte = ReadUByte();
                value |= (currentByte & SEGMENT_BITS) << position;

                if ((currentByte & CONTINUE_BIT) == 0)
                {
                    break;
                }

                position += 7;

                if (position >= 32)
                {
                    throw new Exception("VarInt is too big.");
                }
            }

            return value;
        }

        public void WriteVarLong(long value)
        {
            while (true)
            {
                if ((value & ~SEGMENT_BITS) == 0)
                {
                    _packetData = Combine(_packetData, new byte[1] { (byte)value });
                    return;
                }

                _packetData = Combine(_packetData, new byte[1] { (byte)((value & SEGMENT_BITS) | CONTINUE_BIT) });
                value = value >>> 7;
            }
        }

        public long ReadVarLong()
        {
            long value = 0;
            int position = 0;
            byte currentByte;

            while (true)
            {
                currentByte = ReadUByte();
                value |= (long)(currentByte & SEGMENT_BITS) << position;

                if ((currentByte & CONTINUE_BIT) == 0)
                {
                    break;
                }

                position += 7;

                if (position >= 32)
                {
                    throw new Exception("VarLong is too big.");
                }
            }

            return value;
        }

        public void WriteString(string value)
        {
            WriteVarInt(value.Length);
            _packetData = Combine(_packetData, Encoding.UTF8.GetBytes(value));
        }

        public string ReadString(int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(length));
        }

        public string ReadString()
        {
            return ReadString(ReadVarInt());
        }
    }
}