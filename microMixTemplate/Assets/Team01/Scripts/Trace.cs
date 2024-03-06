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
        public int[] runeTypes; //list of rune types
        public int maxCount; //max number of runes to complete
        public int currentRune; //current rune in sequence
        
        // Start is called before the first frame update
        void Start()
        {
            //getStepPosition(runeID);
            runeTypes = new int[10];
            getLineup();
            getNewRune(runeTypes[currentRune], true); //replace this with hiding cursor on Start, then show cursor OnGameStart
        }
        protected override void OnGameStart()
        {
            //base.OnGameStart();
            getNewRune(runeTypes[currentRune], false);
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
                    currentRune++;
                    if (currentRune < maxCount)
                    {
                        getNewRune(runeTypes[currentRune], false); //gets the next rune in the sequence
                    }
                    else
                    {
                        ReportGameCompletedEarly(); //end game if all runes are completed early
                    }
                }
                

            }
            else if (transform.position != currentPoint && stepCount > 0)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentPoint, cursorSpeed * Time.deltaTime);
            }
            else if (stepCount == runeStepCounts[runeID] && direction == Vector2.zero)
            {
                currentRune++;
                if (currentRune < maxCount)
                {
                    getNewRune(runeTypes[currentRune], false);
                }
                else
                {
                    ReportGameCompletedEarly();
                }
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

        void getNewRune(int type, bool onFirstLoad)
        {
            //if(random)
            //{
            //    runeID = Random.Range(0, 10); //only update runeID if specified. This allows for easy testing of specific runes.
            //}
            runeID = type;
            stepCount = 0; //reset step count
            getStepPosition(runeID); //get actual step position
            transform.position = new Vector3(runeStarts[runeID][0] - 0f, runeStarts[runeID][1] - 1.5f, 0f); //reset cursor transform, adjusting for new position
            target = runeStepInputs[stepPos]; //set first target direction for new rune
            linkedRune.GetComponent<Rune>().type = runeID; //sets linked rune's type
            if(!onFirstLoad) //prevent errors by not updating on first load
            {
                linkedRune.SendMessage("updateSprite", runeID);
                linkedRune.GetComponent<Rune>().id = -1;
            }
        }

        void getLineup()
        {
            for(int i = 0; i < maxCount; i++)
            {
                if(i < Mathf.Round(maxCount / 10 * 4))
                {
                    runeTypes[i] = runeDifficulties[Random.Range(0, 4)]; //grab the rune type from a list of difficulties with a difficulty value of 1
                }
                else if(i < Mathf.Round(maxCount / 10 * 8))
                {
                    runeTypes[i] = runeDifficulties[Random.Range(4, 8)]; //difficulty 2
                }
                else
                {
                    runeTypes[i] = runeDifficulties[Random.Range(8, 10)]; //difficulty 3
                }
            }
               
            for(int i = 0; i < maxCount; i++)
            {
                GameObject currentRuneItem;
                currentRuneItem = Instantiate(lineupRune, new Vector3(-2.25f + (i * 0.5f), -4.5f, 0f), Quaternion.identity); //instantiate runes at positions 0.5x apart from each other
                currentRuneItem.GetComponent<Rune>().id = i;   //get the script and set id
                currentRuneItem.GetComponent<Rune>().type = runeTypes[i]; //as well as rune type
                currentRuneItem.GetComponent<Rune>().controller = gameObject;
                //currentRuneItem.SendMessage("updateSprite", i, SendMessageOptions.DontRequireReceiver); //uncomment this to throw an error :skull:
            }
            currentRune = 0; //reset current rune
        }
    }
}    
