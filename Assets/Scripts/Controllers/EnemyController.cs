using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Entity))]
public class EnemyController : MonoBehaviour, INetworked
{
    private Entity entity;

    Transform NetPrevPos;

    private void Awake()
    {
        NetworkManager.Instance.tickUpdate += Tick;

        entity = GetComponent<Entity>();
    }

    public void Tick()
    {
        if (!GameManager.Instance.Loading && NetPrevPos.position != transform.position && (Mathf.Abs((transform.position - NetPrevPos.position).magnitude) > .1 || RotationDifference()))
        {
            Packet packet = new Packet();
            packet.type = 0;
            packet.id = entity.id;
            packet.transform = new TransformPacket(transform, Camera.main.transform.eulerAngles.x + 90.0f);
            NetworkManager.Instance.SendMessage(packet);
        }
    }

    private bool RotationDifference()
    {
        var difference = (transform.rotation * Quaternion.Inverse(NetPrevPos.rotation)).eulerAngles;

        for (int i = 0; i < 3; i++)
        {
            switch (i)
            {
                case 0:
                    return difference.x > 0;
                case 1:
                    return difference.y > 0;
                case 2:
                    return difference.z > 0;
            }
        }

        return false;
    }
}
