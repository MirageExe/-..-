using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Content.Server.Chat.Managers;
using Content.Shared._Amour.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Amour.DiscordOoc;

/// <summary>
/// Fetches OOC messages from Discord bot and sends them in-game.
/// </summary>
public sealed class DiscordOocReceiverSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly HttpClient _httpClient = new();
    
    private string _apiUrl = string.Empty;
    private string _apiPassword = string.Empty;
    private bool _enabled;
    
    private TimeSpan _lastFetch;
    private readonly TimeSpan _fetchInterval = TimeSpan.FromSeconds(1);
    
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("discord.ooc");
        
        Subs.CVar(_cfg, AmourCCVars.DiscordOocBotApiUrl, url =>
        {
            _apiUrl = url;
            _enabled = !string.IsNullOrEmpty(url);
        }, true);
        
        Subs.CVar(_cfg, AmourCCVars.DiscordOocBotApiPassword, pass => _apiPassword = pass, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_enabled)
            return;

        var curTime = _timing.CurTime;
        if (curTime - _lastFetch < _fetchInterval)
            return;

        _lastFetch = curTime;
        
        // Fire and forget - don't block game loop
        _ = FetchMessagesAsync();
    }

    private async Task FetchMessagesAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/ooc/pending");
            request.Headers.Add("Authorization", _apiPassword);
            
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    _sawmill.Warning($"Failed to fetch Discord OOC: {response.StatusCode}");
                return;
            }

            var messages = await response.Content.ReadFromJsonAsync<List<DiscordOocMessage>>();
            
            if (messages == null || messages.Count == 0)
                return;

            foreach (var msg in messages)
            {
                SendDiscordOocInGame(msg.Sender, msg.Content);
            }
        }
        catch (HttpRequestException)
        {
            // Bot is offline - silently ignore
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Error fetching Discord OOC: {ex.Message}");
        }
    }

    private void SendDiscordOocInGame(string sender, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        // Format: DS: (PlayerName): message
        var message = $"DS: ({sender}): {content}";
        
        _chat.DispatchServerMessage(message);
        _sawmill.Info($"Discord OOC: {sender}: {content}");
    }

    private sealed class DiscordOocMessage
    {
        [JsonPropertyName("Sender")]
        public string Sender { get; set; } = string.Empty;
        
        [JsonPropertyName("Content")]
        public string Content { get; set; } = string.Empty;
    }
}