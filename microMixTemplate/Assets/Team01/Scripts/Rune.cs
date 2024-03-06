using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace team01
{
    public class Rune : MicrogameInputEvents
    {

        public Sprite[] runeSprites;
        SpriteRenderer sr;
        // Start is called before the first frame update
        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
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

