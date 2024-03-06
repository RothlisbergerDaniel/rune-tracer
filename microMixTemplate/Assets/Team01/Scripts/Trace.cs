using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace team01
{
    public class Trace : MicrogameInputEvents
    {

        public Vector3[] runeStarts = new Vector3[10]; //tracks start points as a Vector3, i.e. where to place the pointer when a new rune loads
        public int[] runeStepCounts = new int[10]; //tracks number of steps needed to complete the rune
        public Vector3[] runePoints = new Vector3[39]; //tracks vertices of each rune, i.e. where the pointer targets at each step - 1 more than necessary
        public Vector2[] runeStepInputs = new Vector2[39]; //stores inputs required to complete each rune, in terms of joystick values - 1 more than necessary
        public int stepPos; //tracks step position based on rune ID #, previous steps taken, etc.
        int stepCount = 0; //tracks current steps taken to compare against current rune's total step count

        public int runeID = 0; //public for testing purposes
        public GameObject linkedRune; //rune linked to this copy of the game controller
        public GameObject lineupRune; //links to the lineup rune prefab

        Vector2 direction; //current user direction input
        Vector2 target; //target direction input
        public float cursorSpeed = 100f; //speed at which the cursor snaps to points, default 10

        public int[] runeDifficulties = new int[10]; //list of rune difficulties by ID in order from low to high
        public int maxCount;
        
        // Start is called before the first frame update
        void Start()
        {
            //getStepPosition(runeID);
            getNewRune(false, true); //replace this with hiding cursor on Start, then show cursor OnGameStart
            getLineup();
        }
        protected override void OnGameStart()
        {
            //base.OnGameStart();
            getNewRune(false, false);
        }


        // Update is called once per frame
        void Update()
        {
            direction = stick; //direction variable is set to stick input
            Vector3 currentPoint = new Vector3(runePoints[Mathf.Clamp(stepPos - 1, 0, 100)][0] - 0f, runePoints[Mathf.Clamp(stepPos - 1, 0, 100)][1] - 1.5f, 0f); //convert preset point to new position, scalabale to 2P
            if (direction == target && (transform.position == currentPoint || stepCount == 0)) //use Mathf.Clamp to ensure no index out-of-bounds errors
            {
                if (stepCount < runeStepCounts[runeID])
                {
                    stepPos++;
                    //stepPos = Mathf.Clamp(stepPos, 0, runePoints.Length); //clamp so it doesn't go out-of-bounds
                    stepCount++; //update actual and virtual step position
                    target = runeStepInputs[stepPos]; //get new target input
                } 
                else
                {
                    getNewRune(true, false); //gets a new random rune - this will be changed to get the next rune in a random sequence
                }
                

            }
            else if (transform.position != currentPoint && stepCount > 0)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentPoint, cursorSpeed * Time.deltaTime);
            }
            else if (stepCount == runeStepCounts[runeID] && direction == Vector2.zero)
            {
                getNewRune(true, false); //gets a new random rune - this will be changed to get the next rune in a random sequence
            }
        }

        void getStepPosition(int ID)
        {
            stepPos = 0;
            for (int i = 0; i < ID; i++)
            {
                stepPos += runeStepCounts[i];
            }

            //return stepPos;
        }

        void getNewRune(bool random, bool onFirstLoad)
        {
            if(random)
            {
                runeID = Random.Range(0, 10); //only update runeID if specified. This allows for easy testing of specific runes.
            }
            stepCount = 0; //reset step count
            getStepPosition(runeID); //get actual step position
            transform.position = new Vector3(runeStarts[runeID][0] - 0f, runeStarts[runeID][1] - 1.5f, 0f); //reset cursor transform, adjusting for new position
            target = runeStepInputs[stepPos]; //set first target direction for new rune
            linkedRune.GetComponent<Rune>().type = runeID;
            if(!onFirstLoad)
            {
                linkedRune.SendMessage("updateSprite", runeID);
            }
        }

        void getLineup()
        {
           for(int i = 0; i < maxCount; i++)
            {
                GameObject currentRuneItem;
                currentRuneItem = Instantiate(lineupRune, new Vector3(-2.25f + (i * 0.5f), -4.5f, 0f), Quaternion.identity); //instantiate runes at positions 0.5x apart from each other
                currentRuneItem.GetComponent<Rune>().id = i;   //get the script and set id
                currentRuneItem.GetComponent<Rune>().type = i; //as well as rune type
                //currentRuneItem.SendMessage("updateSprite", i, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}    
