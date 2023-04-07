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
        bool changedColour = false;
        Material newColour;
        while (!changedColour){
            newColour = Colors[Random.Range(0, Colors.Length)];
            if (newColour.color != gameObject.GetComponent<MeshRenderer>().material.color)
            {
                gameObject.GetComponent<MeshRenderer>().material.color = newColour.color;
                changedColour = true;
            }
        }

        activeState = AIState.PATROL;
    }
}
