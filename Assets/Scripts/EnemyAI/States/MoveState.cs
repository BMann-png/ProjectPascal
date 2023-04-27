using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public MoveState(string name, BaseAI agent) : base(name, agent) { }

    public override void OnEnter()
    {
        Agent.navMeshAgent.isStopped = false;

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = Agent.entity.id;
		packet.action = new ActionPacket(1);

		NetworkManager.Instance.SendMessage(packet);
	}

    public override void OnExit()
    {
        Agent.navMeshAgent.isStopped = true;

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = Agent.entity.id;
		packet.action = new ActionPacket(0);

		NetworkManager.Instance.SendMessage(packet);
	}

    public override void OnUpdate()
    {
        Agent.navMeshAgent.SetDestination(Agent.obsession.transform.position);
        Agent.entity.animator.SetFloat("speed", Agent.navMeshAgent.speed);
	}
}
