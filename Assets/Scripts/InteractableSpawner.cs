using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableSpawner : MonoBehaviour
{
	public bool guaranteeSpawn = false;
	public byte type;
	//type 0 - water gun
	//type 1 - bubble gun
	//type 2 - dart gun
	//type 3 - other gun
	//type 4 - pacifier

	//type 100 - chair pushable
	//TODO: other pushables

	//type 255 - random gun
}