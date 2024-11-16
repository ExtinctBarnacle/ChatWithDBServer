using ChatWithDBServer;
using System.Data.SQLite;

class ChatDBService
{
    // файл базы данных
    static string connectionString = "Data Source=mydatabase.db; Version=3;";

    public static void CreateChatTable()
    {
        // Создание базы данных и таблицы
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string createTableQuery = "CREATE TABLE IF NOT EXISTS ChatHistory (Id INTEGER PRIMARY KEY AUTOINCREMENT, text TEXT, DateTimeStamp TEXT, user INTEGER)";
            using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
            createTableQuery = "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, IP TEXT)";
            using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        string dbFilePath = System.IO.Path.GetFullPath("mydatabase.db");
        Console.WriteLine($"Database file is located at: {dbFilePath}");
    }
    public static void StoreDataToDB(ChatMessageModel chatMessage) {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string insertQuery = "INSERT INTO ChatHistory (text, DateTimeStamp, user) VALUES (@text, @DateTimeStamp, @user)";
            using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@text", chatMessage.Text);
                command.Parameters.AddWithValue("@DateTimeStamp", chatMessage.DateTimeStamp);
                command.Parameters.AddWithValue("@user", chatMessage.user.Name);
                command.ExecuteNonQuery();
            }
        }
    }
    public static ChatMessageModel[] LoadChatTable() {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
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
                        chat[^1].user = new User();
                        chat[^1].user.Name = (string) reader[3];
                        Array.Resize(ref chat, chat.Length + 1);
                    }
                    Array.Resize(ref chat, chat.Length - 1);
                    return chat;
                }
            }
        }
    }
        
    
}