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
            // �� ��� ��������� ����������� ������, ��� �������� ������ ������ ����� � ������ ChatServer
            while (true) { }

        }
    }
}