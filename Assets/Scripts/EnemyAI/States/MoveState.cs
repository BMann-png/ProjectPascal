using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public MoveState(string name, BaseAI agent) : base(name, agent) { }

    public override void OnEnter()
    {
        Agent.navMeshAgent.isStopped = false;
    }

    public override void OnExit()
    {
        Agent.navMeshAgent.isStopped = true;
    }

    public override void OnUpdate()
    {
        Agent.navMeshAgent.SetDestination(Agent.obsession.transform.position);
    }
}
