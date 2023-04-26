using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : State
{
    float persist = 5.0f;

    public DeathState(string name, BaseAI agent) : base(name, agent) { }

    public override void OnEnter()
    {
        Debug.Log("Died");
        Agent.animator.SetTrigger("isDead");
        GameObject.Destroy(Agent.gameObject, persist);
    }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUpdate()
    {
        //throw new System.NotImplementedException();
    }
}
