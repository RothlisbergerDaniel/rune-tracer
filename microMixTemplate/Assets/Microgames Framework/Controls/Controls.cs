using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controls : MonoBehaviour
{
    public static Controls Instance { get; private set; }
    public InputActionAsset actionAsset;

    private InputActionMap player1Actions;
    private InputActionMap player2Actions;

    private Gamepad player1Gamepad;
    private Gamepad player2Gamepad;

    private void OnEnable() {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        switch (change) {
            case InputDeviceChange.Added:
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Reconnected:
                AssignGamepads();
                break;
        }
    }
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        player1Actions = actionAsset.FindActionMap("Player1");
        player2Actions = actionAsset.FindActionMap("Player2");
    }

    private void Start() {
        AssignGamepads();
    }

    private void AssignGamepads() {

        // On my laptop, Nintendo Switch Pro Controller connected via Bluetooth creates two "gamepad" entries on the system, one seems to be a virtual Xbox Controller and the other SwitchProControllerHID
        // (verified with Unity's Player Input Manager Component)
        // Related to XInput?
        // Consequently, remove all SwitchProControllerHIDs from the list.
        var gamepads = Gamepad.all.Where(gp => gp.name != "SwitchProControllerHID").ToList();
        var keyboard = Keyboard.current;

        // Assign or reassign player 1's gamepad
        if (player1Gamepad == null || !gamepads.Contains(player1Gamepad)) {
            player1Gamepad = gamepads.Count >= 1 ? gamepads[0] : null;
        }

        // Assign or reassign player 2's gamepad
        if (player2Gamepad == null || !gamepads.Contains(player2Gamepad)) {
            player2Gamepad = gamepads.Count >= 2 ? gamepads[1] : null;
        }

        // Set devices for player 1
        if (player1Gamepad != null) {
            player1Actions.devices = new InputDevice[] { player1Gamepad, keyboard };
            player1Actions.bindingMask = InputBinding.MaskByGroup("Player1Scheme");
        } else {
            player1Actions.devices = new InputDevice[] { keyboard };
        }

        // Set devices for player 2
        if (player2Gamepad != null) {
            player2Actions.devices = new InputDevice[] { player2Gamepad, keyboard };
            player2Actions.bindingMask = InputBinding.MaskByGroup("Player2Scheme");
        } else {
            player2Actions.devices = new InputDevice[] { keyboard };
        }
    }

    public void EnableActionMap(string actionMapName) {
        InputActionMap actionMap = actionAsset.FindActionMap(actionMapName, throwIfNotFound: false);
        if (actionMap != null) {
            actionMap.Enable();
            //Debug.Log("Enabled action map: " + actionMapName);
        } else {
            //Debug.LogError("Failed to find action map: " + actionMapName);
        }
    }

    public void DisableActionMap(string actionMapName) {
        InputActionMap actionMap = actionAsset.FindActionMap(actionMapName, throwIfNotFound: false);
        if (actionMap != null) {
            actionMap.Disable();
            //Debug.Log("Disabled action map: " + actionMapName);
        } else {
            //Debug.LogError("Failed to find action map: " + actionMapName);
        }
    }

}
