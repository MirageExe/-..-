using Content.Server.Discord;
using Content.Shared._Amour.CCVar;
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
    private WebhookIdentifier? _webhookId;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("discord.ooc");
        
        Subs.CVar(_cfg, AmourCCVars.DiscordOocWebhookUrl, url =>
        {
            _webhookUrl = url;
            _webhookId = null;

            if (!string.IsNullOrWhiteSpace(url))
                _discord.TryGetWebhook(url, data => _webhookId = data.ToIdentifier());
        }, true);
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
            
            if (_webhookId is not { } webhookId)
                return;

            var mentions = new WebhookMentions();
            // Do not add anything to mentions.Parse => no mentions allowed.

            var payload = new WebhookPayload
            {
                Username = playerName,
                Content = sanitizedMessage,
                AllowedMentions = mentions
            };

            await _discord.CreateMessage(webhookId, payload);
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to send OOC to Discord: {ex.Message}");
        }
    }
}