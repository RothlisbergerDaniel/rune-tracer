using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TexturePreviewAttribute))]
public class TexturePreviewDrawer : PropertyDrawer
{
    enum ValueType {
        Unknown,
        Other,
        Sprite,
        Texture2D
    }

    ValueType valueType;
    float imageSize;

    bool IsApplicable(SerializedProperty property) {
        switch (valueType) {
            case ValueType.Unknown:
                if (property.propertyType != SerializedPropertyType.ObjectReference) {
                    valueType = ValueType.Other;
                    return false;
                }

                var texPreview = (TexturePreviewAttribute)attribute;
                imageSize = texPreview.size * EditorGUIUtility.singleLineHeight;
                var value = property.objectReferenceValue;
                switch (value) {
                    case Sprite s:  valueType = ValueType.Sprite; return true;
                    case Texture2D t: valueType = ValueType.Texture2D; return false;
                }

                valueType = ValueType.Other;
                imageSize = 0;
                return false;
            case ValueType.Sprite:
            case ValueType.Texture2D:
                return true;
            default:
                return false;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        IsApplicable(property);
        return EditorGUI.GetPropertyHeight(property) + imageSize;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);

        Texture2D tex = null;
        Rect uv = new Rect(0, 0, 1, 1);
        float aspect = 1f;

        switch(valueType) {
            case ValueType.Sprite:
                var sprite = (Sprite)property.objectReferenceValue;
                if (sprite != null) {
                    tex = sprite.texture;
                    uv = sprite.textureRect;
                    aspect = uv.width / uv.height;
                    uv.xMin /= tex.width;
                    uv.width /= tex.width;
                    uv.yMin /= tex.height;
                    uv.height /= tex.height;

                    //Debug.Log(aspect);
                }
                break;
            case ValueType.Texture2D:
                tex = (Texture2D)property.objectReferenceValue;
                aspect = tex.width / tex.height;
                break;
            default:
                return;
        }

        var thumbLeft = position.xMax - imageSize;        
        var thumbPosition = position;
        thumbPosition.xMin = thumbLeft;
        thumbPosition.width = imageSize;
        thumbPosition.yMin += EditorGUIUtility.singleLineHeight;
        thumbPosition.height = imageSize;

        if (aspect > 1f) {
            float size = imageSize / aspect;
            float mid = thumbPosition.center.y;
            thumbPosition.yMin = mid - size * 0.5f;
            thumbPosition.height = size;
        } else if (aspect < 1f) {
            float size = imageSize * Mathf.Sqrt(aspect);
            float mid = thumbPosition.center.x;
            thumbPosition.xMin = mid - size * 0.5f;
            thumbPosition.width = size;
        }

        GUI.DrawTextureWithTexCoords(thumbPosition, tex, uv);
    }
}
