using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Collection you can draw from like a shuffled deck or bag.
/// </summary>
/// <typeparam name="T">The type of item to store.</typeparam>
[System.Serializable]
public class ShuffleBag<T> {
    [SerializeField] T[] _items;

    // This index of the last chosen item forms a
    // partition between the items "still in the bag" 
    // and those that have recently been drawn.
    int _remaining = 0;

    public ShuffleBag(List<T> items) {
        _items = items.ToArray();
    }

    public ShuffleBag(params T[] items) {
        _items = new T[items.Length];
        items.CopyTo(_items, 0);
    }

    /// <summary>
    /// Draw a random item from the bag, re-shuffling if needed.
    /// This uses a Fisher-Yates shuffle, constant time per draw.
    /// </summary>
    /// <returns>A random item.</returns>
    public T Draw() {
        // Refill the bag if it's empty.
        if (IsEmpty) _remaining = _items.Length;

        // Choose a random item from the bag.
        var index = Random.Range(0, _remaining);
        var selected = _items[index];

        // Already-chosen items are swapped to the end
        // of the array so they're not chosen again
        // until the bag is refilled.
        _remaining--;
        _items[index] = _items[_remaining];
        _items[_remaining] = selected;

        return selected;
    }

    public void UnDraw() {
        _remaining = Mathf.Min(_remaining + 1, _items.Length);
    }

    public bool IsEmpty => _remaining < 1;

    public void PeekOthers(T[] output) {
        if (_items.Length < 2) {
            for (int i = 0; i < output.Length; i++) {
                output[i] = _items[0];
            }
            return;
        }

        int source = 0;
        for (int i = 0; i < output.Length; i++) {
            source--;
            int index = (_remaining - source) % _items.Length;
            if (index < 0) index += _items.Length;
            if (index == _remaining) {
                i--;
                continue;
            }
            output[i] = _items[index];
        }
    }
}
