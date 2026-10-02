using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class CustomUnityEvent : UnityEvent<Component, object> { }

[Serializable]
public class EventAndResponse
{
    public GameEventSO gameEvent;
    public CustomUnityEvent response;
}

public class EventListener : MonoBehaviour
{

    [Header("Event")]
    public List<EventAndResponse> events;

    // Register to the event
    private void OnEnable()
    {
        foreach (var eventAndResponse in events)
        {
            if (eventAndResponse.gameEvent != null)
                eventAndResponse.gameEvent.RegisterListener(this);
        }
    }

    // Unregister to the event
    private void OnDisable()
    {
        foreach (var eventAndResponse in events)
        {
            if (eventAndResponse.gameEvent != null)
                eventAndResponse.gameEvent.UnregisterListener(this);
        }

    }

    // Safely invoke the linked response event
    public void OnEventRaised(GameEventSO gameEvent, Component sender, object data)
    {
        foreach(var eventAndResponse in events)
        {
            if(eventAndResponse.gameEvent == gameEvent)
                eventAndResponse.response?.Invoke(sender, data);
        }
    }

}
