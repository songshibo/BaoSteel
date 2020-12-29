using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureEditorWindow : EditorWindow
{
    public enum SOURCE { HeatMap, HeatLoad }
    public SOURCE source;
    string info;
    Color infoColor;

    [MenuItem("Tools/Export Texture")]
    public static void Init()
    {
        TextureEditorWindow window = GetWindow(typeof(TextureEditorWindow)) as TextureEditorWindow;
        window.minSize = new Vector2(400, 200);
        window.Show();
    }

    private void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.fontSize = 16;

        // Title
        EditorGUILayout.LabelField("Export Texture", titleStyle, GUILayout.ExpandWidth(true));

        EditorGUILayout.BeginHorizontal();
        source = (SOURCE)EditorGUILayout.EnumPopup("Texture Source:", source);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target path:", GUILayout.MaxWidth(80f));
        string path = EditorGUILayout.TextField(Application.dataPath.Replace("Assets", "ExportedTextures/"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target name:", GUILayout.MaxWidth(80f));
        string name = EditorGUILayout.TextField("texture");
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Export"))
        {
            name += "_" + Util.ComputeFileIndex(path, name, ".png").ToString();
            switch (source)
            {
                case SOURCE.HeatMap:
                    if (!TextureProcessor.SaveTextoPNG(HeatmapUpdater.Instance.HeatMap, name, path))
                    {
                        infoColor = Color.red;
                        info = "Fail to export HeatMap texture! (Maybe it hasn't been generated)";
                    }
                    else
                    {
                        infoColor = Color.white;
                        info = "Successfully exported to " + path;
                        EditorUtility.RevealInFinder(path + name + ".png");
                    }
                    break;
                case SOURCE.HeatLoad:
                    infoColor = Color.yellow;
                    info = "Export HeatLoad is not supported yet";
                    break;
                default:
                    break;
            }
        }

        GUIStyle errorStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        errorStyle.normal.textColor = infoColor;
        GUI.Box(new Rect(0, 110, position.width - 20, 50), info, errorStyle);
    }
}
