using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : State
{
    public DeathState(string name, BaseAI agent) : base(name, agent) { }

    public override void OnEnter()
    {
        Agent.animator.SetTrigger("isDead");
    }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}
