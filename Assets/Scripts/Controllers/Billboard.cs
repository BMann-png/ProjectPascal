using UnityEngine;

public class Billboard : MonoBehaviour
{
	private void Update()
	{
		transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
	}
}
