using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSpawner : Singleton<MainMenuSpawner>
{
    private PrefabManager prefabManager;
    public PrefabManager PrefabManager { get => prefabManager; }

	public List<GameObject> playerLocations = new List<GameObject>();
	private static readonly int MAX_ENEMY_COUNT = 15;
	private int enemyCount = 0;

	private GameObject[] spawners;

	protected override void Awake()
	{
		base.Awake();

		prefabManager = FindFirstObjectByType<PrefabManager>();

		spawners = GameObject.FindGameObjectsWithTag("EnemySpawner");
	}

    private void Update()
    {
		if (enemyCount < MAX_ENEMY_COUNT)
        {
			GameObject spawner = spawners[Random.Range(0, spawners.Length)];
			float x = Random.Range(-spawner.transform.localScale.x * 0.5f, spawner.transform.localScale.x * 0.5f);
			float z = Random.Range(-spawner.transform.localScale.z * 0.5f, spawner.transform.localScale.z * 0.5f);
			Transform t = gameObject.AddComponent<Transform>();
			t.position = new Vector3(x, spawner.transform.position.y, z);
			Instantiate(prefabManager.Enemy, t);
			enemyCount++;
        }

	}


}
