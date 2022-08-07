namespace MultiMillionaire.Models;

public class MultiplayerGame
{
    public int Id { get; set; }
    public User Host { get; set; }

    public MultiplayerGame(User host)
    {
        Host = host;
    }
}