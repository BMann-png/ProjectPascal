using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
	public IdleState(string name, BaseAI agent) : base(name, agent) { }

	public override void OnEnter()
	{
		Packet packet = new Packet();
		packet.type = 1;
		packet.id = Agent.entity.id;
		packet.action = new ActionPacket(0);

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
