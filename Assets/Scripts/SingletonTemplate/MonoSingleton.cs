using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    /// <summary>
    /// Thread lock
    /// </summary>
    private static readonly object _lock = new object();
    /// <summary>
    /// Is Application quitting
    /// </summary>
    protected static bool IsQuitting { get; private set; }
    /// <summary>
    /// Is Golbal singleton
    /// </summary>
    protected static bool isGolbal = true;

    static MonoSingleton()
    {
        IsQuitting = false;
    }

    public static T Instance
    {
        get
        {
            if (IsQuitting)
            {
                if (Debug.isDebugBuild)
                    Debug.LogWarning("[Singleton] " + typeof(T) + " already destroyed on application quit." + " ----return null");
                return null;
            }

            lock (_lock)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    // check instance number
                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        if (Debug.isDebugBuild)
                            Debug.LogWarning("[Singleton] " + typeof(T) + " exist more than 1 instance.");
                    }

                    if (instance == null)
                    {
                        GameObject monoSingleton = new GameObject();
                        instance = monoSingleton.AddComponent<T>();
                        monoSingleton.name = typeof(T) + "(MonoSingleton)";

                        if (isGolbal && Application.isPlaying)
                            DontDestroyOnLoad(monoSingleton);

                        return instance;
                    }
                }

                return instance;
            }
        }
    }
}
