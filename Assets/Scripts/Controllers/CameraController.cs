using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
	private static readonly float MAX_ANGLE = 90.0f;

	private Transform character;
	private HUDManager hudManager;

	private void Start()
	{
		hudManager = FindFirstObjectByType<HUDManager>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		character = transform.parent;
	}

	private void Update()
	{
		if (!GameManager.Instance.Loading && !hudManager.Paused)
		{
			float sensitivity = PlayerPrefs.GetFloat("Sensitivity");

			float mouseX = Input.GetAxis("Mouse X") * sensitivity;
			float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

			character.Rotate(Vector3.up, mouseX);

			Quaternion yQuat = transform.localRotation * Quaternion.Euler(-mouseY, 0.0f, 0.0f);
			if (Quaternion.Angle(Quaternion.identity, yQuat) < MAX_ANGLE) { transform.localRotation = yQuat; }
		}
	}
}
