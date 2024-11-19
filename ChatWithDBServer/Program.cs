namespace ChatWithDBServer
{
    internal static class Program
    {
        [STAThread]
        static void Main ()
        {
            //ChatServer chatServer = new ChatServer ();
            ChatDBService.CreateChatTable();
            ChatServer.MainServerLoop();
            // не даёт программе завершиться раньше, чем закончит работу второй поток в классе ChatServer
            while (true) { }

        }
    }
}