﻿namespace MultiMillionaire.Models;

public class User
{
    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public MultiplayerGame? Game { get; set; }

    public User(string connectionId, string name)
    {
        ConnectionId = connectionId;
        Name = name;
    }
}