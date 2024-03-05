using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ControlsDisplay : MonoBehaviour
{
    [SerializeField] CanvasGroup _leftControls;
    [SerializeField] CanvasGroup _rightControls;

    [SerializeField] float _fadeInSeconds = 0.5f;
    [SerializeField] float _waitSeconds = 1.5f;

    [SerializeField] ControlLockup _promptPrefab;

    [SerializeField] ControlIcon _joystickPrefab;
    [SerializeField] ControlIcon _buttonPrefab;
    [SerializeField] ControlIcon _buttonReleasePrefab;
    [SerializeField] ControlIcon _buttonHoldPrefab;
    [SerializeField] ControlIcon _buttonMashPrefab;

    [SerializeField] Color _button1Tint;
    [SerializeField] Color _button2Tint;

    [SerializeField] Sprite _overlayCW;
    [SerializeField] Sprite _overlayCCW;
    [SerializeField] Sprite _overlayHorizontal;
    [SerializeField] Sprite _overlayVertical;
    [SerializeField] Sprite _overlayAllDirections;
    [SerializeField] Sprite _overlayLeft;
    [SerializeField] Sprite _overlayRight;
    [SerializeField] Sprite _overlayUp;
    [SerializeField] Sprite _overlayDown;

    [SerializeField] Image _plusPrefab;
    [SerializeField] Sprite _slash;


    public Coroutine ShowControls(GameInfo info, PlayerID playersActive) {
        switch (playersActive) {
            case PlayerID.LeftPlayer:
                Populate((RectTransform)_leftControls.transform, info.controls);
                break;
            case PlayerID.RightPlayer:
                Populate((RectTransform)_rightControls.transform, info.controls);
                break;
            case PlayerID.BothPlayers:
                Populate((RectTransform)_leftControls.transform, info.controls);
                var p2Controls = info.controls;
                if (info.rightPlayerAltControls.Length > 0) {
                    p2Controls = info.rightPlayerAltControls;
                }
                Populate((RectTransform)_rightControls.transform, p2Controls);
                break;
        }

        return StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn() {
        float speed = 1f/_fadeInSeconds;
        for (float t = 0; t < 1f; t += speed * Time.deltaTime) {
            float alpha = Mathf.SmoothStep(0, 1, t);
            _leftControls.alpha = alpha;
            _rightControls.alpha = alpha;
            yield return null;
        }
        yield return new WaitForSeconds(_waitSeconds);
    }

    public void HideControls() {
        _leftControls.gameObject.SetActive(false);
        _leftControls.alpha = 0;
        _rightControls.gameObject.SetActive(false);
        _rightControls.alpha = 0;
    }

    void Populate(RectTransform parent, GameInfo.ControlPrompt[] prompts) {
        // Clear out old controls. (Could re-use for some small efficiency win)
        for (int i = parent.childCount - 1; i >= 0; i--) {
            Destroy(parent.GetChild(i).gameObject);
        }       

        foreach(var prompt in prompts) {
            var lockup = Instantiate(_promptPrefab, parent);
            lockup.label.text = prompt.label;
            bool hasAddedPrompt = false;
            if (prompt.primary != ControlAction.None) {
                hasAddedPrompt = true;
                AddIcon(prompt.primary, lockup.controls);
            }

            if (prompt.secondary != ControlAction.None) {
                if (hasAddedPrompt) {
                    var joiner = Instantiate(_plusPrefab, lockup.controls);
                    switch (prompt.joiner) {
                        case GameInfo.ControlPrompt.Joiner.Or:
                            joiner.sprite = _slash;
                            break;
                        case GameInfo.ControlPrompt.Joiner.Then:
                            joiner.sprite = _overlayRight;
                            break;
                    }
                }
                AddIcon(prompt.secondary, lockup.controls);
            }
        }

        parent.gameObject.SetActive(true);
    }

    void AddIcon(ControlAction action, RectTransform container) {
        int buttonNumber = (int)(action & (ControlAction.Button1 | ControlAction.Button2));

        if (buttonNumber > 0) {
            var prefab = _buttonPrefab;
            if (action.HasFlag(ControlAction.JoystickUp)) {
                if (action.HasFlag(ControlAction.JoystickDown))
                    prefab = _buttonMashPrefab;
                else 
                    prefab = _buttonReleasePrefab;
            } 
            if (action.HasFlag(ControlAction.JoystickDown)) prefab = _buttonHoldPrefab;
            

            var button = Instantiate(prefab, container);

            var tint = (buttonNumber < 2) ?  _button1Tint : _button2Tint;
            button.control.color = tint;
        } else {
            var stick = Instantiate(_joystickPrefab, container);
            if (action == ControlAction.Joystick) {
                stick.overlay.gameObject.SetActive(false);
                return;
            }
            stick.overlay.sprite = GetOverlay(action);            
        }
    }

    Sprite GetOverlay(ControlAction action) {
        switch(action) {
            case ControlAction.JoystickUp: return _overlayUp;
            case ControlAction.JoystickDown: return _overlayDown;
            case ControlAction.JoystickLeft: return _overlayLeft;
            case ControlAction.JoystickRight: return _overlayRight;
            case ControlAction.JoystickHorizontal: return _overlayHorizontal;
            case ControlAction.JoystickVertical: return _overlayVertical;
            case ControlAction.JoystickAllDirections: return _overlayAllDirections;
            case ControlAction.JoystickCW: return _overlayCW;
            case ControlAction.JoystickCCW: return _overlayCCW;
            default: return null;
        }
    }
}

public class ControlPrompt : MonoBehaviour {

}