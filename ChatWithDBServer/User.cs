﻿namespace ChatWithDBServer
{
    internal class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public Boolean LastMessageReceived { get; set; }
    }
}