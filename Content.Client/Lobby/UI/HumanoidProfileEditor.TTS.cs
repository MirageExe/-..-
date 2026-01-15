// SPDX-FileCopyrightText: 2025 Amour
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Amour.TTS;
using System.Linq;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private List<TTSVoicePrototype> _ttsPrototypes = new();

    private void InitializeTTSVoice()
    {
        TTSVoiceButton.OnItemSelected += args =>
        {
            TTSVoiceButton.SelectId(args.Id);
            SetTTSVoice(_ttsPrototypes[args.Id]);
        };

        TTSVoicePlayButton.OnPressed += _ => PlayPreviewTTS();
    }

    private void UpdateTTSVoice()
    {
        if (Profile is null)
            return;

        _ttsPrototypes = _prototypeManager
            .EnumeratePrototypes<TTSVoicePrototype>()
            .Where(o => o.RoundStart)
            .OrderBy(o => Loc.GetString(o.Name))
            .ToList();

        TTSVoiceButton.Clear();

        var selectedTTSId = -1;
        for (var i = 0; i < _ttsPrototypes.Count; i++)
        {
            var voice = _ttsPrototypes[i];
            if (voice.ID == Profile.Voice)
                selectedTTSId = i;

            TTSVoiceButton.AddItem(Loc.GetString(voice.Name), i);
        }

        if (selectedTTSId == -1)
            selectedTTSId = 0;

        TTSVoiceButton.SelectId(selectedTTSId);
        if (_ttsPrototypes.Count > 0)
            SetTTSVoice(_ttsPrototypes[selectedTTSId]);
    }

    private void PlayPreviewTTS()
    {
        if (Profile is null)
            return;

        // TODO: Implement TTS preview playback
        // var ev = new PreviewTTSEvent(Profile.Voice);
        // _entManager.EventBus.RaiseEvent(EventSource.Local, ref ev);
    }
}
