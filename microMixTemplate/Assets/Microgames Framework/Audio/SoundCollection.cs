using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newSoundCollection.asset", menuName = "microMix/Sound Collection")]
public class SoundCollection : ScriptableObject
{
    public AudioClip[] sounds;

    int _lastSelectedIndex = int.MaxValue;

    public AudioClip SelectNonRepeating() {
        if (sounds.Length == 1) return sounds[0];

        int selected = Random.Range(0, sounds.Length-1);
        if (selected >= _lastSelectedIndex) selected++;
        _lastSelectedIndex = selected;
        return sounds[selected];
    }
}
