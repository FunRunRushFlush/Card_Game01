using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Event Table")]
public class EventTable : ScriptableObject
{
    public List<EventDefinition> events;
}

public abstract class EventDefinition : ScriptableObject
{
    public string title;
    [TextArea] public string description;

    public abstract void Apply(RunState run);
}
