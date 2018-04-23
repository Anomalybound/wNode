using System;
using UnityEngine;

public abstract class ResourcesSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                CreateAndLoad();
            }

            return _instance;
        }
    }

    private static void CreateAndLoad()
    {
        var filePath = GetFilePath();
        if (!string.IsNullOrEmpty(filePath))
        {
            _instance = Resources.Load<T>(filePath);
        }
    }

    private static string GetFilePath()
    {
        foreach (var customAttribute in typeof(T).GetCustomAttributes(true))
        {
            var path = customAttribute as ResourcePathAttribute;
            if (path != null)
            {
                return path.filepath;
            }
        }

        return string.Empty;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ResourcePathAttribute : Attribute
{
    public ResourcePathAttribute(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            Debug.LogError("Invalid relative path! (its null or empty)");
        }
        else
        {
            if (relativePath[0] == '/')
            {
                relativePath = relativePath.Substring(1);
            }

            filepath = relativePath;
        }
    }

    public string filepath { get; set; }
}