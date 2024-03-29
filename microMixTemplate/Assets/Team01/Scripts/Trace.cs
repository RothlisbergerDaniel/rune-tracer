//using System;
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
        public GameObject lineupController; //links to the lineup controller

        Vector2 direction; //current user direction input
        Vector2 target; //target direction input
        public float cursorSpeed = 100f; //speed at which the cursor snaps to points, default 10

        public int[] runeDifficulties = new int[10]; //list of rune difficulties by ID in order from low to high
        public int[] runeTypes; //list of rune types
        public int maxCount; //max number of runes to complete
        public int currentRune; //current rune in sequence
        bool gameCompleted; //checks whether the game is completed or not

        TrailRenderer tr; //TrailRenderer for tracing out rune shapes

        public AudioSource[] strokes; //stroke sounds for tracing
        public AudioSource lose; //loss
        public AudioSource win;  //and win sfx
        public AudioSource runeComplete; //rune completion sfx
        public AudioSource bgm; //background music

        public GameObject portal; //portal for animation
        SpriteRenderer srPortal;  //portal SpriteRenderer
        Animator animPortal;

        public GameObject lightning1; //lightning gameObjects
        ParticleSystem psLightning1;  //and particle systems
        public GameObject lightning2;
        ParticleSystem psLightning2;

        public GameObject burst;  //burst gameObject
        ParticleSystem psBurst; //and particle system
        
        // Start is called before the first frame update
        void Start()
        {
            tr = GetComponent<TrailRenderer>();
            //getStepPosition(runeID);
            runeTypes = new int[10]; //refresh rune type list
            getLineup(); //get new rune lineup
            getNewRune(runeTypes[currentRune], true); //replace this with hiding cursor on Start, then show cursor OnGameStart?
            gameCompleted = false; //make sure the game can be completed

            srPortal = portal.GetComponent<SpriteRenderer>();
            srPortal.enabled = false; //get and disable portal SpriteRenderer
            animPortal = portal.GetComponent<Animator>();
            //animPortal.SetBool("Open", false); //get and disable animation
            animPortal.ResetTrigger("End");

            psLightning1 = lightning1.GetComponent<ParticleSystem>();
            psLightning2 = lightning2.GetComponent<ParticleSystem>();
            psBurst = burst.GetComponent<ParticleSystem>();
        }
        protected override void OnGameStart()
        {
            //base.OnGameStart();
            getNewRune(runeTypes[currentRune], false);
            bgm.Play();
        }


        // Update is called once per frame
        void Update()
        {
            
            direction = stick; //direction variable is set to stick input
            Vector3 currentPoint = new Vector3(runePoints[Mathf.Clamp(stepPos - 1, 0, 100)][0] - 0f, runePoints[Mathf.Clamp(stepPos - 1, 0, 100)][1] - 1.5f, 0f); //convert preset point to new position, scalabale to 2P
            if ((target[0] == 0 && direction[1] == target[1] || target[1] == 0 && direction[0] == target[0]) && (transform.position == currentPoint || stepCount == 0)) //use Mathf.Clamp to ensure no index out-of-bounds errors
            {
                if (stepCount < runeStepCounts[runeID])
                {
                    stepPos++;
                    //stepPos = Mathf.Clamp(stepPos, 0, runePoints.Length); //clamp so it doesn't go out-of-bounds
                    stepCount++; //update actual and virtual step position
                    target = runeStepInputs[stepPos]; //get new target input
                    strokes[Random.Range(0, 3)].Play(); //plays a random stroke sound
                } 
                else
                {
                    currentRune++;
                    if (currentRune < maxCount)
                    {
                        getNewRune(runeTypes[currentRune], false); //gets the next rune in the sequence
                        runeComplete.Play();
                        psBurst.Play();
                    }
                    else if (!gameCompleted)
                    {
                        gameCompleted = true;
                        ReportGameCompletedEarly(); //end game if all runes are completed early
                        win.Play();
                        srPortal.enabled = true;
                        //animPortal.SetBool("Open", true);
                        animPortal.SetTrigger("End");
                        
                        tr.Clear();
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
                    runeComplete.Play();
                    psBurst.Play();
                }
                else if (!gameCompleted)
                {
                    gameCompleted = true;       //prevent game from continually reporting early completion
                    ReportGameCompletedEarly(); //then report early completion
                    win.Play();
                    srPortal.enabled = true;
                    //animPortal.SetBool("Open", true); //send win feedback
                    animPortal.SetTrigger("End");

                    tr.Clear();
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
            tr.Clear();
            target = runeStepInputs[stepPos]; //set first target direction for new rune
            linkedRune.GetComponent<Rune>().type = runeID; //sets linked rune's type
            if(!onFirstLoad) //prevent errors by not updating on first load
            {
                linkedRune.SendMessage("updateSprite", runeID);
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
                currentRuneItem = Instantiate(lineupRune, transform); //instantiate runes at positions 0.5x apart from each other
                currentRuneItem.GetComponent<Lineup>().id = i;   //get the script and set id
                currentRuneItem.GetComponent<Lineup>().type = runeTypes[i]; //as well as rune type
                currentRuneItem.GetComponent<SpriteRenderer>().sprite = currentRuneItem.GetComponent<Lineup>().runeSprites[runeTypes[i]]; //holy crap I hate this I hate this I hate this
                currentRuneItem.GetComponent<Lineup>().controller = gameObject;
                //currentRuneItem.SendMessage("updateSprite", i, SendMessageOptions.DontRequireReceiver); //uncomment this to throw an error :skull:
            }
            currentRune = 0; //reset current rune
        }

        protected override void OnTimesUp()
        {
            base.OnTimesUp();
            lose.Play();
            psLightning1.Play();
            psLightning2.Play();
        }
    }
}    
