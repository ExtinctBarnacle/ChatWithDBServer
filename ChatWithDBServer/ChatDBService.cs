using ChatWithDBServer;
using System.Data.SQLite;
using System.Net.Security;

class ChatDBService
{
    // файл базы данных
    static string СonnectionString = "Data Source=mydatabase.db; Version=3;";

    // метод создаёт таблицу ChatHistory (история переписки в чате), если в БД mydatabase.db такой таблицы нет
    public static void CreateChatTable()
    {
        using (SQLiteConnection connection = new SQLiteConnection(СonnectionString))
        {
            connection.Open();
            string createTableQuery = "CREATE TABLE IF NOT EXISTS ChatHistory (Id INTEGER PRIMARY KEY AUTOINCREMENT, text TEXT, DateTimeStamp TEXT, user TEXT)";
            using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    // метод сохраняет в таблицу ChatHistory очередное сообщение чата, присланное клиентом
    public static void StoreDataToDB(ChatMessageModel chatMessage) {
        using (SQLiteConnection connection = new SQLiteConnection(СonnectionString))
        {
            if (chatMessage.User.Name == null) chatMessage.User.Name = string.Empty;
            connection.Open();
            string insertQuery = "INSERT INTO ChatHistory (text, DateTimeStamp, user) VALUES (@text, @DateTimeStamp, @user)";
            using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@text", chatMessage.Text);
                command.Parameters.AddWithValue("@DateTimeStamp", chatMessage.DateTimeStamp);
                command.Parameters.AddWithValue("@user", chatMessage.User.Name);
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
    
    // метод читает из БД и возвращает массив объектов сообщений - историю переписки в чате
    public static ChatMessageModel[] LoadChatTable() {
        using (SQLiteConnection connection = new SQLiteConnection(СonnectionString))
        {
            connection.Open();
            string selectQuery = "SELECT * FROM ChatHistory";
            using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    ChatMessageModel[] chat = new ChatMessageModel[1];
                    while (reader.Read())
                    {
                        chat[^1] = new ChatMessageModel();
                        chat[^1].Text = (string) reader[1];
                        chat[^1].DateTimeStamp = (string) reader[2];
                        chat[^1].User = new User();
                        chat[^1].User.Name = (string) reader[3];
                        Array.Resize(ref chat, chat.Length + 1);
                    }
                    Array.Resize(ref chat, chat.Length - 1);
                    connection.Close();
                    return chat;
                }
            }
            
        }
    }
}