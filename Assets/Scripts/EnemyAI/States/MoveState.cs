using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public MoveState(string name, BaseAI agent) : base(name, agent) { }

    public override void OnEnter()
    {
        Debug.Log("Move");
        Agent.navMeshAgent.isStopped = false;
    }

    public override void OnExit()
    {
        Agent.navMeshAgent.isStopped = true;
    }

    public override void OnUpdate()
    {
        Agent.navMeshAgent.SetDestination(Agent.obsession.transform.position);
        Agent.animator.SetFloat("speed", Agent.navMeshAgent.speed);
    }
}
