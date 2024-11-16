namespace ChatWithDBServer
{
    internal static class Program
    {
        [STAThread]
        static void Main ()
        {
            ChatServer chatServer = new ChatServer ();
            chatServer.MainServerLoop();
            while (true) { }

        }
    }
}