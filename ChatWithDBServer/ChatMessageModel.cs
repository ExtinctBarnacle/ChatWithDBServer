namespace ChatWithDBServer
{
    //класс сообщения - хранит текст сообщения, дату и время отправки, ссылку на отправителя и булевый список пользователей, которые должны получить сообщение
    internal class ChatMessageModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string DateTimeStamp { get; set; }
        public User user { get; set; }
        // массив содержит пары: пользователь - получил / не получил данное сообщение
        public Dictionary<string, Boolean> usersToReceive { get; set; }

        // проверка, все ли пользователи получили данное сообщение
        public Boolean isReceivedByAllUsers()
                {
            foreach (var user in usersToReceive)
            {
                if (user.Value == false) return false;
            }
            return true;
        }
    }
}
