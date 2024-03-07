using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace team01
{
    public class Lineup : MicrogameEvents
    {
        public Sprite[] runeSprites; //stores unlit graphics
        public Sprite[] litSprites; //stores lit graphics
        SpriteRenderer sr; //gets SpriteRenderer to set graphic
        public int id = 0;   //rune ID
        public int type = 0; //and type, to be accessed and set when instantiating lineup prefabs

        public GameObject controller;
        // Start is called before the first frame update
        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (id != -1)
            {
                transform.position = new Vector3(-2.25f + 0.5f * id, -4.5f, 0f); //reset position
                transform.localScale = new Vector3(0.8f, 0.8f, 1f);              //and transform so that the children don't follow the cursor (the parent object)
                if (controller.GetComponent<Trace>().currentRune > id) //checks id > -1 to differentiate between prefabs and main object with the same script
                {
                    sr.sprite = litSprites[type];
                }
            }

        }
    }

}