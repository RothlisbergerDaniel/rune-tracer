using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpinnerEntry : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Image image;

    public RectTransform rectTransform => (RectTransform)transform;

    public float alpha {
        set {
            label.alpha = value;
            var c = image.color;
            c.a = value;
            image.color = c;
        }
    }
}
