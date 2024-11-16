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
        static Dictionary<User, Boolean> users = new Dictionary<User, Boolean>(new UserEqualityComparer());

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
                        var msg = Encoding.UTF8.GetString(response.ToArray());

                        // маркер окончания - выходим из цикла
                        if (msg == "STOP") break;

                        if (msg == null) continue;

                        responseString = CheckMessageFromClient(msg);
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
        }
        static string CheckMessageFromClient(string msg)
        {
            // если пользователь онлайн
            if (msg.Substring(0, 2) == "ON") return SendNewMessageToUser(msg);
            // если пользователь офлайн
            if (msg.Substring(0, 2) == "OF")
            {
                User? user = JsonSerializer.Deserialize<User>(msg.Substring(2));
                if (user != null && users.ContainsKey(user))
                {
                    users[user] = false;
                    return "";
                }
            }
            // если пользователь подключился и запросил историю чата
            if (msg.Substring(0, 2) == "CH")
            {
                return JsonSerializer.Serialize(ChatHistory);
            }
            // если пользователь прислал сообщение в чат
            if (msg.Substring(0, 2) == "UM")
            {
                return AddNewMessageToHistory(msg);
            }
            return "";
        }
        
        static string SendNewMessageToUser(string msg) 
        {
            User? user = new User();
            user = JsonSerializer.Deserialize<User>(msg.Substring(2));
            string response = "";
            if (user != null)
                {
                    if (users.ContainsKey(user) == false)
                        {
                             users.Add(user, true);
                        }
                    // пересылаем этому пользователю новое сообщение в чате
                    if (NewMessagesIndex > -1 && ChatHistory[NewMessagesIndex].usersToReceive[user.Name] == false)
                        {
                            response = "NM" + JsonSerializer.Serialize(ChatHistory[NewMessagesIndex]); // prefix NM - New Message for client
                            ChatHistory[NewMessagesIndex].usersToReceive[user.Name] = true;
                            if (ChatHistory[NewMessagesIndex].isReceivedByAllUsers())
                                {
                                    if (NewMessagesIndex != ChatHistory.Length - 1) NewMessagesIndex++;
                                    else NewMessagesIndex = -1;
                                }
                                
                         }
                    
                }
            return response;
        }
        static string AddNewMessageToHistory(string msg)
        {
            ChatMessageModel? message = JsonSerializer.Deserialize<ChatMessageModel>(msg.Substring(2));
            ChatDBService.StoreDataToDB(message);
            message.usersToReceive = new Dictionary<string, Boolean>();
            foreach (User user in users.Keys)
            {
                if (!message.usersToReceive.ContainsKey(user.Name))
                {
                    message.usersToReceive.Add(user.Name, false);
                }
            }
            Array.Resize(ref ChatHistory, ChatHistory.Length + 1);
            ChatHistory[ChatHistory.Length - 1] = message;
            NewMessagesIndex = ChatHistory.Length - 1;
            if (!users.ContainsKey(message.user))
            {
                users.Add(message.user, true);
            }
            return msg;
        }            
    }
}