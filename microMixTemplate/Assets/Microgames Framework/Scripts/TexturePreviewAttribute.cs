using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class TexturePreviewAttribute : PropertyAttribute
{
    readonly public float size;
    public TexturePreviewAttribute(float size = 3f) {
        this.size = size;
    }
}
