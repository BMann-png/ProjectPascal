    using UnityEngine;

//ID 0-3 - player
//ID 4-100 - common enemy
//ID 101 - Spitter
//ID 102 - Alarmer
//ID 103 - Lurker
//ID 104 - Hurler
//ID 105 - Snatcher
//ID 106 - Corrupted Spitter
//ID 107 - Corrupted Alarmer
//ID 108 - Corrupted Lurker
//ID 109 - Corrupted Hurler
//ID 110 - Corrupted Snatcher
//ID 111-10000 is an interactable
//ID 10001-20000 is a water projectile
//ID 20001-30000 is a bubble projectile
//ID 30001-40000 is a dart projectile
//ID 40001-50000 is a other projectile
//ID of 65535 is invalid

public class Entity : MonoBehaviour
{
    [HideInInspector] public ushort id;

    private Vector3 previousPosition;
    private Vector3 targetPosition;
    private float targetRotation;

	public Transform shoot;
	public Transform weapon;
	[HideInInspector] public GameObject model;
	[HideInInspector] public Animator animator;

    [SerializeField] bool inLobby = false;
    private bool quitting = false;
	public bool destroyed = false;

    private void Awake()
    {
        targetPosition = transform.position;
        targetRotation = transform.eulerAngles.y;
    }

    private void Update()
    {
        if ((id < 4 && GameManager.Instance.ThisPlayer != id) || (id > 3 && (id < 111 || id > 10000) && !GameManager.Instance.IsServer))
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.3f);
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);
        }

        if (!inLobby && animator != null)
        {
            float speed = (transform.position - previousPosition).magnitude / Time.deltaTime;

            animator.SetFloat("Speed", Mathf.Abs(speed));

            if (transform.position != previousPosition) previousPosition = transform.position;
        }
    }

    public void SetTransform(TransformPacket tp)
    {
        targetPosition = new Vector3(tp.xPos, tp.yPos, tp.zPos);
        targetRotation = tp.yRot;

        if (id > 10000)
        {
            transform.eulerAngles = new Vector3(tp.xRot, tp.yRot, 0.0f); //TODO: Lerp smoothly
        }
        else
        {
            transform.eulerAngles = new Vector3(0.0f, tp.yRot, 0.0f); //TODO: Lerp smoothly

            if (shoot != null)
            {
                shoot.eulerAngles = new Vector3(tp.xRot, tp.yRot, 0.0f);
            }
        }
    }

    public void SetModel()
    {
        //TODO: Check for existing model

        if (id < 4)
        {
            model = Instantiate(GameManager.Instance.PrefabManager.PlayerModels[id], transform);
        }
        else if (id < 101)
        {
            model = Instantiate(GameManager.Instance.PrefabManager.RandomCommon(), transform);
        }
        else if (id < 111)
        {
            model = Instantiate(GameManager.Instance.PrefabManager.SpecialModels[id - 100], transform);
        }
        else if (id < 20001) // Water Projectile
        {
            //TODO: Set Model
        }
        else if (id < 20001) // Bubble Projectile
        {
            //TODO: Set Model
        }
        else if (id < 20001) // Dart Projectile
        {
            //TODO: Set Model
        }
        else if (id < 20001) // Other Projectile
        {
            //TODO: Set Model
        }

        model.TryGetComponent(out animator);

        if (id < 4 && GameManager.Instance.InLobby)
        {
            animator.SetBool("InLobby", true);
        }
    }

    public void DoAction(ActionPacket packet)
    {
        if (id < 4)
        {
            switch (packet.data)
            {
                case 0: //Sprint
                    {
                        CapsuleCollider collider = GetComponent<CapsuleCollider>();
                        collider.height = 2.2f;
                        collider.radius = 0.3f;
                        collider.center = Vector3.up * 1.1f;
                        animator.SetTrigger("Sprint");
                    }
                    break;
                case 1: //Stop Sprint
                    {
                        CapsuleCollider collider = GetComponent<CapsuleCollider>();
                        collider.height = 1.0f;
                        collider.radius = 0.5f;
                        collider.center = Vector3.up * 0.5f;
                        animator.SetTrigger("StopSprint");
                    }
                    break;
                case 2: //Push
                    {
                        CapsuleCollider collider = GetComponent<CapsuleCollider>();
                        collider.height = 2.2f;
                        collider.radius = 0.3f;
                        collider.center = Vector3.up * 1.1f;
                        animator.SetTrigger("Push");
                    }
                    break;
                case 3: //Stop Push
                    {
                        CapsuleCollider collider = GetComponent<CapsuleCollider>();
                        collider.height = 1.0f;
                        collider.radius = 0.5f;
                        collider.center = Vector3.up * 0.5f;
                        animator.SetTrigger("StopPush");
                    }
                    break;
                case 4: //Trip
                    {
                        CapsuleCollider collider = GetComponent<CapsuleCollider>();
                        collider.height = 1.0f;
                        collider.radius = 0.5f;
                        collider.center = Vector3.up * 0.5f;
                        animator.SetTrigger("Trip");
                    }
                    break;
                case 5: //Down
                    {
                        CapsuleCollider collider = GetComponent<CapsuleCollider>();
                        collider.height = 1.0f;
                        collider.radius = 0.5f;
                        collider.center = Vector3.up * 0.5f;
                        GetComponent<Revive>().OnDown();
                        animator.SetTrigger("Down");
                        GetComponent<Inventory>().HideWeapon();
                    }
                    break;
                case 6: //Revive
                    {
                        animator.SetTrigger("Revive");
                        GetComponent<Revive>().OnRevive();
					    GetComponent<Inventory>().ShowWeapon();
				    }
                    break;


                case 10: //StartRevive
                    {
                        GetComponent<PlayerController>().StartRevive();
                    }
                    break;
                case 11: //EndRevive
                    {
                        GetComponent<PlayerController>().EndRevive();
                    }
                    break;

                case 23: //Play Dart Sound
                    {
                        GameManager.Instance.AudioManager.Sfx.PlayOneShot
                            (GameManager.Instance.AudioManager.GetShots(0));
                    }
                    break;
                case 24: //Play Bubble Sound
                    {
                        GameManager.Instance.AudioManager.Sfx.PlayOneShot
                            (GameManager.Instance.AudioManager.GetShots(1));
                    }
                    break;
                case 25: //Play Squirt Sound
                    {
                        GameManager.Instance.AudioManager.Sfx.PlayOneShot
                            (GameManager.Instance.AudioManager.GetShots(2));
                    }
                    break;
            }
        }
        else if (id < 101)
        {
            switch (packet.data)
            {
                case 0: //Attack
                    {
                        animator.SetBool("isAttacking", true);
                    }
                    break;
                case 1: //Attack
                    {
                        animator.SetBool("isAttacking", false);
                    }
                    break;
                case 2: //Death
                    {
                        animator.SetTrigger("isDead");
                    }
                    break;
            }
        }
        else if (id < 111)
        {
            switch (packet.data)
            {
                case 0: //Attack
                    {
                        animator.SetBool("isAttacking", true);
                    }
                    break;
                case 1: //Attack
                    {
                        animator.SetBool("isAttacking", false);
                    }
                    break;
                case 2: //Death
                    {
                        animator.SetTrigger("isDead");
                    }
                    break;
            }
        }
        else if (id < 10001)
        {
            if (TryGetComponent(out Pushable p))
            {
                if (packet.data < 100) { p.OtherPush(packet.data); }
                else if (packet.data < 255) { p.OtherStop((byte)(packet.data - 100)); }
                else { p.OtherComplete(); }
            }
            else if (TryGetComponent(out Animate a))
            {
                a.OtherPlay();
            }
        }
    }

    private void OnApplicationQuit()
    {
        quitting = true;
    }

    private void OnDestroy()
    {
        if (!quitting && !destroyed)
        {
            GameManager.Instance.Destroy(this);
        }
    }
}
