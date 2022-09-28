using Microsoft.AspNetCore.SignalR;
using MultiMillionaire.Models;

namespace MultiMillionaire.Hubs;

public interface IMultiplayerGameHub
{
    Task Message(string message);
    Task JoinSuccessful(string gameId);
    Task PopulatePlayerList(IEnumerable<UserViewModel> players);
    Task PlayerJoined(UserViewModel player);
    Task PlayerLeft(UserViewModel player);
}

public class MultiplayerGameHub : Hub<IMultiplayerGameHub>
{
    private static List<MultiplayerGame> Games { get; } = new();
    private static List<User> Users { get; } = new();

    #region MiscellaneousMethods

    public async Task Echo(string message)
    {
        await Clients.Caller.Message(message);
    }

    #endregion


    #region UserMethods

    private User? GetCurrentUser()
    {
        lock (Users)
        {
            return Users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        lock (Users)
        {
            if (GetCurrentUser() == null) Users.Add(new User(Context.ConnectionId));
        }

        Console.WriteLine($"User connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = GetCurrentUser();
        if (user != null)
        {
            await LeaveGame();
            Users.Remove(user);
        }

        Console.WriteLine($"User disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    private async Task AddUserToGame(User user, MultiplayerGame game, UserRole role)
    {
        await Groups.AddToGroupAsync(user.ConnectionId, game.Id);
        user.Game = game;
        user.Role = role;
        switch (role)
        {
            case UserRole.Audience:
                game.Audience.Add(user);
                break;
            case UserRole.Spectator:
                game.Spectators.Add(user);
                break;
            case UserRole.Host:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }

    private async Task RemoveUserFromGame(User user)
    {
        if (user.Game == null) return;
        await Groups.RemoveFromGroupAsync(user.ConnectionId, user.Game.Id);
        user.Game = null;
    }

    public void SetName(string name)
    {
        var user = GetCurrentUser();
        if (user == null) return;

        user.Name = name;
        Console.WriteLine($"Registered player: {name}");
    }

    #endregion

    #region GameMethods

    private static MultiplayerGame? GetGameById(string gameId)
    {
        return Games.SingleOrDefault(g => g.Id == gameId);
    }

    private static MultiplayerGame CreateGame(User host)
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

    public async Task HostGame()
    {
        var user = GetCurrentUser();
        if (user == null || user.Name == null) return;

        // Create game
        var game = CreateGame(user);
        Games.Add(game);

        // Join game
        await LeaveGame();
        await AddUserToGame(user, game, UserRole.Host);
        await JoinSuccessful(game);
    }

    public async Task JoinGameAudience(string gameId)
    {
        var user = GetCurrentUser();
        if (user == null || user.Name == null) return;

        var game = GetGameById(gameId);
        if (game == null || game.Audience.Contains(user)) return;

        // Join game
        await LeaveGame();
        await AddUserToGame(user, game, UserRole.Audience);

        await Clients.OthersInGroup(game.Id).PlayerJoined(user.ToViewModel());
        await Clients.OthersInGroup(game.Id).Message($"{user.Name} has joined the game.");
        await JoinSuccessful(game);
    }

    public async Task SpectateGame(string gameId)
    {
        var user = GetCurrentUser();
        if (user == null) return;

        var game = GetGameById(gameId);
        if (game == null || game.Audience.Contains(user)) return;

        // Join game
        await LeaveGame();
        await AddUserToGame(user, game, UserRole.Spectator);
        await JoinSuccessful(game);
    }

    private async Task LeaveGame()
    {
        var user = GetCurrentUser();
        if (user == null) return;

        var game = user.Game;
        if (game != null)
        {
            if (game.Host == user) await EndGame(game);

            await RemoveUserFromGame(user);
            if (user.Name != null) await Clients.Group(game.Id).PlayerLeft(user.ToViewModel());
            await Clients.OthersInGroup(game.Id).Message($"{user.Name} has left the game.");
            await Clients.Caller.Message($"You have left game {game.Id}");
        }
    }

    private async Task EndGame(MultiplayerGame game)
    {
        await Clients.Group(game.Id).Message("Game ended by host");

        foreach (var user in game.Audience) await RemoveUserFromGame(user);
        foreach (var user in game.Spectators) await RemoveUserFromGame(user);
        await RemoveUserFromGame(game.Host);

        Games.Remove(game);
    }

    private async Task JoinSuccessful(MultiplayerGame game)
    {
        await Clients.Caller.Message($"Welcome to game {game.Id}. Your host is {game.Host.Name}.");
        await Clients.Caller.PopulatePlayerList(game.GetPlayers().Select(u => u.ToViewModel()));
        await Clients.Caller.JoinSuccessful(game.Id);
    }

    #endregion
}