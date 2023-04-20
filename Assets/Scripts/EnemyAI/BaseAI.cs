using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAI : MonoBehaviour
{
    [SerializeField] Perception playerPerception;
    [SerializeField] Perception allyPerception;

    BAIStateMachine stateMachine = new BAIStateMachine();

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
