using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameInfo", menuName = "microMix/GameInfo", order = 1)]
public class GameInfo : ScriptableObject
{
    public bool includeInRandomizer = true;

    [HideInInspector] public int teamNumber;

    public string gameTitle;

    [TexturePreview(3)]
    public Sprite gameIcon;

    [FormerlySerializedAs("prompt"), Delayed]
    public string announcerText;

    [HideInInspector] public string announcerAudioFile;

 #if UNITY_EDITOR
    // Handy for dragging in the right file in the inspector,
    // but not deserializable in game, so we use these to save paths.
    [SerializeField] AudioClip _announcerAudio;
    public UnityEditor.SceneAsset singlePlayerScene;
    public UnityEditor.SceneAsset twoPlayerScene;

    string GetScenePath(UnityEditor.SceneAsset scene) {
        if (scene == null) return string.Empty;
        return UnityEditor.AssetDatabase.GetAssetPath(scene);
    }

//@"(.+?)(\w[Vv]\d+)\w?\-\w*\d+"
    static readonly System.Text.RegularExpressions.Regex AUDIO_TEXT_EXTRACTOR = new(@"(.+?)(V?\d+)?\s*-RX10", System.Text.RegularExpressions.RegexOptions.Compiled);
    static readonly System.Text.RegularExpressions.Regex TEAM_EXTRACTOR = new(@"Team[\s-_:#]*(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled);


    static string CanonizeString(string text) {
        return text.Replace('_', ' ').Trim().Replace(" and ", " & ").Replace(" The ", " the ");
    }

    static string ExtractTextFromFileName(string fileName) {
        var match = AUDIO_TEXT_EXTRACTOR.Match(fileName);
        if (match.Success) {
            return CanonizeString(match.Groups[1].Value);
        }
        return CanonizeString(fileName);
    }

    [ContextMenu("Test audio clip name extraction")]
    void TestAudioNameExtraction() {
        var clips = DataHelper.GetAllAssetsOfType<AudioClip>("Assets/Resources");
        foreach(var clip in clips) {
            Debug.Log($"'{clip.name}'\t->\t'{ExtractTextFromFileName(clip.name)}'");
        }
    }

    string _warnedAboutText;

    void OnValidate() {
        DataHelper.MarkChangesForSaving(this, "Update GameInfo");

        if (teamNumber < 1) {
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            var match = TEAM_EXTRACTOR.Match(path);
            if (match.Success) {
                teamNumber = int.Parse(match.Groups[1].Value);
            }
        }

        singlePlayerScenePath = GetScenePath(singlePlayerScene);
        twoPlayerScenePath = GetScenePath(twoPlayerScene);

        if (_announcerAudio == null && !string.IsNullOrEmpty(announcerAudioFile)) {
            _announcerAudio = Resources.Load<AudioClip>(announcerAudioFile);
        }
        if (_announcerAudio == null && !string.IsNullOrWhiteSpace(announcerText)) {
            _announcerAudio = Resources.Load<AudioClip>(announcerText);
            if (_announcerAudio == null) {
                var match = announcerText.Replace('?', ' ').Replace('.', ' ').Replace('!', ' ').Trim().ToLowerInvariant();
                var clips = DataHelper.GetAllAssetsOfType<AudioClip>("Assets/Resources");
                foreach(var clip in clips) {
                    if (clip.name.ToLowerInvariant().StartsWith(match)) {
                        _announcerAudio = clip;
                        break;
                    }
                }
            }
        }

        if (_announcerAudio != null) {
            announcerAudioFile = _announcerAudio.name;
            if (string.IsNullOrWhiteSpace(announcerText)) {
                announcerText = ExtractTextFromFileName(_announcerAudio.name);
            }
            _warnedAboutText = "";
        } else if (_warnedAboutText != announcerText) {
            Debug.LogWarning($"No matching audio clip found for Announcer Text '{announcerText}' in GameInfo '{name}.asset' ('{gameTitle}'){(teamNumber > 0 ? " by team " + teamNumber : "" )}");
            _warnedAboutText = announcerText;
        }

        foreach(var work in licensedWorksCredits) {
            work.creator = work.creator.Trim();
            work.source = work.source.Trim();
        }
    }
#endif


    [Tooltip("Control prompts shown before game. Use this for the left player or both players.")]
    public ControlPrompt[] controls;

    [Tooltip("Alternate controls for the right-side player. If blank, the same controls are used for both players.")]
    public ControlPrompt[] rightPlayerAltControls;


    // Actual paths used for serializing/deserializing scenes for runtime use.
    [HideInInspector] public string singlePlayerScenePath;
    [HideInInspector] public string twoPlayerScenePath;

    public string GetScenePath(int playerCount) {
        if (playerCount == 1) {
            if (string.IsNullOrEmpty(singlePlayerScenePath)) return twoPlayerScenePath;
            return singlePlayerScenePath;
        } else {
            if (string.IsNullOrEmpty(twoPlayerScenePath)) return singlePlayerScenePath;
            return twoPlayerScenePath;
        }
    }


    public string[] postgameMessages;


    [TexturePreview(5)]
    public Sprite bannerImage;

    public string[] developerCredits;
    public LicensedWorksCredit[] licensedWorksCredits;

    public bool CanPlay1P => singlePlayerScenePath.Length > 0;        
    public bool CanPlay2P => twoPlayerScenePath.Length > 0;


    [System.Serializable]
    public class LicensedWorksCredit
    {
        public string titleOfWork;

        [Delayed]
        public string creator;

        [Delayed]
        public string source;
        public string url;
        public string licence;
    }

    [System.Serializable]
    public struct ControlPrompt {
        public enum Joiner {
            And,
            Or,
            Then
        }
        public string label;
        public ControlAction primary;
        public Joiner joiner;
        public ControlAction secondary;
    }
}