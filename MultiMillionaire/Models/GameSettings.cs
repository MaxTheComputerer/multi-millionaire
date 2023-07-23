using System.Net;

namespace MultiMillionaire.Models;

public class GameSettings
{
    public bool MuteHostSound { get; set; } = true;
    public bool MuteAudienceSound { get; set; } = true;
    public bool MuteSpectatorSound { get; set; }
    public bool AudienceAreSpectators { get; set; }
    public bool UseLifxLight { get; set; }
    public IPAddress LifxLightIp { get; set; } = IPAddress.Parse("38.242.174.238");
}
