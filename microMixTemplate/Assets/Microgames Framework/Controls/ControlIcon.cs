using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlIcon : MonoBehaviour
{

    public Image control;
    public Image overlay;

    [SerializeField] Sprite[] _sprites;
    [SerializeField] float _animationSpeed;

    void Update() {
        float animPhase = Mathf.PingPong(Time.time * _animationSpeed, _sprites.Length);
        control.sprite = _sprites[Mathf.Min(Mathf.FloorToInt(animPhase), _sprites.Length-1)];
    }
}
