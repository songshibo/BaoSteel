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
            switch (source)
            {
                case SOURCE.HeatMap:
                    name += "_Heatmap";
                    if (!TextureProcessor.SaveTextoPNG(HeatmapUpdater.Instance.HeatMap, name, path))
                    {
                        infoColor = Color.red;
                        info = "Fail to export HeatMap texture! (Maybe it hasn't been generated)";
                    }
                    else
                    {
                        infoColor = Color.green;
                        info = "Successfully exported to " + path;
                        if (File.Exists(path + name + ".png"))
                            info += "(Overwrite)";
                        EditorUtility.RevealInFinder(path + name + ".png");
                    }
                    break;
                case SOURCE.HeatLoad:
                    name += "_Heatload";
                    if (!TextureProcessor.SaveTextoPNG(HeatLoadUpdater.Instance.Heatload, name, path))
                    {
                        infoColor = Color.red;
                        info = "Fail to export HeatLoad texture! (Maybe it hasn't been generated)";
                    }
                    else
                    {
                        infoColor = Color.green;
                        info = "Successfully exported to " + path;
                        if (File.Exists(path + name + ".png"))
                            info += "(Overwrite)";
                        EditorUtility.RevealInFinder(path + name + ".png");
                    }
                    break;
                default:
                    break;
            }
        }



        if (GUILayout.Button("Reveal in folder"))
        {
            EditorUtility.RevealInFinder(path);
        }

        GUIStyle errorStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        errorStyle.normal.textColor = infoColor;
        GUI.Box(new Rect(0, 130, position.width - 20, 50), info, errorStyle);
    }
}
