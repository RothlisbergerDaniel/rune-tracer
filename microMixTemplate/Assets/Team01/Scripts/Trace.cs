using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace team01
{
    public class Trace : MicrogameInputEvents
    {

        public Vector3[] runeStarts = new Vector3[10]; //tracks start points as a Vector3, i.e. where to place the pointer when a new rune loads
        public int[] runeStepCounts = new int[10]; //tracks number of steps needed to complete the rune
        public Vector3[] runePoints = new Vector3[38]; //tracks vertices of each rune, i.e. where the pointer targets at each step
        public Vector2[] runeStepInputs = new Vector2[38]; //stores inputs required to complete each rune, in terms of joystick values
        public int stepPos; //tracks step position based on rune ID #, previous steps taken, etc.
        int stepCount; //tracks current steps taken to compare against current rune's total step count

        public int runeID = 0; //public for testing purposes

        Vector2 direction;
        Vector2 target;
        // Start is called before the first frame update
        void Start()
        {
            //getStepPosition(runeID);
        }
        protected override void OnGameStart()
        {
            //base.OnGameStart();
            getNewRune(false);
        }


        // Update is called once per frame
        void Update()
        {
            direction = stick; //direction variable is set to stick input
            if(direction == target)
            {
                if(stepCount < runeStepCounts[runeID])
                {
                    stepPos++;
                    stepCount++; //update actual and virtual step position
                } 
                else
                {
                    getNewRune(false); //gets a new random rune - this will be changed to get the next rune in a random sequence
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

        void getNewRune(bool random)
        {
            if(random)
            {
                runeID = Random.Range(0, 10); //only update runeID if specified. This allows for easy testing of specific runes.
            }
            stepCount = 0; //reset step count
            getStepPosition(runeID); //get actual step position
            transform.position = runeStarts[runeID]; //reset cursor transform
            target = runeStepInputs[stepPos]; //set first target direction for new rune
        }
    }
}    
