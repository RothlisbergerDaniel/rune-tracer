using UnityEngine;
public class AnimationEventsHandler : MonoBehaviour
{
    public void OnGoalShown() => MicrogamesManager.Instance.OnGoalShown();
    public void OnTimerReady() => MicrogamesManager.Instance.OnTimerReady();
    public void OnFifteenSecondsLeft() => MicrogamesManager.Instance.OnFifteenSecondsLeft();
    public void OnTenSecondsLeft() => MicrogamesManager.Instance.OnTenSecondsLeft();
    public void OnFiveSecondsLeft() => MicrogamesManager.Instance.OnFiveSecondsLeft();
    public void OnCountdownComplete() => MicrogamesManager.Instance.OnCountdownComplete();
    public void OnControlsSequenceFinished() => MicrogamesManager.Instance.OnControlsSequenceFinished();
    public void OnInterstitialFinished() => MicrogamesManager.Instance.OnInterstitialFinished();
    public void OnCurtainsClosed() => MicrogamesManager.Instance.OnCurtainsClosed();
    public void OnCurtainsOpened() => MicrogamesManager.Instance.OnCurtainsOpened();

}
