using UnityEngine;

/// <summary>
/// Simple scene-scoped singleton.
/// Ensures a single instance of <typeparamref name="T"/> exists within the current scene.
/// </summary>
/// <typeparam name="T">Component type using this singleton.</typeparam>
public class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;
    
    /// <summary>
    /// Indicates whether the singleton instance is initialized.
    /// </summary>
    /// <returns>True if an instance exists, otherwise False.</returns>
    public static bool IsInitialized => _instance != null;
    
    /// <summary>
    /// Returns the instance if it exists, otherwise null.
    /// </summary>
    /// <returns>The singleton instance or null if not initialized.</returns>
    public static T TryGetInstance => IsInitialized ? _instance : null;
    
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    /// <returns>The existing instance or a newly created one if none exists.</returns>
    public static T Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            _instance = FindAnyObjectByType<T>();
            if (_instance != null)
            {
                return _instance;
            }
            
            var singletonObject = new GameObject(typeof(T).Name + " (Auto Generated)");
            _instance = singletonObject.AddComponent<T>();
            
            return _instance;
        }
    }

    /// <summary>
    /// Initializes the singleton instance for the current scene.
    /// </summary>
    protected virtual void initializeSingleton()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        _instance = this as T;
    }


    protected virtual void Awake()
    {
        initializeSingleton();
    }
}