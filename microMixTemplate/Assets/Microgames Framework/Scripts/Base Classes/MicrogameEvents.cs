using UnityEngine;

/// <summary>
/// Use this for a game manager script that needs to respond to game start / time running out,
/// but does not manage player control input.
/// </summary>
public abstract class MicrogameEvents : MonoBehaviour
{
    protected virtual void OnEnable() {
        if (!MicrogamesManager.isLoaded) return;
        MicrogamesManager.Instance.GameStartEvent += OnGameStart;
        MicrogamesManager.Instance.FifteenSecondsWarningEvent += OnFifteenSecondsLeft;
        MicrogamesManager.Instance.TenSecondsWarningEvent += OnTenSecondsLeft;
        MicrogamesManager.Instance.FiveSecondsWarningEvent += OnFiveSecondsLeft;
        MicrogamesManager.Instance.TimesUpEvent += OnTimesUp;
    }

    protected virtual void OnDisable() {
        if (!MicrogamesManager.isLoaded) return;
        MicrogamesManager.Instance.GameStartEvent -= OnGameStart;
        MicrogamesManager.Instance.FifteenSecondsWarningEvent -= OnFifteenSecondsLeft;
        MicrogamesManager.Instance.TenSecondsWarningEvent -= OnTenSecondsLeft;
        MicrogamesManager.Instance.FiveSecondsWarningEvent -= OnFiveSecondsLeft;
        MicrogamesManager.Instance.TimesUpEvent -= OnTimesUp;
    }

    protected void ReportGameCompletedEarly() {
        MicrogamesManager.Instance.OnGameCompletedEarly();
    }

    protected virtual void OnGameStart() {
        // Code to execute when the microgame starts
    }
    protected virtual void OnFifteenSecondsLeft() {
        // Code to execute when there are 15 seconds left in the game
    }
    protected virtual void OnTenSecondsLeft() {
        // Code to execute when there are 10 seconds left in the game
    }
    protected virtual void OnFiveSecondsLeft() {
        // Code to execute when there are 5 seconds left in the game
    }
    protected virtual void OnTimesUp() {
        // Code to execute when time runs out in the game
    }
}
