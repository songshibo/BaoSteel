using UnityEngine;
using UnityEditor;

public class TagMenuItem
{
    [MenuItem("Tools/Generate Tags")]
    private static void TagGenerationMenu()
    {
        string filename = "Tag.txt";
        string config = Util.ReadConfigFile(filename);

        string[] lines = Util.RemoveComments(config.Split('\n'));
        foreach (string item in lines)
        {
            string[] name_model = item.Split(':'); // 所在分组的名字，以及该分组拥有的模型
            string[] ms = name_model[1].Split(' '); // 该分组拥有的模型
            foreach (string m in ms)
            {
                if (m.StartsWith("-")) // 以 ‘-’ 开头为本地模型，先生成 tag，再生成模型，要去掉开头的 ‘-’
                    GenerateTag(m.Trim('-').Trim());
                else
                    GenerateTag(m.Trim());
            }
        }
    }

    [MenuItem("Tools/Remove Tags")]
    private static void RemoveCustomTags()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagProp = tagManager.FindProperty("tags");
        int size = tagProp.arraySize;
        for (int i = 0; i < size; i++)
        {
            Debug.Log("tag deleted:" + tagProp.GetArrayElementAtIndex(0).stringValue);
            tagProp.DeleteArrayElementAtIndex(0);
        }
        tagManager.ApplyModifiedProperties();
        tagManager.Update();
    }

    private static void GenerateTag(string tag)
    {
        if (!IsHasTag(tag))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagProp = tagManager.FindProperty("tags");
            tagProp.InsertArrayElementAtIndex(0);
            tagProp.GetArrayElementAtIndex(0).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            tagManager.Update();
            Debug.Log("tag generated:" + tag);
        }
        else
            Debug.Log("tag existed:" + tag);
    }

    private static bool IsHasTag(string tag)
    {

        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(tag))
                return true;
        }
        return false;
    }
}
