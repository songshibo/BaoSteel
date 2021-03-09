using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureBaker))]
public class TextureBakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TextureBaker baker = (TextureBaker)target;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.fontSize = 16;

        EditorGUILayout.LabelField("Bake Texture", titleStyle, GUILayout.ExpandWidth(true));

        EditorGUILayout.BeginHorizontal();
        baker.obj2bake = EditorGUILayout.ObjectField("Object to Bake:", baker.obj2bake, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        baker.resolution = EditorGUILayout.IntField("Resolution", baker.resolution);
        EditorGUILayout.EndHorizontal();

        if (baker.obj2bake != null)
        {
            // Choose which submesh to bake or all mesh
            Mesh selectedMesh = baker.obj2bake.GetComponent<MeshFilter>().sharedMesh;
            int subMeshCount = selectedMesh.subMeshCount;
            int[] value = new int[subMeshCount];
            string[] info = new string[subMeshCount];
            for (int i = 0; i < subMeshCount; i++)
            {
                info[i] = "Submesh index <" + i.ToString() + ">";
                value[i] = i;
            }
            // Only display this selection when submesh count > 1;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SubMesh to Bake:");
            baker.subMeshIndex = EditorGUILayout.IntPopup(baker.subMeshIndex, info, value);
            EditorGUILayout.EndHorizontal();
        }
    }
}
