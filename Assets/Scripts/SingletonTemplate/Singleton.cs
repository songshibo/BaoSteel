using UnityEngine;

public class Singleton<T> where T : new()
{
    private static T instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                        instance = new T();
                }
            }
            return instance;
        }
    }
}
