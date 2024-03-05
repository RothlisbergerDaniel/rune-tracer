using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameInfo", menuName = "microMix/GameInfo", order = 1)]
public class GameInfo : ScriptableObject
{
    public bool includeInRandomizer = true;

    [HideInInspector] public int teamNumber;

    public string gameTitle;

    [TexturePreview(3)]
    public Sprite gameIcon;

    
    public string prompt;

 #if UNITY_EDITOR
    // Handy for dragging in the right file in the inspector,
    // but not deserializable in game, so we use these to save paths.
    public UnityEditor.SceneAsset singlePlayerScene;
    public UnityEditor.SceneAsset twoPlayerScene;

    string GetScenePath(UnityEditor.SceneAsset scene) {
        if (scene == null) return string.Empty;
        return UnityEditor.AssetDatabase.GetAssetPath(scene);
    }

    void OnValidate() {
        singlePlayerScenePath = GetScenePath(singlePlayerScene);
        twoPlayerScenePath = GetScenePath(twoPlayerScene);

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