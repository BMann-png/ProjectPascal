using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
	public AttackState(string name, BaseAI agent) : base(name, agent) { }

	public override void OnEnter()
	{
		Agent.entity.animator.SetBool("isAttacking", true);

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = Agent.entity.id;
		packet.action = new ActionPacket(0);

		NetworkManager.Instance.SendMessage(packet);
	}

	public override void OnExit()
	{
		Agent.entity.animator.SetBool("isAttacking", false);

        Packet packet = new Packet();
        packet.type = 1;
        packet.id = Agent.entity.id;
        packet.action = new ActionPacket(1);

        NetworkManager.Instance.SendMessage(packet);
    }

	public override void OnUpdate()
	{

	}
}
