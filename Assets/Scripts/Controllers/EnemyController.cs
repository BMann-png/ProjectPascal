using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Entity))]
public class EnemyController : MonoBehaviour, INetworked
{
    private Entity entity;

    private Vector3 prevPos;
    Vector3 NetPrevPos { get { return prevPos; } set { prevPos = value; } }

    private void Awake()
    {
        NetworkManager.Instance.tickUpdate += Tick;

        entity = GetComponent<Entity>();
    }

    public void Tick()
    {
        if (!GameManager.Instance.Loading && NetPrevPos != transform.position && Mathf.Abs((transform.position - NetPrevPos).magnitude) > .5)
        {
            Packet packet = new Packet();
            packet.type = 0;
            packet.id = entity.id;
            packet.transform = new TransformPacket(transform, Camera.main.transform.eulerAngles.x + 90.0f);
            NetworkManager.Instance.SendMessage(packet);
        }
    }
}
