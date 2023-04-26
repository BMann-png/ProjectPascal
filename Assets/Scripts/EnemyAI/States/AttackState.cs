using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    public AttackState(string name, BaseAI agent) : base(name, agent) {}

    public override void OnEnter()
    {
        Agent.animator.SetBool("isAttacking", true);
    }

    public override void OnExit()
    {
        Agent.animator.SetBool("isAttacking", false);
    }

    public override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}
