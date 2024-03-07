using UnityEngine;

public enum PlayerID
{
    None = 0,
    LeftPlayer = 1,
    RightPlayer = 2,
    BothPlayers = LeftPlayer | RightPlayer
}

public class PlayerInfo
{
    public enum PlayerStatus { InProgress, Won, Failed }
    public PlayerID playerID { get; private set; }
    public PlayerStatus status { get; set; }

    public bool isReady { get; set; }
    double _lastReadyTime;

    public float SecondsSinceInput => (float)(Time.timeAsDouble - _lastReadyTime);

    public PlayerInfo(PlayerID playerID) {
        this.playerID = playerID;
        status = PlayerStatus.InProgress;
        isReady = false;
    }

    public void Ready() {
        isReady = true;
        _lastReadyTime = Time.timeAsDouble;
    }

    public void Unready() {
        isReady = false;
    }
}
