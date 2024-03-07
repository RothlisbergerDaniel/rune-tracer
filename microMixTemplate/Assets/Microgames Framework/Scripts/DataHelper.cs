using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DataHelper
{

    public enum DigitalDirection : byte {
        None,
        Right,
        UpRight,
        Up,
        UpLeft,
        Left,
        DownLeft,
        Down,
        DownRight
    }

    private static DigitalDirection[] DIRECTION_DECODE = new[]{
        DigitalDirection.DownLeft,
        DigitalDirection.Down,
        DigitalDirection.DownRight,
        DigitalDirection.Left,
        DigitalDirection.None,
        DigitalDirection.Right,
        DigitalDirection.UpLeft,
        DigitalDirection.Up,
        DigitalDirection.UpRight
    };

    public static DigitalDirection Get8Way(this Vector2 v) {
        int x = System.Math.Sign(v.x);
        int y = System.Math.Sign(v.y);
        int code = 4 + x + 3 * y;
        return DIRECTION_DECODE[code];
    }

    /// <summary>
    /// Number of 45-degree hops between this direction and the one passed as a parameter.
    /// </summary>
    /// <param name="from">The start direction.</param>
    /// <param name="to">The end direction</param>
    /// <returns>An integer from -3 (3 hops clockwise) to +4 (4 hops counter-clockwise). Returns zero if either direction was None.</returns>
    public static int GetRotationTo(this DigitalDirection from, DigitalDirection to) {
        if (from == DigitalDirection.None || to == DigitalDirection.None) return 0;
        int difference = (int)to - (int)from;
        if (difference > 4) difference -=8;
        else if (difference <= -4) difference += 8;
        return difference;
    }

    public static int NumberOfTrailingZeros(this int i)
    {
        return TRAILING_ZERO_LOOKUP[(i & -i) % 37];
    }

    private static byte[] TRAILING_ZERO_LOOKUP = new byte[]{
        32, 0, 1, 26, 2, 23, 27, 0, 3, 16, 24, 30, 28, 11, 0, 13, 4, 7, 17,
        0, 25, 22, 31, 15, 29, 10, 12, 6, 0, 21, 14, 9, 5, 20, 8, 19, 18
    };

    public static int FirstLayerId(this LayerMask mask) {
        return mask.value.NumberOfTrailingZeros();
    }

    /// <summary>
    /// Selects a random element from the list or array.
    /// </summary>
    /// <typeparam name="T">The type the collection holds.</typeparam>
    /// <param name="list">The list or array to sample from.</param>
    /// <returns>A single random item.</returns>
    public static T Random<T>(this IList<T> list) {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Randomizes the order of elements in an array or list in-place. 
    /// (Uses a Fisher-Yates / "Knuth" shuffle algorithm, O(n))
    /// </summary>
    /// <typeparam name="T">The type the collection holds.</typeparam>
    /// <param name="list">The list or array to shuffle.</param>
    public static void Shuffle<T>(this IList<T> list) {
        int remain = list.Count;
        while (remain > 1) {
            int index = UnityEngine.Random.Range(0, remain);
            T selected = list[index];
            remain--;
            list[index] = list[remain];
            list[remain] = selected;
        }
    }

#if UNITY_EDITOR
    public static void MarkChangesForSaving(Object target, string reason)
    {
        Undo.RecordObject(target, reason);
        EditorUtility.SetDirty(target);
    }

    public static T GetOrCreateAsset<T>(string name, Dictionary<string, T> existing, string folder, string reason) where T : ScriptableObject {
        if (!existing.TryGetValue(name, out T asset)) {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, $"Assets/{folder}/{name}.asset");
            existing.Add(name, asset);
        }

        MarkChangesForSaving(asset, reason);
        return asset;
    }

    public static Dictionary<string, T> GetNamedAssetsOfType<T>() where T : Object {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var collection = new Dictionary<string, T>(guids.Length);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            collection.Add(asset.name, asset);
        }

        return collection;
    }

    public static List<T> GetAllAssetsOfType<T>() where T : Object {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var collection = new List<T>(guids.Length);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            collection.Add(asset);
        }

        return collection;
    }

    public static List<T> GetAllAssetsOfType<T>(string searchPath) where T : Object {

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[]{ searchPath });
        var collection = new List<T>(guids.Length);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            collection.Add(asset);
        }

        return collection;
    }

    public static T GetFirstAssetOfType<T>() where T:Object {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (guids.Length == 0) {
            Debug.LogError($"No assets of type {typeof(T).Name} found in project.");
            return null;
        } 
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return asset;
    }

    public static T GetFirstAssetOfType<T>(string searchPath) where T:Object {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { searchPath });
        if (guids.Length == 0) {
            Debug.LogError($"No assets of type {typeof(T).Name} found in {searchPath}.");
            return null;
        } 
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        return asset;
    }
#endif
}
