using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteSheetAnimator))]
[CanEditMultipleObjects]
public class SpriteSheetAnimatorEditor : Editor {

    public override void OnInspectorGUI() {
        // DrawDefaultInspector();

		if(DrawTextureDragDrop<Sprite>(serializedObject.FindProperty("sprites"))) {
            serializedObject.ApplyModifiedProperties();
        }
		// if(DrawFolderDragDrop<AudioClip>(serializedObject.FindProperty("audioClips"))) {
        //     serializedObject.ApplyModifiedProperties();
        // }
        base.OnInspectorGUI();
        // if (GUI.changed && target != null) {
        //     EditorUtility.SetDirty(target);
        // }
    }

    // public override void OnInspectorGUI () {
	// }

    // public static bool DrawFolderDragDrop<T> (SerializedProperty arrayProperty) where T : Object {
    //     if(arrayProperty == null) {
    //         Debug.LogWarning("Property is null!");
    //         return false;
    //     }
    //     if(!arrayProperty.isArray) {
    //         Debug.LogWarning("Property "+arrayProperty.name+" is not array!");
    //         return false;
    //     }
    //     EditorGUILayout.LabelField("Drag and drop folders here", EditorStyles.helpBox, GUILayout.Height(40));
    //     var rect = GUILayoutUtility.GetLastRect();
    //     if(rect.Contains(Event.current.mousePosition)) {
    //         if (Event.current.type == EventType.DragUpdated) {
    //             DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    //             Event.current.Use();
    //         } else if (Event.current.type == EventType.DragPerform) {
    //             DragAndDrop.AcceptDrag();
    //             arrayProperty.ClearArray();
    //             foreach (string path in DragAndDrop.paths) {
    //                 if(System.IO.Directory.Exists(path)) {
    //                     foreach(var assetGUID in AssetDatabase.FindAssets("t:AudioClip", new[]{path})) {
    //                         var loadedAsset = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(assetGUID));
    //                         if(loadedAsset != null) {
    //                             var i = arrayProperty.arraySize;
    //                             arrayProperty.InsertArrayElementAtIndex(i);
    //                             arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = loadedAsset;
    //                         }
    //                     }
    //                 }
    //             }
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    public static bool DrawTextureDragDrop<T> (SerializedProperty arrayProperty) where T : Object {
        if(arrayProperty == null) {
            Debug.LogWarning("Property is null!");
            return false;
        }
        if(!arrayProperty.isArray) {
            Debug.LogWarning("Property "+arrayProperty.name+" is not array!");
            return false;
        }
        EditorGUILayout.LabelField("Drag and drop spritesheets here", EditorStyles.helpBox, GUILayout.Height(40));
        var rect = GUILayoutUtility.GetLastRect();
        if(rect.Contains(Event.current.mousePosition)) {
            if (Event.current.type == EventType.DragUpdated) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            } else if (Event.current.type == EventType.DragPerform) {
                DragAndDrop.AcceptDrag();
                arrayProperty.ClearArray();
                foreach (string path in DragAndDrop.paths) {
                    var objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                    var sprites = objects.Where(q => q is Sprite).Cast<Sprite>();
                    if(sprites != null) {
                        foreach(var sprite in sprites) {
                            var i = arrayProperty.arraySize;
                            arrayProperty.InsertArrayElementAtIndex(i);
                            arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                        }
                    }
                }
                return true;
            }
        }
        return false;
    }
}