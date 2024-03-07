using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Use for a COOP controller that processes both players' control input events.
/// </summary>
public abstract class MicrogameMultiInputEvents : MicrogameEvents
{
    public InputSource getInputFrom; // Identifies which player control set is used by this script
    protected InputAction leftButton1;
    protected InputAction leftButton2;
    private InputAction _leftStick;

    protected InputAction rightButton1;
    protected InputAction rightButton2;
    private InputAction _rightStick;

    protected Vector2 leftStick { 
        get {
            if (_leftStick == null) return default;
            return _leftStick.ReadValue<Vector2>();
        }
    }

    protected Vector2 rightStick { 
        get {
            if (_rightStick == null) return default;
            return _rightStick.ReadValue<Vector2>();
        }
    }

    public void Initialize(InputSource source) {
        var controls = Controls.Instance.actionAsset;
        
        InputActionMap actionMap = controls.FindActionMap("Player1");
        leftButton1 = actionMap.FindAction("Button1");
        leftButton2 = actionMap.FindAction("Button2");
        _leftStick = actionMap.FindAction("Stick");

        actionMap = controls.FindActionMap("Player2");
        rightButton1 = actionMap.FindAction("Button1");
        rightButton2 = actionMap.FindAction("Button2");
        _rightStick = actionMap.FindAction("Stick");
    }

    protected override void OnEnable() {
        if (!MicrogamesManager.isLoaded) return;
        base.OnEnable();

        if (leftButton1 == null)
            Initialize(getInputFrom);

        // Setup input action bindings and enable them
        leftButton1.performed += OnLeftButton1Pressed;
        leftButton1.canceled += OnLeftButton1Released;
        leftButton2.performed += OnLeftButton2Pressed;
        leftButton2.canceled += OnLeftButton2Released;

        rightButton1.performed += OnRightButton1Pressed;
        rightButton1.canceled += OnRightButton1Released;
        rightButton2.performed += OnRightButton2Pressed;
        rightButton2.canceled += OnRightButton2Released;
    }

    protected override void OnDisable() {
        if (!MicrogamesManager.isLoaded) return;
        base.OnDisable();
        // Disable and clean up input actions
        leftButton1.performed -= OnLeftButton1Pressed;
        leftButton1.canceled -= OnLeftButton1Released;
        leftButton2.performed -= OnLeftButton2Pressed;
        leftButton2.canceled -= OnLeftButton2Released;

        rightButton1.performed -= OnRightButton1Pressed;
        rightButton1.canceled -= OnRightButton1Released;
        rightButton2.performed -= OnRightButton2Pressed;
        rightButton2.canceled -= OnRightButton2Released;
    }

    protected virtual void OnLeftButton1Pressed(InputAction.CallbackContext context) {
    }

    protected virtual void OnLeftButton2Pressed(InputAction.CallbackContext context) {
    }

    protected virtual void OnLeftButton1Released(InputAction.CallbackContext context) {
    }

    protected virtual void OnLeftButton2Released(InputAction.CallbackContext context) {
    }

    protected virtual void OnRightButton1Pressed(InputAction.CallbackContext context) {
    }

    protected virtual void OnRightButton2Pressed(InputAction.CallbackContext context) {
    }

    protected virtual void OnRightButton1Released(InputAction.CallbackContext context) {
    }

    protected virtual void OnRightButton2Released(InputAction.CallbackContext context) {
    }
}