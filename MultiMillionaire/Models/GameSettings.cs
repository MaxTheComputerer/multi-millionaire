using System.Net;

namespace MultiMillionaire.Models;

public class GameSettings
{
    public bool MuteHostSound { get; private set; } = true;
    public bool MuteAudienceSound { get; private set; } = true;
    public bool MuteSpectatorSound { get; private set; }
    public bool AudienceAreSpectators { get; private set; }
    public bool UseLifxLight { get; private set; }
    public IPAddress LifxLightIp { get; private set; } = IPAddress.Parse("192.168.0.9");

    public bool TryUpdateSwitchSetting(string settingName, bool value)
    {
        switch (settingName)
        {
            case "audienceAreSpectators":
                AudienceAreSpectators = value;
                break;
            case "muteHostSound":
                MuteHostSound = value;
                break;
            case "muteAudienceSound":
                MuteAudienceSound = value;
                break;
            case "muteSpectatorSound":
                MuteSpectatorSound = value;
                break;
            case "useLifxLight":
                UseLifxLight = value;
                break;
            default:
                return false;
        }

        return true;
    }

    public bool TryUpdateTextSetting(string settingName, string value)
    {
        switch (settingName)
        {
            case "lifxLightIp":
                var parseSuccess = IPAddress.TryParse(value, out var parsedAddress);
                if (parseSuccess) LifxLightIp = parsedAddress!;
                break;
            default:
                return false;
        }

        return true;
    }
}