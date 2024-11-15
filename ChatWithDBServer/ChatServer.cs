using ChatWithDBServer;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var tcpListener = new TcpListener(IPAddress.Any, 8080);
var users = new Dictionary<string, Boolean>();
ChatMessageModel newMessage = null;
try
{
    tcpListener.Start();    // запускаем сервер
    //Console.WriteLine("Сервер запущен. Ожидание подключений...");
    //ChatDBService.CreateChatTable();

    while (true)
    {
        // получаем подключение в виде TcpClient
        using var tcpClient = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
        // получаем объект NetworkStream для взаимодействия с клиентом
        var stream = tcpClient.GetStream();
        // буфер для входящих данных
        var response = new List<byte>();
        int bytesRead = 10;
        string responseString = "";
        while (true)
        {
            // считываем данные до конечного символа
            while ((bytesRead = stream.ReadByte()) != '\n')
            {
                // добавляем в буфер
                response.Add((byte)bytesRead);
            }
            var msg = Encoding.UTF8.GetString(response.ToArray());

            // маркер окончания
            // выходим из цикла
            if (msg == "STOP") break;

            //Console.WriteLine($"Server has got a message: {msg}");
            if (msg == null) continue;
            if (msg.Substring(0, 2) == "ON")
            {
                User user = JsonSerializer.Deserialize<User>(msg.Substring(2));
                if (user != null)
                {
                    if (users.ContainsKey(user.Name))
                        {
                        users.Add(user.Name, true);

                    }
                }
            }
            if (msg.Substring(0, 2) == "OF")
            {
                User user = JsonSerializer.Deserialize<User>(msg.Substring(2));
                if (user != null)
                {
                    if (users.ContainsKey(user.Name))
                    {
                        users.Add(user.Name, false);
                    }
                }
            }
            if (msg.Substring(0,2) == "CH")
            {
                string[] history = ChatDBService.LoadChatTable();
                responseString = JsonSerializer.Serialize(history);
            }
            if (msg.Substring(0, 2) == "UM")
            {
                ChatMessageModel message = JsonSerializer.Deserialize<ChatMessageModel>(msg.Substring(2));
                ChatDBService.StoreDataToDB(message);
                newMessage = message;
                responseString = msg;
                users.Add(message.user.Name, true);
            }
            // отправляем ответ сервера
            await stream.WriteAsync(Encoding.UTF8.GetBytes(responseString + "\n")).ConfigureAwait(false);
            response.Clear();
        }
    }
}
finally
{
    tcpListener.Stop();
}