using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[HideInInspector] public byte id;   //ID 0-3 is a player
										//ID 4-38 is an enemy
										//ID of 255 is invalid
	[HideInInspector] public byte type; //Type decides what model/kind of entity this is
										//Type 0 - Player model 1
										//Type 1 - Player model 2
										//Type 2 - Player model 3
										//Type 3 - Player model 4
										//Type 4 - Common enemy
										//Type 5 - 
										//Type 6 - 
										//Type 7 - 
										//Type 8 - 
										//Type 9 - 

	private Vector3 targetPosition;
	private Vector3 targetRotation;

	private void Awake()
	{
		targetPosition = transform.position;
		targetRotation = transform.eulerAngles;
	}

	private void Update()
	{
		if (GameManager.Instance.thisPlayer != id || (id > 3 && !GameManager.Instance.IsServer))
		{
			transform.position = Vector3.Lerp(transform.position, targetPosition, 0.3f);
			transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetRotation, 0.3f);
		}
	}

	public void SetTransform(TransformPacket tp)
	{
		targetPosition = new Vector3(tp.xPos, tp.yPos, tp.zPos);
		targetRotation = new Vector3(transform.rotation.x, tp.yRot, tp.zRot);
	}

	//TODO: Actions, Health, Inventory

	const byte RUN_FLAG = 0b00000001;

	public void DoAction(ActionPacket packet)
	{
		if ((packet.data & RUN_FLAG) == RUN_FLAG)
		{
			//do thing
		}
	}
}
