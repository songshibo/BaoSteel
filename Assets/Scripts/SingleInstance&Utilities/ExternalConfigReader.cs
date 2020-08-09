using UnityEngine;
using System.IO;

public class ExternalConfigReader
{
    private static ExternalConfigReader instance = new ExternalConfigReader();

    private ExternalConfigReader()
    {

    }

    public static ExternalConfigReader Instance()
    {
        return instance;
    }

    /// <summary>
    /// 读取Application.datapath/Config/ 下的对应txt文件，将内容返回为一个string
    /// </summary>
    /// <param name="filename">txt file name</param>
    /// <returns>A string object contains all lines of txt file</returns>
    public string ReadConfigFile(string filename)
    {
        string path = Application.dataPath + "/Config/" + filename;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                StreamReader streamReader = new StreamReader(fs, true);
                return streamReader.ReadToEnd();
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError(ex);
            return null;
        }
    }
}
