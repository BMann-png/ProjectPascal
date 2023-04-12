using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles {

public class oxspin : MonoBehaviour {

	public Animator XO_Spin;
	public bool open;
	public Transform Player;

	void Start() {
		open = false;
	}

	void OnMouseOver() {
		{
			if (Player) {
				float dist = Vector3.Distance(Player.position, transform.position);
				if (dist < 15) {
					if (open == false) {
						if (Input.GetMouseButtonDown(0)) {
							StartCoroutine(opening());
						}
					} else {
						if (open == true) {
							if (Input.GetMouseButtonDown(0)) {
								StartCoroutine(closing());
							}
						}

					}

				}
			}

		}

	}

	IEnumerator opening() {
		print("you are spinning");
		XO_Spin.Play("XO_Spin180_01");
		open = true;
		yield return new WaitForSeconds(.5f);
	}

	IEnumerator closing() {
		print("you are spinning again");
		XO_Spin.Play("XO_Spin180_02");
		open = false;
		yield return new WaitForSeconds(.5f);
	}


}

}