using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : State
{
	float persist = 5.0f;

	public DeathState(string name, BaseAI agent) : base(name, agent) { }

	public override void OnEnter()
	{
		Agent.entity.animator.SetTrigger("isDead");
		GameObject.Destroy(Agent.gameObject, persist);

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = Agent.entity.id;
		packet.action = new ActionPacket(3);

		NetworkManager.Instance.SendMessage(packet);
	}

	public override void OnExit()
	{
		//throw new System.NotImplementedException();
	}

	public override void OnUpdate()
	{
		//throw new System.NotImplementedException();
	}
}
