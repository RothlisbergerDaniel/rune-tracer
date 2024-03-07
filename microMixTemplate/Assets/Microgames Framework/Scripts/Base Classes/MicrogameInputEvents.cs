using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Use for a Player Controller class that processes control input from one player.
/// </summary>
public abstract class MicrogameInputEvents : MicrogameEvents
{
    public InputSource getInputFrom; // Identifies which player control set is used by this script
    private InputActionAsset controls;
    protected InputAction button1;
    protected InputAction button2;
    private InputAction _rawStick;
    public PlayerID playerID { get; private set; }

    protected Vector2 stick { 
        get {
            if (_rawStick == null) return default;
            return _rawStick.ReadValue<Vector2>();
        }
    }

    public void Initialize(InputSource source) {
        getInputFrom = source;
        string actionMapName = "Player1";

        controls = Controls.Instance.actionAsset;
        playerID = PlayerID.LeftPlayer;
        if (source == InputSource.AnySinglePlayer) {
            var recent = MicrogamesManager.Instance.RecentlyActivePlayers;
            if (recent == PlayerID.RightPlayer) {
                playerID = PlayerID.RightPlayer;
                actionMapName = "Player2";
            }        
        } else if (source == InputSource.RightPlayer) {
            playerID = PlayerID.RightPlayer;
            actionMapName = "Player2";
        }

        InputActionMap actionMap = controls.FindActionMap(actionMapName);
        button1 = actionMap.FindAction("Button1");
        button2 = actionMap.FindAction("Button2");
        _rawStick = actionMap.FindAction("Stick");
    }

    protected override void OnEnable() {
        if (!MicrogamesManager.isLoaded) return;
        base.OnEnable();

        if (button1 == null)
            Initialize(getInputFrom);

        // Setup input action bindings and enable them
        button1.performed += OnButton1Pressed;
        button1.canceled += OnButton1Released;

        button2.performed += OnButton2Pressed;
        button2.canceled += OnButton2Released;
    }

    protected override void OnDisable() {
        if (!MicrogamesManager.isLoaded) return;
        base.OnDisable();
        // Disable and clean up input actions
        button1.performed -= OnButton1Pressed;
        button1.canceled -= OnButton1Released;

        button2.performed -= OnButton2Pressed;
        button2.canceled -= OnButton2Released;
    }

    protected virtual void OnButton1Pressed(InputAction.CallbackContext context) {
        //Debug.Log(playerActionMap.ToString() + " pressed Button 1");
    }

    protected virtual void OnButton2Pressed(InputAction.CallbackContext context) {
       //Debug.Log(playerActionMap.ToString() + " pressed Button 2");
    }

    protected virtual void OnButton1Released(InputAction.CallbackContext context) {
        //Debug.Log(playerActionMap.ToString() + " released Button 1");
    }

    protected virtual void OnButton2Released(InputAction.CallbackContext context) {
       //Debug.Log(playerActionMap.ToString() + " released Button 2");
    }
}

public enum InputSource {
    AnySinglePlayer = 0,
    LeftPlayer = 1,
    RightPlayer = 2
}
