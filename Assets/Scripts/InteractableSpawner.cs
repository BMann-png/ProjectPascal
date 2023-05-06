using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableSpawner : MonoBehaviour
{
	public bool guaranteeSpawn = false;
	public byte type;
	public byte id;
	public UnityEvent onInteract;
	public UnityEvent onStopInteract;
	public UnityEvent onComplete;
	//type 0 - water gun
	//type 1 - bubble gun
	//type 2 - dart gun
	//type 3 - other gun
	//type 4 - pacifier

	//type 100-254 other

	//type 255 - random gun
}