using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [SerializeField] private GameObject[] playerModels;
    public GameObject[] PlayerModels { get => playerModels; }

    [SerializeField] private GameObject[] enemyModels;
    public GameObject[] EnemyModels { get => enemyModels; }

    [SerializeField] private GameObject[] specialModels;
    public GameObject[] SpecialModels { get => specialModels; }

	[SerializeField] private GameObject[] pickups;
	public GameObject[] Pickups { get => pickups; }

	[SerializeField] private GameObject[] pushables;
	public GameObject[] Pushables { get => pushables; }

	[SerializeField] private GameObject player;
	public GameObject Player { get => player; }

    [SerializeField] private GameObject networkPlayer;
    public GameObject NetworkPlayer { get => networkPlayer; }

    [SerializeField] private GameObject lobbyPlayer;
    public GameObject LobbyPlayer { get => lobbyPlayer; }

    [SerializeField] private GameObject enemy;
    public GameObject Enemy { get => enemy; }

    [SerializeField] private GameObject networkEnemy;
    public GameObject NetworkEnemy { get => networkEnemy; }

    [SerializeField] private GameObject[] projectiles;
    public GameObject[] Projectiles { get => projectiles; }

    [SerializeField] private GameObject[] networkProjectiles;
    public GameObject[] NetworkProjectiles { get => networkProjectiles; }

	[SerializeField] private GameObject healthBar;
	public GameObject HealthBar { get => healthBar; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public GameObject RandomCommon()
    {
        return enemyModels[Random.Range(0, enemyModels.Length)];
    }
}
