﻿using MultiMillionaire.Models;

namespace MultiMillionaire.Services;

public interface IMultiplayerHubStorage
{
    public HashSet<MultiplayerGame> Games { get; }
    public HashSet<User> Users { get; }

    public User? GetUserById(string connectionId);
    public MultiplayerGame? GetGameById(string gameId);
}

public class MultiplayerHubStorage : IMultiplayerHubStorage
{
    public HashSet<MultiplayerGame> Games { get; } = new();
    public HashSet<User> Users { get; } = new();

    public User? GetUserById(string connectionId)
    {
        lock (Users)
        {
            return Users.SingleOrDefault(u => u.ConnectionId == connectionId);
        }
    }

    public MultiplayerGame? GetGameById(string gameId)
    {
        return Games.SingleOrDefault(g => g.Id == gameId);
    }
}