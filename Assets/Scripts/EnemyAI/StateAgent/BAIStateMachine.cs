using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAIStateMachine
{
    public State CurrentState { get; private set; }
    private readonly Dictionary<State, List<KeyValuePair<Transition, State>>> stateTransitions = new();

    public void OnUpdate()
    {
        if (CurrentState == null)
            return;

        var transitions = stateTransitions[CurrentState];

        foreach (var transition in transitions)
        {
            if (transition.Key.CheckConditions())
            {
                SetState(transition.Value);
                break;
            }
        }

        CurrentState.OnUpdate();
    }

    public void AddState(State state)
    {
        if (!stateTransitions.ContainsKey(state))
        {
            stateTransitions[state] = new List<KeyValuePair<Transition, State>>();
        }
    }

    public void AddTransition(State stateFrom, Transition transition, State stateTo)
    {
        if (stateTransitions.ContainsKey(stateFrom))
        {
            var transitions = stateTransitions[stateFrom];
            transitions.Add(new KeyValuePair<Transition, State>(transition, stateTo));
        }
    }

    public void AddTransition(string stateFrom, Transition transition, string stateTo)
    {
        if (stateTransitions.ContainsKey(GetState(stateFrom)))
        {
            var transitions = stateTransitions[GetState(stateFrom)];
            transitions.Add(new KeyValuePair<Transition, State>(transition, GetState(stateTo)));
        }
    }

    public void SetState(State state)
    {
        if (CurrentState == state || state == null)
            return;

        CurrentState?.OnExit();
        CurrentState = state;
        CurrentState.OnEnter();
    }

    public State GetState(string name)
    {
        foreach (var state in stateTransitions)
        {
            if (string.Equals(state.Key.Name, name, System.StringComparison.OrdinalIgnoreCase))
                return state.Key;
        }

        return null;
    }

    public string GetStateName()
    {
        return CurrentState?.Name;
    }
}
