namespace MultiMillionaire.Models;

public class GameSettings
{
    public bool MuteHostSound { get; set; } = true;
    public bool MuteAudienceSound { get; set; } = true;
    public bool MuteSpectatorSound { get; set; }
    public bool AudienceAreSpectators { get; set; } = true;
}