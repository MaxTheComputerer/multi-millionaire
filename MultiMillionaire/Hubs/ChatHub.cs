using Microsoft.AspNetCore.SignalR;

namespace MultiMillionaire.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
    Task Send(string message);
}

public class ChatHub : Hub<IChatClient>
{
    public ChatHub()
    {
        TestList.Add("hello");
    }

    public static List<string> TestList { get; set; } = new();

    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine(TestList.Count);
        await Clients.All.ReceiveMessage(user, message);
    }

    public async Task AddToGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).Send($"{Context.ConnectionId} has joined the group {groupName}.");
    }

    public async Task RemoveFromGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).Send($"{Context.ConnectionId} has left the group {groupName}.");
    }

    public override async Task OnConnectedAsync()
    {
        await AddToGroup("Game1");
        await base.OnConnectedAsync();
    }
}
