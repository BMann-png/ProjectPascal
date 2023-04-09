using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[HideInInspector] public byte id;   //ID of 255 is invalid
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

	public void SetTransform(TransformPacket tp)
	{
		transform.position = new Vector3(tp.xPos, tp.yPos, tp.zPos);
		transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, tp.yRot, tp.zRot));
	}

	//TODO: Actions, Health, Inventory
}
