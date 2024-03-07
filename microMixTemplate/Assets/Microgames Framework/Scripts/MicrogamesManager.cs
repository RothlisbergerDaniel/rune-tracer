using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using UnityEngine.Rendering.Universal;

public class MicrogamesManager : MonoBehaviour
{
    [Flags]
    public enum SkipStage : Byte {
        None = 0,
        TitleScreen  = 1 << 0,
        Interstitial = 1 << 1,
        GameSelection = 1 << 2, 
        Controls     = 1 << 3,
        Credits      = 1 << 4
    }

    public static MicrogamesManager Instance { get; private set; }
    public static readonly string FrameworkScenePath = "Assets/microMix.unity";
    
    // flags for "quick testing mode", to bypass parts of the regular game flow

    public static bool isLoaded = false;
    public enum GameState
    {
        TitleScreen,
        PlayersReady,
        ControlsRevealStarted,
        ControlsRevealCompleted,
        CurtainsOpened,
        CurtainsClosed,
        GoalShowing,
        CountdownStarting,
        CountdownCompleted,
        MiniGameInProgress,
        MiniGameCompleted,
        MiniGameCompletedEarly,
        ResultsAcknowledged,
        InterstitialPlayed
    }
    [SerializeField] GameState _state = GameState.TitleScreen;

    private void SetState(GameState destination) {
        //Debug.Log($"{_state} ---> {destination}");
        _state = destination;
    }

    private void SetState(GameState destination, params GameState[] validPreStates) {
        if (Array.IndexOf(validPreStates, _state) >= 0) {
            //Debug.Log($"{_state} ---> {destination}");
            _state = destination;
        } else {
            //Debug.Log($"Cannot transition from state {_state} to {destination}");
        }
    }

    // List of mini-games
    public GamesIndex gamesIndex;

    public int roundCount;

    // Animator references
    public Animator curtainsAnimator;
    public Animator goalTextAnimator;
    public Animator countdownAnimator;
    public Animator countdownVolumeAnimator;
    public Animator countdownSandAnimator;
    //public Animator controlsAnimator;

    // Events
    public event Action GameStartEvent;
    public event Action OnLeftPlayerStartEvent;
    public event Action OnRightPlayerStartEvent;
    public event Action FifteenSecondsWarningEvent;
    public event Action TenSecondsWarningEvent;
    public event Action FiveSecondsWarningEvent;
    public event Action TimesUpEvent;

    [SerializeField] private InputActionAsset controls;
    public PlayerInfo leftPlayer;
    public PlayerInfo rightPlayer;

    private bool playersReadyCountdownStarted = false;
    [SerializeField]float playersReadyCountdownDuration = 5.0f;

    [SerializeField] TextMeshProUGUI goalUIText;

    [SerializeField] TextMeshProUGUI leftPlayerReadyText;
    [SerializeField] TextMeshProUGUI rightPlayerReadyText;

    [SerializeField] RectTransform titleScreen;

    [SerializeField] TextMeshProUGUI InterstitialText;
    [SerializeField] Animator interstitialTextAnimator;

    [SerializeField] SpinnerWidget gameSelectionSpinner;

    [SerializeField] ControlsDisplay controlsScreen;

    [SerializeField] AudioSource goalAnnouncement;

    [SerializeField] Credits creditsScreen;

    public int ReadiedPlayerCount => (leftPlayer.isReady ? 1 : 0) + (rightPlayer.isReady ? 1 : 0);

    public PlayerID RecentlyActivePlayers {get; private set;}

    SkipStage _quickTestSkip;

    static string _quickTestModeScene;
    private GameInfo _quickTestModeGame;
    private PlayerID _quickTestPlayers;

    private List<GameInfo> _gamesPlayed = new();

    [SerializeField] TextMeshProUGUI _joinPromptLeft;
    [SerializeField] TextMeshProUGUI _joinPromptRight;

    static readonly string JOIN_PROMPT = "Press Any Button to Join";
    static readonly string JOIN_CONFIRMATION = "Joining...";

    IEnumerator GameFlow() {
        creditsScreen.Clear();
        var dummies = new GameInfo[gameSelectionSpinner.EntryCount - 1];
        bool firstBoot = true;
        
        while(true) {

            titleScreen.gameObject.SetActive(false);

            leftPlayerReadyText.enabled = false;
            rightPlayerReadyText.enabled = false;

            leftPlayer.Unready();
            rightPlayer.Unready();

            if (!_quickTestSkip.HasFlag(SkipStage.TitleScreen)) {
                titleScreen.gameObject.SetActive(true);

                if (firstBoot) {
                    firstBoot = false;
                } else {
                    OpenCurtainsAnimation();
                    yield return new WaitUntil(() => _state == GameState.CurtainsOpened);
                }
                SetState(GameState.TitleScreen);
                yield return new WaitForSecondsRealtime(1);
                EnablePlayerControls();
                yield return new WaitUntil(() => _state == GameState.PlayersReady);
                DisablePlayerControls();
                CloseCurtainsAnimation();
                yield return new WaitUntil(() => _state == GameState.CurtainsClosed);
                titleScreen.gameObject.SetActive(false);
            }

            _gamesPlayed.Clear();
            int _roundsIdle = 0;

            for (int roundNumber = 0; roundNumber < roundCount; roundNumber++) {
                EnablePlayerControls();

                if (roundNumber > 0 && !_quickTestSkip.HasFlag(SkipStage.Interstitial))
                    yield return new WaitForSecondsRealtime(1.5f);    // Let the post-game message have the screen for a moment.

                // Figure out how many players we need for this round.

                // First, if quicktest, fake players readying-up in case title screen was skipped.
                if (_quickTestPlayers.HasFlag(PlayerID.LeftPlayer)) leftPlayer.Ready();
                if (_quickTestPlayers.HasFlag(PlayerID.RightPlayer)) rightPlayer.Ready();

                // Then, identify current players.
                var currentPlayers = PlayerID.None;
                if (leftPlayer.isReady) { 
                    currentPlayers |= PlayerID.LeftPlayer;
                    _joinPromptLeft.text = string.Empty;
                } else {
                    _joinPromptLeft.text = JOIN_PROMPT;
                }
                if (rightPlayer.isReady) {
                    currentPlayers |= PlayerID.RightPlayer;
                    _joinPromptRight.text = string.Empty;
                } else {
                    _joinPromptRight.text = JOIN_PROMPT;
                }

                int playerCount = ReadiedPlayerCount;

                // If none, assume the same player(s) will come back.
                if (currentPlayers == PlayerID.None) {
                    //Debug.Log($"No players ready - reverting to last active set {RecentlyActivePlayers}");
                    currentPlayers = RecentlyActivePlayers;
                    playerCount = (RecentlyActivePlayers == PlayerID.BothPlayers) ? 2 : 1;
                    // ...but if they don't, fast-forward to the credits.                    
                    if (_roundsIdle++ > 1 && !_quickTestSkip.HasFlag(SkipStage.Credits)) {
                        if (!_quickTestSkip.HasFlag(SkipStage.Interstitial))
                            yield return new WaitUntil(() => _state == GameState.InterstitialPlayed);
                        //Debug.Log("All players idle - bailing to credits.");
                        break;
                    }
                } else {
                    // Otherwise, remember this set of players.
                    RecentlyActivePlayers = currentPlayers;
                    _roundsIdle = 0;
                }

                GameInfo selectedGame;
                
                while (true) {
                    // Quick test: always re-play the scene that was open. Otherwise, choose randomly.                
                    if (_quickTestModeGame != null) {
                        selectedGame = _quickTestModeGame;
                        Array.Fill(dummies, selectedGame);
                    } else {
                        selectedGame = gamesIndex.SelectGame(playerCount, dummies);
                    }
                    LoadGameInfo(selectedGame);
                    
                    // Load the new game scene.
                    if (!_quickTestSkip.HasFlag(SkipStage.GameSelection)) {
                        // Set the spinner to animate game selection.
                        for (int i = 0; i < dummies.Length; i++) {
                            gameSelectionSpinner.Populate(i, dummies[i].gameIcon, dummies[i].gameTitle);
                        }
                        gameSelectionSpinner.Populate(dummies.Length, selectedGame.gameIcon, selectedGame.gameTitle);
                        gameSelectionSpinner.spinTicks = UnityEngine.Random.Range(10, 12);
                        var spinInProgress = gameSelectionSpinner.Spin(dummies.Length, true); 

                        bool upgradedTo2P = false;
                        while (spinInProgress.MoveNext()) {
                            if (playerCount == 1 && ReadiedPlayerCount == 2)   {
                                playerCount = 2;
                                if (_joinPromptLeft.text == JOIN_PROMPT)
                                    _joinPromptLeft.text = JOIN_CONFIRMATION;
                                if (_joinPromptRight.text == JOIN_PROMPT)
                                    _joinPromptRight.text = JOIN_CONFIRMATION;

                                currentPlayers = PlayerID.BothPlayers;

                                if (!selectedGame.CanPlay2P) {
                                    upgradedTo2P = true;
                                    gamesIndex.Unselect(1);                        
                                    break;
                                }
                            }
                            yield return null;
                        } 

                        if (upgradedTo2P) {
                            playerCount = 2;                            
                            continue;
                        }
                    }
                    break;
                }

                _joinPromptLeft.text = string.Empty;
                _joinPromptRight.text = string.Empty;

                var scenePath = selectedGame.GetScenePath(playerCount);
                var asyncLoad = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                asyncLoad.allowSceneActivation = false;
                DisablePlayerControls();

                if (!_quickTestSkip.HasFlag(SkipStage.Controls)) {
                    yield return controlsScreen.ShowControls(selectedGame, currentPlayers);
                    SetState(GameState.ControlsRevealCompleted);
                }
            
                asyncLoad.allowSceneActivation = true;
                while(!asyncLoad.isDone) {
                    yield return null;
                }
                // Scene is ready to go!
                // Scene activation is needed to ensure Instantiate() calls
                // without a parent don't dump objects into the framework scene.
                var gameScene = SceneManager.GetSceneByPath(scenePath);
                SceneManager.SetActiveScene(gameScene);

                if (!_quickTestSkip.HasFlag(SkipStage.GameSelection))
                    yield return gameSelectionSpinner.FadeOut(0.5f);
                
                OpenCurtainsAnimation();
                yield return new WaitUntil(() => _state == GameState.CurtainsOpened);

                
                controlsScreen.HideControls();
                ShowGoalAnimation();

                if (goalAnnouncement.clip != null)
                    goalAnnouncement.PlayDelayed(0.2f);
                
                yield return new WaitUntil(() => _state == GameState.GoalShowing);

                StartCountdownAnimation();
                yield return new WaitUntil(() => _state == GameState.CountdownStarting);

                // Treat players as ready for next round only if they provide input this round.
                leftPlayer.Unready();
                rightPlayer.Unready();

                SetState(GameState.MiniGameInProgress);
                EnablePlayerControls();
                GameStartEvent?.Invoke();

                // Wait until countdown is complete or mini-game is completed
                yield return new WaitUntil(() => _state == GameState.CountdownCompleted || _state == GameState.MiniGameCompleted || _state == GameState.MiniGameCompletedEarly);

                DisablePlayerControls();

                if (_state == GameState.CountdownCompleted || _state == GameState.MiniGameCompletedEarly) {

                    yield return new WaitForSecondsRealtime(5);
                    SetState(GameState.MiniGameCompleted);
                }

                CloseCurtainsAnimation();

                Debug.Log("Closing curtains");
                yield return new WaitUntil(() => _state == GameState.CurtainsClosed);

                SceneManager.UnloadSceneAsync(gameScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

                if (!_quickTestSkip.HasFlag(SkipStage.Interstitial)) {
                    // Check if there are any one-liners
                    if (selectedGame.postgameMessages != null && selectedGame.postgameMessages.Length > 0) {
                        // Select a random one-liner from the list
                        string randomOneLiner = selectedGame.postgameMessages.Random();
                        // Display the selected one-liner
                        InterstitialText.text = randomOneLiner;

                        
                    } else {
                        Debug.LogWarning($"No postgame messages found for game: {selectedGame.name}");
                        InterstitialText.text = _defaultInterstitials.Random();
                    }

                    yield return new WaitForSecondsRealtime(0.5f);
                    
                    PlayInterstitialAnimation();
                    if (roundNumber + 1 >= roundCount) // Only wait for full fade after last game.
                        yield return new WaitUntil(() => _state == GameState.InterstitialPlayed);
                }

                RestoreGlobalState();
            }

            _joinPromptLeft.text = string.Empty;
            _joinPromptRight.text = string.Empty;

            EnablePlayerControls();
            if (!_quickTestSkip.HasFlag(SkipStage.Credits)) {
                creditsScreen.Populate(_gamesPlayed);
                OpenCurtainsAnimation();
                yield return creditsScreen.StartScroll();
                CloseCurtainsAnimation();

                yield return new WaitUntil(() => _state == GameState.CurtainsClosed);
                creditsScreen.Clear();
            }

            // If any games are using PlayerPrefs for persisting a high score/etc.
            // ensure it's not lost if we crash/lose power without a graceful exit.
            PlayerPrefs.Save();

            // Clean up excess assets loaded and lingering from the last round of games.
            goalAnnouncement.clip = null;
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BeforeSceneLoad() {
        Instance = null;
        isLoaded = false;

        Scene currentScene = SceneManager.GetActiveScene();
        // Debug.Log("Before Scene Load " + currentScene.name);
        if (currentScene.path == FrameworkScenePath) {
            _quickTestModeScene = null;
            return;
        } else {
            _quickTestModeScene = currentScene.path;
            if (string.IsNullOrEmpty(_quickTestModeScene)) {
                Debug.LogWarning("Save this scene before you can use it with the Microgame Framework.");
                return;
            }
        }

        // Check for game objects with components inheriting from MicrogameEvents or MicrogameInputEvents
        var microgameEventsComponent = FindAnyObjectByType<MicrogameEvents>(FindObjectsInactive.Include);

        // If no such components are found, load the microMix scene
        if (microgameEventsComponent != null) { // || microgameInputEventsComponents.Length > 0) {
            var frameworkScene = UnityEditor.AssetDatabase.LoadMainAssetAtPath(FrameworkScenePath);
            if (frameworkScene == null) {
                Debug.LogError($"Could not find framework scene at {FrameworkScenePath} - make sure you did not delete, move, or rename it!");
                // Abort so error is addressed right away.
                UnityEditor.EditorApplication.ExitPlaymode();
            }
            SceneManager.LoadScene(FrameworkScenePath);
        } else {
            Debug.Log("Skipped loading microMix because no existing game objects with MicrogameEvents or MicrogameInputEvents components.");
        }
    }
#endif


    private void Awake() {
        // flag for base classes implementing input and events
        isLoaded = true;

        // Singleton pattern implementation
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Input system initialization
        controls = Controls.Instance.actionAsset;

        // Set up input actions for both players
        InputActionMap player1ActionMap = controls.FindActionMap("Player1");
        player1ActionMap.FindAction("Stick").performed += OnLeftPlayerActivity;
        player1ActionMap.FindAction("Button1").performed += OnLeftPlayerActivity;
        player1ActionMap.FindAction("Button2").performed += OnLeftPlayerActivity;
        player1ActionMap.FindAction("Auxiliary").performed += OnLeftPlayerActivity;
    
        InputActionMap player2ActionMap = controls.FindActionMap("Player2");
        player2ActionMap.FindAction("Stick").performed += OnRightPlayerActivity;
        player2ActionMap.FindAction("Button1").performed += OnRightPlayerActivity;
        player2ActionMap.FindAction("Button2").performed += OnRightPlayerActivity;
        player2ActionMap.FindAction("Auxiliary").performed += OnRightPlayerActivity;

        var debug = controls.FindActionMap("Debug");
        debug.Enable();
        debug.FindAction("Abort").performed += Abort;
        
   
        leftPlayer = new PlayerInfo(PlayerID.LeftPlayer);
        rightPlayer = new PlayerInfo(PlayerID.RightPlayer);

        if (!string.IsNullOrEmpty(_quickTestModeScene)) {
            _quickTestSkip = gamesIndex.quickTestSkip;
            EnterTestMode();
        } else {
            _quickTestSkip = SkipStage.None;
            _quickTestModeGame = null;
            _quickTestPlayers = PlayerID.None;            
        }
        StartCoroutine(GameFlow());
    }

    // Fast-forward a game.
    void Abort(InputAction.CallbackContext context) {
        if (_state == GameState.MiniGameInProgress) {
            OnGameCompletedEarly();
        }
    }
    private void EnterTestMode() {
        #if UNITY_EDITOR

        _quickTestPlayers = gamesIndex.quickTestPlayers;
        _quickTestModeGame = null;
        foreach(var info in DataHelper.GetAllAssetsOfType<GameInfo>()) {
            
            if (info.singlePlayerScenePath == _quickTestModeScene) {
                _quickTestModeGame = info;
                if (_quickTestPlayers == PlayerID.BothPlayers && info.twoPlayerScenePath != _quickTestModeScene) {
                    // This scene is only for single player. Assume left player.                    
                    _quickTestPlayers= PlayerID.LeftPlayer;                    
                }
                break;
            } else if (info.twoPlayerScenePath == _quickTestModeScene) {
                // This scene is only for multiplayer.
                _quickTestModeGame = info;
                _quickTestPlayers = PlayerID.BothPlayers;
                break;
            }
        }
            
        if (_quickTestModeGame == null) {
            Debug.LogWarning("No GameInfo asset found for this scene.\nMake sure you create one using Assets > Create > microMix > GameInfo");
            _quickTestModeGame  = ScriptableObject.CreateInstance<GameInfo>();
            _quickTestModeGame.gameTitle = _quickTestModeScene;
            _quickTestModeGame.singlePlayerScenePath = _quickTestModeScene;
            _quickTestModeGame.twoPlayerScenePath = _quickTestModeScene;
        }

        playersReadyCountdownDuration = 0.5f;

        // Start with curtains closed to avoid peeking the game when skipping title.
        if (_quickTestSkip.HasFlag(SkipStage.TitleScreen)) {
            curtainsAnimator.Play("Close Curtains", 0, 1);
        }
        #endif
    }

    private void OnLeftPlayerActivity(InputAction.CallbackContext context) {
        leftPlayer.Ready();
        if (_state == GameState.TitleScreen) {
            leftPlayerReadyText.enabled = true;
            OnLeftPlayerStartEvent?.Invoke();            
            StartCountdownIfNeeded();
        }
    }

    private void OnRightPlayerActivity(InputAction.CallbackContext context) {
        rightPlayer.Ready();
        if (_state == GameState.TitleScreen) {
            rightPlayerReadyText.enabled = true;
            OnRightPlayerStartEvent?.Invoke();            
            StartCountdownIfNeeded();
        }
    }

    private void StartCountdownIfNeeded() {
        if (!playersReadyCountdownStarted && (leftPlayer.isReady || rightPlayer.isReady)) {
            playersReadyCountdownStarted = true;
            StartCoroutine(StartGameCountdown());
        }
    }
    private bool AreBothPlayersReady() {
        return leftPlayer.isReady && rightPlayer.isReady;
    }

    private IEnumerator StartGameCountdown() {
        float timer = playersReadyCountdownDuration;
        while (timer > 0) {

            if (AreBothPlayersReady()) {
                break; // Both players are ready, break the countdown
            }
            timer -= Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        SetState(GameState.PlayersReady); // Change state when countdown is over or both are ready
        playersReadyCountdownStarted = false;
    }


    private void DisablePlayerControls() {
        //Debug.Log($"DISabling controls in state {state}");
        Controls.Instance.DisableActionMap("Player1");
        Controls.Instance.DisableActionMap("Player2");
    }

    private void EnablePlayerControls() {
        //Debug.Log($"Enabling controls in state {state}");
        Controls.Instance.EnableActionMap("Player1");
        Controls.Instance.EnableActionMap("Player2");
    }

    readonly string[] _defaultInterstitials = new[]{
        "Remember to set some postgame texts for your game!",
        "I wish I had something to say here...",
        "...",
        "This space for rent"
    };


    // Load the GameInfo ScriptableObject
    void LoadGameInfo(GameInfo gameInfo) {
        _gamesPlayed.Add(gameInfo);
        string prompt = gameInfo.announcerText;
        goalUIText.text = prompt;

        var clip = Resources.Load<AudioClip>(gameInfo.announcerAudioFile);
        if (clip != null) {
            clip.LoadAudioData();
            goalAnnouncement.clip = clip;
        } else {
            Debug.LogWarning($"No voice sample found for game goal/prompt '{prompt}' / file 'Resources/{gameInfo.announcerAudioFile}.wav'");
            goalAnnouncement.clip = null;
        }
    }

    public void OnGameCompletedEarly() {
        SetState(GameState.MiniGameCompletedEarly, GameState.MiniGameInProgress);
        StopCountdownAnimation();
    }


    // Animation event methods
    public void OnCurtainsOpened() {
        SetState(GameState.CurtainsOpened);
    }

    public void OnCurtainsClosed() {
        SetState(GameState.CurtainsClosed);
    }

    public void OnGoalShown() {
        SetState(GameState.GoalShowing);
    }

    public void OnTimerReady() {
        SetState(GameState.CountdownStarting);
    }

    public void OnMiniGameCompleted() {
        SetState(GameState.MiniGameCompleted);
    }

    public void OnControlsSequenceFinished() {
        SetState(GameState.ControlsRevealCompleted);
    }

    public void OnInterstitialFinished() {
        SetState(GameState.InterstitialPlayed);
    }

    public void OnFifteenSecondsLeft() {
        FifteenSecondsWarningEvent?.Invoke();
    }

    public void OnTenSecondsLeft() {
        TenSecondsWarningEvent?.Invoke();
    }

    public void OnFiveSecondsLeft() {
        FiveSecondsWarningEvent?.Invoke();
    }

    public void OnCountdownComplete() {
        SetState(GameState.CountdownCompleted);
        TimesUpEvent?.Invoke();
        StartCoroutine(DelayBeforeMiniGameCompleted());
    }

    private IEnumerator DelayBeforeMiniGameCompleted() {
        yield return new WaitForSecondsRealtime(3);
        SetState(GameState.MiniGameCompleted);
    }

    // Animation triggers
    private void OpenCurtainsAnimation() {
        curtainsAnimator.SetTrigger("Open");
    }

    /*
    private void ShowControlsAnimation() {
        controlsAnimator.SetTrigger("Reveal");
    }
    */

    private void ShowGoalAnimation() {
        goalTextAnimator.SetTrigger("Show Goal");
    }

    private void StartCountdownAnimation() {
        countdownAnimator.SetTrigger("Start");
        countdownVolumeAnimator.SetTrigger("Start");
        countdownSandAnimator.SetTrigger("Start");
        countdownVolumeAnimator.speed = 1;
    }

    void StopCountdownAnimation() {
        countdownAnimator.SetTrigger("Stop");
        countdownSandAnimator.SetTrigger("Stop");
        countdownVolumeAnimator.speed = 0;
    }


    private void CloseCurtainsAnimation() {
        curtainsAnimator.SetTrigger("Close");
    }

    private void PlayInterstitialAnimation() {
        interstitialTextAnimator.SetTrigger("Play");
    }

    [SerializeField] UniversalRenderPipelineAsset _renderPipeline;

    void RestoreGlobalState() {
        // TODO: Check for any shenanigans students pull in global settings
        // and put it all back after we unload their scene.
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        Physics.gravity = new Vector3(0, -9.81f, 0);
        Physics.defaultSolverIterations = 6;
        Physics.defaultSolverVelocityIterations = 1;
        Physics.queriesHitBackfaces = false;
        Physics.queriesHitTriggers = true;

        Physics2D.gravity = new Vector2(0, -9.81f);

        // Reset global render settings.
        _renderPipeline.shadowCascadeCount = 4;
        _renderPipeline.msaaSampleCount = 4;
    }
}
