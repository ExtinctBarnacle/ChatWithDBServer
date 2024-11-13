using ChatWithDBServer;
using System;
using System.Data.SQLite;

class ChatDBService
{
    // Путь к файлу базы данных (создаст файл, если он не существует)
    static string connectionString = "Data Source=mydatabase.db; Version=3;";
    public static void CreateChatTable()
    {
        

        // Создание базы данных и таблицы
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            // Создание таблицы
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT)";
            using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
    public static void StoreDataToDB(ChatMessage chatMessage) {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            // Вставка данных
            string insertQuery = "INSERT INTO Users (Name) VALUES (@name)";
            using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@name", "Юлия");
                command.ExecuteNonQuery();
            }
        }
    }
    public static void LoadChatTable() {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            // Чтение данных
            string selectQuery = "SELECT * FROM Users";
            using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}");
                    }
                }
            }
        }
    }
        
    
}