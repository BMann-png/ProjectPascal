using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSpawner : Singleton<MainMenuSpawner>
{
    private PrefabManager prefabManager;
    public PrefabManager PrefabManager { get => prefabManager; }

	public List<GameObject> playerLocations = new List<GameObject>();
	private static readonly int MAX_ENEMY_COUNT = 15;

	protected override void Awake()
	{
		base.Awake();

		prefabManager = FindFirstObjectByType<PrefabManager>();
	}


}
