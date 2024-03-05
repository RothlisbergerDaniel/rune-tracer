using Unity.VisualScripting;
using UnityEngine;
namespace team99
{
    public class GameManager : MicrogameEvents
    {

        public ZoneTracker player1Zone;
        public ZoneTracker player2Zone;

        public Transform player1ResultPosition;
        public Transform player2ResultPosition;

        public GameObject trophy;
        public Transform fork;

        public AudioSource music;
        public float trophyHeight;
        protected override void OnTimesUp() {
            base.OnTimesUp();

            music.Stop();


            int player1Count = player1Zone.objectsWithinTrigger.Count;
            int player2Count = player2Zone.objectsWithinTrigger.Count;

            if (player1Count < player2Count) {
                // Player 1 wins
                Instantiate(trophy, player1ResultPosition.position + Vector3.up * trophyHeight, trophy.transform.rotation);
                fork.position = player2ResultPosition.position;
                fork.gameObject.SetActive(true);
            } else if (player1Count > player2Count) {
                // Player 2 wins
                Instantiate(trophy, player2ResultPosition.position + Vector3.up * trophyHeight, trophy.transform.rotation);
                fork.position = player1ResultPosition.position;
                fork.gameObject.SetActive(true);
            } else {
                // It's a draw
                Instantiate(trophy, player1ResultPosition.position + Vector3.up * trophyHeight, trophy.transform.rotation);
                Instantiate(trophy, player2ResultPosition.position + Vector3.up * trophyHeight, trophy.transform.rotation);
            }

        }
        protected override void OnGameStart() {
            base.OnGameStart();
        }

    }
}