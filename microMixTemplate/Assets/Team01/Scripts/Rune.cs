using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace team01
{
    public class Rune : MicrogameInputEvents
    {

        public Sprite[] runeSprites; //stores all graphics
        SpriteRenderer sr; //gets SpriteRenderer to set graphic
        public int id;   //rune ID
        public int type; //and type, to be accessed and set when instantiating lineup prefabs
        // Start is called before the first frame update
        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            updateSprite(type);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void updateSprite(int runeID)
        {
            sr.sprite = runeSprites[runeID];
        }
    }
}

