using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatWithDBServer
{
    public class ChatServer
    {
        // история чата - массив сообщений
        static ChatMessageModel[] ChatHistory = new ChatMessageModel[1];

        // элемент в истории чата, с которого начинаются сообщения, разосланные не всем пользователям
        static int NewMessagesIndex = -1;

        // массив подключённых пользователей, булево значение - онлайн или нет
        static Dictionary<User, Boolean> Users = new Dictionary<User, Boolean>(new UserEqualityComparer());

        public async void MainServerLoop()
        {
            ChatHistory = ChatDBService.LoadChatTable();
            var tcpListener = new TcpListener(IPAddress.Any, 8080);
            try
            {
                string responseString = "";
                tcpListener.Start();    // запуск сервера
                Console.WriteLine("Server is running. Wait for connections...");
                while (true)
                {
                    // получаем подключение в виде TcpClient
                    var tcpClient = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    // получаем объект NetworkStream для взаимодействия с клиентом
                    var stream = tcpClient.GetStream();
                    // буфер для входящих данных
                    var response = new List<byte>();
                    int bytesRead = 10;

                    while (true)
                    {
                        // считываем данные до конечного символа
                        while ((bytesRead = stream.ReadByte()) != '\n')
                        {
                            // добавляем в буфер
                            response.Add((byte)bytesRead);
                            if (response.Count > 1000000) response.Clear();
                        }
                        var message = Encoding.UTF8.GetString(response.ToArray());
                        // маркер окончания - выходим из цикла
                        if (message == "STOP") break;
                        if (message == null) continue;
                        try
                        {
                            responseString = CheckMessageFromClient(message);
                            await stream.WriteAsync(Encoding.UTF8.GetBytes(responseString + "\n")).ConfigureAwait(false);
                            response.Clear();
                        }
                        catch (ArgumentException e) { 
                            Console.WriteLine(e.ToString());
                        }
                        // отправляем ответ сервера
                        
                    }
                }
            }
            finally
            {
                tcpListener.Stop();
            }
        }
        static string CheckMessageFromClient(string message)
        {
            if (message.Length < 2) throw new ArgumentException("Клиент прислал некорректный запрос - невозможно прочитать.");
            // если пользователь онлайн
            if (message.Substring(0, 2) == "ON") return SendNewMessageToUser(message);
            // если пользователь офлайн
            if (message.Substring(0, 2) == "OF")
            {
                User? user = JsonSerializer.Deserialize<User>(message.Substring(2));
                if (user != null && Users.ContainsKey(user))
                {
                    Users[user] = false;
                    return "";
                }
            }
            // если пользователь подключился и запросил историю чата
            if (message.Substring(0, 2) == "CH")
            {
                return JsonSerializer.Serialize(ChatHistory);
            }
            // если пользователь прислал сообщение в чат
            if (message.Substring(0, 2) == "UM")
            {
                return AddNewMessageToHistory(message);
            }
            return "";
        }
        
        static string SendNewMessageToUser(string message) 
        {
            User? user = new User();
            user = JsonSerializer.Deserialize<User>(message.Substring(2));
            string response = "";
            if (user != null)
                {
                    if (Users.ContainsKey(user) == false)
                        {
                             Users.Add(user, true);
                        }
                    // пересылаем этому пользователю новое сообщение в чате
                    if (NewMessagesIndex > -1 && ChatHistory[NewMessagesIndex].UsersToReceive[user.Name] == false)
                        {
                            response = "NM" + JsonSerializer.Serialize(ChatHistory[NewMessagesIndex]); // prefix NM - New Message for client
                            ChatHistory[NewMessagesIndex].UsersToReceive[user.Name] = true;
                            if (ChatHistory[NewMessagesIndex].IsReceivedByAllUsers())
                                {
                                    if (NewMessagesIndex != ChatHistory.Length - 1) NewMessagesIndex++;
                                    else NewMessagesIndex = -1;
                                }
                        }
                }
            return response;
        }
        static string AddNewMessageToHistory(string message)
        {
            ChatMessageModel? messageObj = JsonSerializer.Deserialize<ChatMessageModel>(message.Substring(2));
            ChatDBService.StoreDataToDB(messageObj);
            messageObj.UsersToReceive = new Dictionary<string, Boolean>();
            foreach (User user in Users.Keys)
            {
                if (!messageObj.UsersToReceive.ContainsKey(user.Name))
                {
                    messageObj.UsersToReceive.Add(user.Name, false);
                }
            }
            Array.Resize(ref ChatHistory, ChatHistory.Length + 1);
            ChatHistory[ChatHistory.Length - 1] = messageObj;
            NewMessagesIndex = ChatHistory.Length - 1;
            if (!Users.ContainsKey(messageObj.User))
            {
                Users.Add(messageObj.User, true);
            }
            return message;
        }            
    }
}