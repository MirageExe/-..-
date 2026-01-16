// SPDX-FileCopyrightText: 2025 Amour
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._Amour.TTS;
using Content.Shared._Amour.TTS;
using Content.Shared.Humanoid;
using System.Linq;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private List<TTSVoicePrototype> _ttsPrototypes = new();
    private TTSVoiceOptionButton? _ttsVoiceButton;

    private void InitializeTTSVoice()
    {
        var parent = TTSVoiceButton.Parent;
        if (parent == null)
            return;

        var index = -1;
        for (var i = 0; i < parent.ChildCount; i++)
        {
            if (parent.GetChild(i) == TTSVoiceButton)
            {
                index = i;
                break;
            }
        }

        _ttsVoiceButton = new TTSVoiceOptionButton
        {
            HorizontalAlignment = TTSVoiceButton.HorizontalAlignment
        };

        if (index >= 0)
        {
            parent.RemoveChild(TTSVoiceButton);
            parent.AddChild(_ttsVoiceButton);
            _ttsVoiceButton.SetPositionInParent(index);
        }

        _ttsVoiceButton.OnItemSelected += args =>
        {
            _ttsVoiceButton.SelectId(args.Id);
            SetTTSVoice(_ttsPrototypes[args.Id]);
        };

        TTSVoicePlayButton.OnPressed += _ => PlayPreviewTTS();
    }

    private void UpdateTTSVoice()
    {
        if (Profile is null || _ttsVoiceButton is null)
            return;

        var profileSex = Profile.Sex;

        _ttsPrototypes = _prototypeManager
            .EnumeratePrototypes<TTSVoicePrototype>()
            .Where(o => o.RoundStart)
            .OrderBy(o => o.Sex == profileSex ? 0 : (o.Sex == Sex.Unsexed ? 1 : 2))
            .ThenBy(o => Loc.GetString(o.Name))
            .ToList();

        _ttsVoiceButton.Clear();

        var selectedTTSId = -1;
        for (var i = 0; i < _ttsPrototypes.Count; i++)
        {
            var voice = _ttsPrototypes[i];
            if (voice.ID == Profile.Voice)
                selectedTTSId = i;

            _ttsVoiceButton.AddVoiceItem(Loc.GetString(voice.Name), voice.Sex, i);
        }

        if (selectedTTSId == -1)
            selectedTTSId = 0;

        _ttsVoiceButton.SelectId(selectedTTSId);
        if (_ttsPrototypes.Count > 0)
            SetTTSVoice(_ttsPrototypes[selectedTTSId]);
    }

    private void PlayPreviewTTS()
    {
        if (Profile is null)
            return;

        var ttsSystem = _entManager.System<TTSSystem>();
        ttsSystem.RequestGlobalTTS(VoiceRequestType.Preview, Profile.Voice);
    }
}
