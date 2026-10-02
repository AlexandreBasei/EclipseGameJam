using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Event System/GameEvent")]
public class GameEventSO : ScriptableObject
{

    [System.NonSerialized]
    private readonly List<EventListener> listeners = new List<EventListener>();


    public void RegisterListener(EventListener listener) 
    {

        if (listener == null) return;

        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(EventListener listener){ listeners.Remove(listener); }

    public void Raise(Component sender, object data)
    {
        for (int i = listeners.Count -1;  i >= 0; i--) {
            var listener = listeners[i];
            listener.OnEventRaised(this, sender, data);
        }
    }
    
}