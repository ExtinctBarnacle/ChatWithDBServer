using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatWithDBServer
{
    public class ChatServer
    {

        static ChatMessageModel[] ChatHistory = new ChatMessageModel[1];
        // элемент в истории чата, с которого начинаются сообщения, разосланные не всем пользователям
        static int NewMessagesIndex = -1;

        // массив подключённых пользователей, булево значение - онлайн или нет
        static Dictionary<User, Boolean> users = new Dictionary<User, Boolean>(new UserEqualityComparer());

        public async void MainServerLoop()
        {
            var tcpListener = new TcpListener(IPAddress.Any, 8080);
            ChatHistory = ChatDBService.LoadChatTable();
            //NewMessagesIndex = ChatHistory.Length-1;
            try
            {
                string responseString = "";
                tcpListener.Start();    // запускаем сервер
                                        Console.WriteLine("Сервер запущен. Ожидание подключений...");
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
                            if (response.Count> 1000000) response.Clear();
                        }
                        var msg = Encoding.UTF8.GetString(response.ToArray());

                        // маркер окончания - выходим из цикла
                        if (msg == "STOP") break;

                        //Console.WriteLine($"Server has got a message: {msg}");
                        if (msg == null) continue;
                        // если этот пользователь онлайн
                        if (msg.Substring(0, 2) == "ON")
                        {
                            User? user = JsonSerializer.Deserialize<User>(msg.Substring(2));
                            if (user != null)
                            {
                                if (users.ContainsKey(user) == false)
                                {
                                    users.Add(user, true);
                                }
                                // пересылаем этому пользователю новое сообщение в чате
                                if (NewMessagesIndex > -1 && ChatHistory[NewMessagesIndex].usersToReceive[user.Name] == false)
                                {
                                    responseString = "NM" + JsonSerializer.Serialize(ChatHistory[NewMessagesIndex]);
                                    ChatHistory[NewMessagesIndex].usersToReceive[user.Name] = true;
                                    if (ChatHistory[NewMessagesIndex].isReceivedByAllUsers())
                                    {
                                        if (NewMessagesIndex != ChatHistory.Length - 1)
                                            NewMessagesIndex++;
                                        else NewMessagesIndex = -1;
                                    }
                                }
                            }
                        }
                        // если этот пользователь офлайн
                        if (msg.Substring(0, 2) == "OF")
                        {
                            User? user = JsonSerializer.Deserialize<User>(msg.Substring(2));
                            if (user != null)
                            {
                                if (users.ContainsKey(user))
                                {
                                    users[user] = false;
                                }
                            }
                        }
                        // если этот пользователь подключился и запросил историю чата
                        if (msg.Substring(0, 2) == "CH")
                        {
                            responseString = JsonSerializer.Serialize(ChatHistory);
                        }
                        // если этот пользователь прислал сообщение в чат
                        if (msg.Substring(0, 2) == "UM")
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
                            responseString = msg;
                            if (!users.ContainsKey(message.user))
                            {
                                users.Add(message.user, true);
                            }
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
        }
    }
}