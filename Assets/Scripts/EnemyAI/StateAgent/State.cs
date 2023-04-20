using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    // Name property where it is set privately
    public string Name { get; private set; }
    public BaseAI Agent { get; private set; }

    public State(string name, BaseAI agent)
    {
        Name = name;
        Agent = agent;
    }

    // Enter
    public abstract void OnEnter();
    // Update
    public abstract void OnUpdate();
    // Exit
    public abstract void OnExit();
}
