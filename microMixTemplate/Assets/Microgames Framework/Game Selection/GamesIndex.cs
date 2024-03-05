using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We only need one instance. Disabling this to remove distractions from the UI.
//[CreateAssetMenu(fileName = "GamesIndex.Asset", menuName = "microMix/Games Index")]
public class GamesIndex : ScriptableObject
{
    #if UNITY_EDITOR
    [Tooltip("Rooted path to folder containing team projects. Use OS directory separator.")]
    [field:SerializeField, HideInInspector] public string sourcePath {get; private set;}

    [Tooltip("Path relative to Assets/ to copy data into. Contents of this folder will be erased. Use forward slashes to separate sub-folders.")]
    [field:SerializeField, HideInInspector, Delayed] public string destinationPath {get; private set;}

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
        destinationPath = destinationPath.Replace('\\', '/');
        if (destinationPath.StartsWith("assets/", System.StringComparison.OrdinalIgnoreCase)) {
            destinationPath = destinationPath.Substring(6);
        } else if (destinationPath.StartsWith('/')) {
            destinationPath = destinationPath.Substring(1);
        }
        if (destinationPath.EndsWith('/')) destinationPath = destinationPath[..^1];
    }
    #endif

    [Header("microMix Framework v. 1.2")]

    [Header("Testing Settings")]
    public MicrogamesManager.SkipStage quickTestSkip;

    public PlayerID quickTestPlayers;



    [Header("Microgame Randomization")]
    [SerializeField] int _avoidRepeatsWithin = 5;
    [SerializeField] GameInfo[] _singlePlayer;

    [SerializeField] GameInfo[] _twoPlayer;



    public GameInfo SelectGame(int playerCount, GameInfo[] dummies = null) {
        if (_spDeck == null) {
            int noRepeatWindow = Mathf.Min(_singlePlayer.Length - 1, _twoPlayer.Length - 1, _avoidRepeatsWithin);
            _recentGames = new GameInfo[noRepeatWindow];
            _spDeck = new ShuffleBag<GameInfo>(_singlePlayer);
            _mpDeck = new ShuffleBag<GameInfo>(_twoPlayer);
        }

        GameInfo selected;
        var deck = (playerCount == 1) ? _spDeck : _mpDeck;
        do {
            selected = deck.Draw();
        } while (System.Array.IndexOf(_recentGames, selected) >= 0);

        if (_recentGames.Length > 0) {
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