using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureBakerWindow : EditorWindow
{
    GameObject objectToBake;
    int subMeshIndex;

    [MenuItem("Tools/Baking Texture")]
    public static void Init()
    {
        TextureBakerWindow window = GetWindow(typeof(TextureBakerWindow)) as TextureBakerWindow;
        window.minSize = new Vector2(400, 200);
        window.Show();
    }

    private void OnGUI()
    {
        string bakeInfo = "";

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.fontSize = 16;
        GUIStyle errorStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };

        EditorGUILayout.LabelField("Bake Texture", titleStyle, GUILayout.ExpandWidth(true));

        EditorGUILayout.BeginHorizontal();
        objectToBake = EditorGUILayout.ObjectField("Object to Bake:", objectToBake, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        int resolution = EditorGUILayout.IntField("Resolution", 1024);
        EditorGUILayout.EndHorizontal();

        if (objectToBake != null)
        {
            // Choose which submesh to bake or all mesh
            Mesh selectedMesh = objectToBake.GetComponent<MeshFilter>().sharedMesh;
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
            subMeshIndex = EditorGUILayout.IntPopup(subMeshIndex, info, value);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Bake"))
            {
                Bake(selectedMesh, subMeshIndex, resolution);
            }
        }
        else
        {
            bakeInfo = "Pls select mesh before baking.\n";
            errorStyle.normal.textColor = Color.yellow;
        }

        // show info
        GUI.Box(new Rect(0, position.height - 50, position.width - 20, 50), bakeInfo, errorStyle);
    }

    private void Bake(Mesh mesh, int subMesh, int res)
    {
        EditorApplication.ExecuteMenuItem("Edit/Play");

        GameObject obj = new GameObject("TextureBaker");
        TextureBaker baker = obj.AddComponent<TextureBaker>();

        if (baker.LoadMaterials())
        {
            //baker.resolution = res;
            //baker.mesh = mesh;
            //baker.subMeshIndex = subMesh;
            //baker.Bake();
        }

        DestroyImmediate(obj);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
