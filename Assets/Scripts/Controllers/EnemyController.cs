using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Entity))]
public class EnemyController : MonoBehaviour, INetworked
{
	private Entity entity;

	private Vector3 prevPos;
    Vector3 NetPrevPos { get{ return prevPos; } set { prevPos = value; } }

    private void Awake()
	{
		entity = GetComponent<Entity>();
	}

	private void FixedUpdate()
	{
		if (!GameManager.Instance.Loading)
		{
			Packet packet = new Packet();
			packet.type = 0;
			packet.id = entity.id;
			packet.transform = new TransformPacket(transform, 0.0f);
			NetworkManager.Instance.SendMessage(packet);
		}
	}

    public void NetworkUpdate()
    {
        if (NetPrevPos != transform.position && Mathf.Abs((transform.position - NetPrevPos).magnitude) > .5)
        {
            Packet packet = new Packet();
            packet.type = 0;
            packet.id = entity.id;
            packet.transform = new TransformPacket(transform, Camera.main.transform.eulerAngles.x + 90.0f);
            NetworkManager.Instance.SendMessage(packet);
        }
    }
}
