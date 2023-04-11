using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoNavigation : MonoBehaviour
{
    [SerializeField] NavigationNode[] nodes;
    [SerializeField] Material[] Colors;
    private enum AIState
    {
        PATROL,
        IDLE,
        ACTION
    }
    private AIState activeState;
    private Vector3 targetMovement;
    private int index = 0;

    private void Start()
    {
        targetMovement = nodes[0].transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.Instance.IsServer)
        {
            switch (activeState)
            {
                case AIState.PATROL:
                    Patrol();
                    if (Random.Range(0, 500) == 0)
                    {
                        activeState = AIState.IDLE;
                    }
                    break;
                case AIState.IDLE:
                    Idle();
                    break;
                case AIState.ACTION:
                    Action();
                    break;
                default:
                    break;
            }

            Packet packet = new Packet();
            packet.type = 0;
            packet.id = 29;
            packet.transform = new TransformPacket(transform);
            NetworkManager.Instance.SendMessage(packet);
        }
    }

    private void Patrol()
    {
        transform.position += (targetMovement - transform.position).normalized * Time.deltaTime;

        if ((targetMovement - transform.position).magnitude < .5)
        {
            index += 1;
            targetMovement = nodes[index % nodes.Length].transform.position;
        }
    }

    private void Idle()
    {
        if (Random.Range(0, 250) == 0)
        {
            activeState = AIState.ACTION;
        }
    }

    private void Action()
    {
        bool changedColor = false;
        Material newColor;
        while (!changedColor)
        {
            var index = Random.Range(0, Colors.Length);

            newColor = Colors[index];
            if (newColor.color != GetComponent<MeshRenderer>().material.color)
            {
                GetComponent<MeshRenderer>().material = newColor;
                changedColor = true;

                Packet packet = new Packet();

                NetworkManager.Instance.SendMessage(packet);
            }
        }

        activeState = AIState.PATROL;
    }

    public void ChangeColor(int index)
    {
        GetComponent<MeshRenderer>().material = Colors[index];
    }
}
