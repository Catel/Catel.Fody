namespace Catel.Fody;

using System.Collections;
using System.Collections.Generic;

public static class CacheHelper
{
    private static readonly Dictionary<string, IDictionary> CacheByName = new Dictionary<string, IDictionary>();

    public static T GetCache<T>(string name)
        where T : IDictionary, new()
    {
        if (!CacheByName.ContainsKey(name))
        {
            CacheByName[name] = new T();
        }

        return (T)CacheByName[name];
    }

    public static void ClearAllCaches()
    {
        foreach (var cache in CacheByName)
        {
            cache.Value.Clear();
        }
    }
}
