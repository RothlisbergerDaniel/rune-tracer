using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace team01
{
    public class Rune : MicrogameInputEvents
    {

        public Sprite[] runeSprites; //stores unlit graphics
        public Sprite[] litSprites; //stores lit graphics
        SpriteRenderer sr; //gets SpriteRenderer to set graphic
        public int id = -1;   //rune ID
        public int type; //and type, to be accessed and set when instantiating lineup prefabs

        public GameObject controller;
        // Start is called before the first frame update
        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            updateSprite(type);
        }

        // Update is called once per frame
        void Update()
        {
            if(controller.GetComponent<Trace>().currentRune > id && id > -1) //checks id > -1 to differentiate between prefabs and main object with the same script
            {
                sr.sprite = litSprites[type];
            }
        }

        void updateSprite(int runeID)
        {
            sr.sprite = runeSprites[runeID];
        }
    }
}

