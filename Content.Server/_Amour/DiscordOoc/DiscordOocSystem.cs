using Content.Server.Discord;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server._Amour.DiscordOoc;

/// <summary>
/// Handles OOC chat relay to/from Discord.
/// </summary>
public sealed class DiscordOocSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DiscordWebhook _discord = default!;

    private string _webhookUrl = string.Empty;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("discord.ooc");
        
        Subs.CVar(_cfg, CCVars.DiscordOocWebhook, url => _webhookUrl = url, true);
    }

    /// <summary>
    /// Sends an OOC message to Discord.
    /// </summary>
    public async void SendOocToDiscord(string playerName, string message)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
            return;

        try
        {
            // Sanitize message
            var sanitizedMessage = message.Replace("@", "@\u200B"); // Prevent pings
            
            var payload = new WebhookPayload
            {
                Username = playerName,
                Content = sanitizedMessage,
                AllowedMentions = new Dictionary<string, string[]>
                {
                    { "parse", Array.Empty<string>() } // No mentions allowed
                }
            };

            await _discord.CreateMessage(_webhookUrl, payload);
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to send OOC to Discord: {ex.Message}");
        }
    }
}