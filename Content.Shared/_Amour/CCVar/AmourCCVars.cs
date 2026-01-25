using Robust.Shared.Configuration;

namespace Content.Shared._Amour.CCVar;

[CVarDefs]
public sealed class AmourCCVars
{
    /// <summary>
    ///     URL of the Discord bot API for receiving OOC messages from Discord.
    /// </summary>
    public static readonly CVarDef<string> DiscordOocBotApiUrl =
        CVarDef.Create("discord.ooc_bot_api_url", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     Password for Discord bot OOC API authentication.
    /// </summary>
    public static readonly CVarDef<string> DiscordOocBotApiPassword =
        CVarDef.Create("discord.ooc_bot_api_password", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);
}