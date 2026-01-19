// SPDX-FileCopyrightText: 2026 Amour
// SPDX-License-Identifier: AGPL-3.0-or-later


using Content.Shared._EinsteinEngines.Language;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Systems;

public partial class ChatSystem
{
    /// <summary>
    /// Sends a quiet emote. Speech bubble visible to all in VoiceRange,
    /// but chat message only appears within QuietEmoteRange (2 tiles).
    /// </summary>
    private void SendEntityQuietEmote(
        EntityUid source,
        string action,
        ChatTransmitRange range,
        string? nameOverride,
        LanguagePrototype language,
        bool hideLog = false,
        bool checkEmote = true,
        bool ignoreActionBlocker = false)
    {
        if (!_actionBlocker.CanEmote(source) && !ignoreActionBlocker)
            return;

        _emoteProtection.OnEmoteDetected(source, action, voluntary: true);

        var ent = Identity.Entity(source, EntityManager);
        var name = FormattedMessage.EscapeText(nameOverride ?? Name(ent));
        action = FormattedMessage.RemoveMarkupOrThrow(action);

        if (checkEmote)
            TryEmoteChatInput(source, action);

        var wrappedMessage = Loc.GetString("chat-manager-entity-me-wrap-message",
            ("entityName", name),
            ("entity", ent),
            ("message", FormattedMessage.EscapeText(action)));

        foreach (var (session, data) in GetRecipients(source, VoiceRange))
        {
            if (session.AttachedEntity is not { Valid: true } listener)
                continue;

            if (EmoteRespectsLOS && !data.Observer && !_examineSystem.InRangeUnOccluded(source, listener, VoiceRange))
                continue;

            var result = MessageRangeCheck(session, data, range);
            if (result == MessageRangeCheckResult.Disallowed)
                continue;

            // Show in chat only if within QuietEmoteRange and has LOS
            var hideChat = data.Range > QuietEmoteRange
                || (EmoteRespectsLOS && !data.Observer && !_examineSystem.InRangeUnOccluded(source, listener, QuietEmoteRange))
                || result == MessageRangeCheckResult.HideChat;

            _chatManager.ChatMessageToOne(ChatChannel.Emotes, action, wrappedMessage, source, hideChat, session.Channel);
        }

        if (!hideLog && HasComp<ActorComponent>(source))
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Quiet emote from {ToPrettyString(source):user}: {action}");

        _replay.RecordServerMessage(new ChatMessage(ChatChannel.Emotes, action, wrappedMessage, GetNetEntity(source), null, MessageRangeHideChatForReplay(range)));
    }
}
