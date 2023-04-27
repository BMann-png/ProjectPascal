using UnityEngine;

public class Saferoom : MonoBehaviour
{
	[SerializeField] byte nextLevel;

	private int playerCount = 0;
	private int enemyCount = 0;

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player") { ++playerCount; }
		else if(other.tag == "Enemy") { ++enemyCount; }

		if(enemyCount == 0 && playerCount == GameManager.Instance.PlayerCount)
		{
			GameManager.Instance.ChangeLevel(nextLevel);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player") { --playerCount; }
		else if (other.tag == "Enemy") { --enemyCount; }

		if (enemyCount == 0 && playerCount == GameManager.Instance.PlayerCount)
		{
			GameManager.Instance.ChangeLevel(nextLevel);
		}
	}
}
