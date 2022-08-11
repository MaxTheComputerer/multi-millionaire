using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using MultiMillionaire.Models;

namespace MultiMillionaire.Hubs;

public interface IMultiplayerGameHub
{
    Task Message(string message);
}

public class MultiplayerGameHub : Hub<IMultiplayerGameHub>
{
    private static List<MultiplayerGame> Games { get; } = new();
    private static List<User> Users { get; } = new();


    #region UserMethods

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await RemoveUserFromGame();
        lock (Users)
        {
            var user = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                Users.Remove(user);
                Console.WriteLine($"User disconnected: {user.Name}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private User? GetCurrentUser()
    {
        lock (Users)
        {
            return Users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);
        }
    }

    public void AddUser(string name)
    {
        name = Regex.Replace(name, @"\s+", "");
        User user;
        lock (Users)
        {
            if (GetCurrentUser() != null) return;

            user = new User(Context.ConnectionId, name);
            Users.Add(user);
        }

        Console.WriteLine($"Registered user: {user.Name}");
    }

    public async Task RemoveUserFromGame()
    {
        var user = GetCurrentUser();
        if (user == null) return;

        var game = user.Game;
        if (game != null)
        {
            if (game.Host == user)
            {
                // End game
            }

            await Groups.RemoveFromGroupAsync(user.ConnectionId, game.Id);
            await Clients.Group(game.Id).Message($"{user.Name} has left the game");
        }
    }

    #endregion

    #region GameMethods

    public static MultiplayerGame? GetGameById(string gameId)
    {
        return Games.SingleOrDefault(g => g.Id == gameId);
    }

    public MultiplayerGame CreateGame(User host)
    {
        // Make sure ID is unique
        string gameId;
        do
        {
            gameId = MultiplayerGame.GenerateRoomId();
        } while (Games.Any(g => g.Id == gameId));

        return new MultiplayerGame
        {
            Id = gameId,
            Host = host
        };
    }

    public async Task JoinGameRoom(string gameId)
    {
        var user = GetCurrentUser();
        if (user == null) return;

        var game = GetGameById(gameId);
        if (game == null) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        await Clients.OthersInGroup(gameId).Message($"{user.Name} has joined the game");
        await Clients.Caller.Message($"Welcome to game {gameId}. Your host is {game.Host.Name}");
    }

    public async Task HostGame()
    {
        await RemoveUserFromGame();

        var user = GetCurrentUser();
        if (user == null) return;

        var game = CreateGame(user);
        Games.Add(game);

        user.Game = game;
        await JoinGameRoom(game.Id);
    }

    public async Task SpectateGame(string gameId)
    {
        await RemoveUserFromGame();

        var user = GetCurrentUser();
        if (user == null) return;

        var game = GetGameById(gameId);
        if (game == null) return;

        if (game.Spectators.Contains(user)) return;

        user.Game = game;
        game.Spectators.Add(user);
        await JoinGameRoom(game.Id);
    }

    #endregion


    #region MiscellaneousMethods

    public async Task Echo(string message)
    {
        await Clients.Caller.Message(message);
    }

    #endregion
}