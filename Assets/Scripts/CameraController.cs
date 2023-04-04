using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
	private static readonly float MAX_ANGLE = 90.0f;

	[SerializeField] private float sensitivity = 100.0f;

	private Transform player;

	private void Start()
	{
		player = transform.parent;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

		player.Rotate(Vector3.up, mouseX);

		Quaternion yQuat = transform.localRotation * Quaternion.Euler(-mouseY, 0.0f, 0.0f);
		if (Quaternion.Angle(Quaternion.identity, yQuat) < MAX_ANGLE) { transform.localRotation = yQuat; }
	}
}
