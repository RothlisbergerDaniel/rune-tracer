using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We only need one instance. Disabling this to remove distractions from the UI.
//[CreateAssetMenu(fileName = "GamesIndex.Asset", menuName = "microMix/Games Index")]
public class GamesIndex : ScriptableObject
{
    #if UNITY_EDITOR
    [Tooltip("Rooted path to folder containing team projects. Use OS directory separator.")]
    [HideInInspector] string _sourcePath;

    [Tooltip("Path relative to Assets/ to copy data into. Contents of this folder will be erased. Use forward slashes to separate sub-folders.")]
    [Delayed, HideInInspector] string _destinationPath = "Imported";

    public string SourcePath => _sourcePath;
    public string DestinationPath => _destinationPath;

    [HideInInspector] public bool enableImport;
    
    public void ReplaceGames(List<GameInfo> infos) {
        if (infos == null || infos.Count == 0) {
            _singlePlayer  = null;
            _twoPlayer = null;
        }

        var sp = new List<GameInfo>();
        var mp = new List<GameInfo>();

        foreach(var game in infos) {
            if (!game.includeInRandomizer) continue;
            if (game.CanPlay1P) sp.Add(game);
            if (game.CanPlay2P) mp.Add(game);
        }        
        _singlePlayer = sp.ToArray();
        _twoPlayer = mp.ToArray();
    }

    void OnValidate() {
        _destinationPath = DestinationPath.Replace('\\', '/');
        if (DestinationPath.StartsWith("assets/", System.StringComparison.OrdinalIgnoreCase)) {
            _destinationPath = DestinationPath.Substring(6);
        } else if (DestinationPath.StartsWith('/')) {
            _destinationPath = DestinationPath.Substring(1);
        }
        if (DestinationPath.EndsWith('/')) _destinationPath = DestinationPath[..^1];
    }
    #endif

    [Header("microMix Framework v. 1.8")]

    [Header("Testing Settings")]
    public MicrogamesManager.SkipStage quickTestSkip;

    public PlayerID quickTestPlayers;



    [Header("Microgame Randomization")]
    [SerializeField] int _avoidRepeatsWithin = 5;
    [SerializeField] GameInfo[] _singlePlayer;

    [SerializeField] GameInfo[] _twoPlayer;



    public GameInfo SelectGame(int playerCount, GameInfo[] dummies = null) {
        if (_spDeck == null) {
            int noRepeatWindow = Mathf.Min(Mathf.Max(0, _singlePlayer.Length - 1, _twoPlayer.Length - 1), _avoidRepeatsWithin);
            _recentGames = new GameInfo[noRepeatWindow];
            _spDeck = new ShuffleBag<GameInfo>(_singlePlayer.Length > 0 ? _singlePlayer : _twoPlayer);
            _mpDeck = new ShuffleBag<GameInfo>(_twoPlayer.Length > 0 ? _twoPlayer : _singlePlayer);
            if (_spDeck.Count == 0) {
                Debug.LogError("No games found - make sure to Refresh Local Games on the Assets/GameIndex asset");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.ExitPlaymode();
                #endif
            }
        }

        GameInfo selected;
        var deck = (playerCount == 1) ? _spDeck : _mpDeck;

        bool useAntiRepeat = _recentGames.Length > 0 && _recentGames.Length < deck.Count;

        do {
            selected = deck.Draw();
        } while (useAntiRepeat && (System.Array.IndexOf(_recentGames, selected) >= 0));

        if (useAntiRepeat) {
            _recentGames[_oldestIndex] = selected;
            _oldestIndex = (_oldestIndex + 1) % _recentGames.Length;
        }

        // Fill the spinner with other, non-selected games.
        if (dummies != null) {
            deck.PeekOthers(dummies);
        }
        return selected;
    }

    public void Unselect(int playerCount) {
        var deck = (playerCount == 1) ? _spDeck : _mpDeck;
        deck.UnDraw();

        if (_recentGames.Length > 0) {
            _oldestIndex = (_oldestIndex + _recentGames.Length - 1) % _recentGames.Length;
            _recentGames[_oldestIndex] = null;
        }
    }

    GameInfo[] _recentGames;
    int _oldestIndex;
    [System.NonSerialized] ShuffleBag<GameInfo> _spDeck;
    [System.NonSerialized] ShuffleBag<GameInfo> _mpDeck;
}