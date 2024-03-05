using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpinnerWidget : MonoBehaviour {

    [SerializeField] TextMeshProUGUI _header;
    [SerializeField] SpinnerEntry[] _entries;

    [SerializeField] int _visibleFlankers;

    [SerializeField] AudioSource _audio;

    [SerializeField] SoundCollection _tickSounds;

    [SerializeField] SoundCollection _selectSound;

    public int EntryCount => _entries.Length;
    public int spinTicks = 20;
    public bool startOffScreen;

    public float spinDuration;

    int _lastSelectedIndex;

    public void Populate(int index, Sprite sprite, string text = "") {
        _entries[index].label.text = text;     // $"{index}\n{text}";
        _entries[index].image.sprite = sprite;
    }

    public IEnumerator Spin(int finishOn = -1, bool shuffle = false) {
        gameObject.SetActive(true);

        if (finishOn < 0) {
            finishOn = Random.Range(0, _entries.Length);
        }

        if (shuffle) {
            var toFinish = _entries[finishOn];
            _entries.Shuffle();
            finishOn = System.Array.IndexOf(_entries, toFinish);
        }
        _lastSelectedIndex = finishOn;

        var rt = (RectTransform)transform;
        float width = rt.rect.width;
        float spacing = width / (2 * _visibleFlankers);
        
        int startIndex = (finishOn - spinTicks) % _entries.Length;
        if (startIndex < 0) startIndex += _entries.Length;

        float speed = spinTicks / spinDuration;

        const float flankerAlpha = 0.3f;
        const float selectedScale = 1.5f;

        int previousStep = -1;
        float t = 1;

        int hideBefore = startOffScreen ? startIndex + _visibleFlankers : -1;
        for (;t > 0; t -= speed * Time.deltaTime) {
            float progress = startIndex + spinTicks * Slowdown(t);
            int step = PositionEntries(progress, spacing, flankerAlpha, hideBefore);
            if (step != previousStep) {
                for (int i = previousStep; i < step; i++) {
                    _entries[(i - _visibleFlankers + _entries.Length) % _entries.Length].gameObject.SetActive(false);
                }
                _audio.PlayOneShot(_tickSounds.SelectNonRepeating());                  
                previousStep = step;
            }
            yield return null;
        }

        _audio.PlayOneShot(_selectSound.SelectNonRepeating());
        for (t = 0; t < 1f; t += Time.deltaTime / 0.5f) {
             
            float reveal = Mathf.SmoothStep(0, 1, t);         
            _header.alpha = reveal; 
            PositionEntries(startIndex + spinTicks, spacing, flankerAlpha * (1 - reveal), -1);
            float scale  = Mathf.SmoothStep(1f, selectedScale, reveal);
            _entries[finishOn].rectTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        _header.alpha = 1f;
        
        _entries[finishOn].rectTransform.localScale = new Vector3(selectedScale, selectedScale, selectedScale);
        for (int i = 1; i <= _visibleFlankers; i++) {
            _entries[(finishOn + i) % _entries.Length].gameObject.SetActive(false);
            _entries[(finishOn - i + _entries.Length) % _entries.Length].gameObject.SetActive(false);
        }
    }

    float Slowdown(float t) {
        return 1f - t * t;
    }

    public IEnumerator FadeOut(float duration) {
        float speed = 1f/duration;
        for (float t = 1f; t > 0f; t -= speed * Time.deltaTime) {
            _entries[_lastSelectedIndex].alpha = t;
            _header.alpha = t * t;
            yield return null;
        }
        _entries[_lastSelectedIndex].gameObject.SetActive(false);
        _entries[_lastSelectedIndex].alpha = 0f;
        gameObject.SetActive(false);
    }

    int PositionEntries(float progress, float spacing, float flankerAlpha, int offScreenBefore) {

        int step = Mathf.RoundToInt(progress);
        float frac = progress - step;
        for (int shift = - _visibleFlankers; shift <= _visibleFlankers; shift++) {
            int index = step + shift;
            if (index < offScreenBefore) continue;

            index = (index + _entries.Length) % _entries.Length;

            var entry = _entries[index];

            float phase = (shift - frac)/_visibleFlankers;
            float absPhase = Mathf.Abs(phase);
            if (absPhase >= 1) {
                entry.gameObject.SetActive(false);
                entry.alpha = 0;
                continue;
            } else {
                entry.gameObject.SetActive(true);
            }

            var rt = entry.rectTransform;
            var pos = rt.anchoredPosition;
            pos.x = spacing * Mathf.SmoothStep(-_visibleFlankers, _visibleFlankers, phase * 0.5f + 0.5f);
            rt.anchoredPosition = pos;

            bool centered = shift == 0; // && (Mathf.Abs(frac) < 0.2f);
            float alpha = (1f - absPhase * absPhase) * (centered ? 1f : flankerAlpha);
            entry.alpha = alpha;

            float scale = Mathf.Lerp(0.2f, 1.0f, (1f - absPhase * absPhase) * (centered ? 1f : 0.9f));
            rt.localScale = new Vector3(scale, scale, scale);
        }

        return step;
    }
}