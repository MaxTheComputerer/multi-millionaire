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
    private static List<MultiplayerGame> _games = new();
    private static readonly List<User> _users = new();


    // User methods

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_users)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                _users.Remove(user);
                Console.WriteLine($"User disconnected: {user.Name}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private User? GetCurrentUser()
    {
        return _users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);
    }

    public void AddUser(string name)
    {
        name = Regex.Replace(name, @"\s+", "");
        User user;
        lock (_users)
        {
            if (GetCurrentUser() != null) return;

            user = new User(Context.ConnectionId, name);
            _users.Add(user);
        }

        Console.WriteLine($"Registered user: {user.Name}");
    }


    // Misc

    public async Task Echo(string message)
    {
        await Clients.Caller.Message(message);
    }
}