using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Transactions;

var tcpListener = new TcpListener(IPAddress.Any, 8080);
//var words = new Dictionary<string, string>();
try
{
    tcpListener.Start();    // запускаем сервер
    Console.WriteLine("Сервер запущен. Ожидание подключений... ");
    ChatDBService.CreateChatTable();

    while (true)
    {
        // получаем подключение в виде TcpClient
        using var tcpClient = await tcpListener.AcceptTcpClientAsync();
        // получаем объект NetworkStream для взаимодействия с клиентом
        var stream = tcpClient.GetStream();
        // буфер для входящих данных
        var response = new List<byte>();
        int bytesRead = 10;
        int msgCount = 0;
        while (true)
        {
            // считываем данные до конечного символа
            while ((bytesRead = stream.ReadByte()) != '\n')
            {
                // добавляем в буфер
                response.Add((byte)bytesRead);
            }
            var word = Encoding.UTF8.GetString(response.ToArray());

            // если прислан маркер окончания взаимодействия,
            // выходим из цикла и завершаем взаимодействие с клиентом
            if (word == "END") break;

            Console.WriteLine($"Запрошен перевод слова {word}");
            // находим слово в словаре и отправляем обратно клиенту
            //if (!words.TryGetValue(word, out var translation)) translation = "не найдено в словаре";
            msgCount++;
            //words.Add(DateTime.Now.ToString() + " " + msgCount.ToString(), word);
            Console.WriteLine(DateTime.Now.ToString() + word);
            var translation = word + " message added to chat";
            // добавляем символ окончания сообщения 
            translation += '\n';
            // отправляем перевод слова из словаря
            await stream.WriteAsync(Encoding.UTF8.GetBytes(translation));
            response.Clear();
        }
    }
}
finally
{
    tcpListener.Stop();
}