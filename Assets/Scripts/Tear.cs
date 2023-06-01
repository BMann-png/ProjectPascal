using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tear : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;

    private float timer = 0f;

    private void Start()
    {
        timer = lifeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        { 
            Destroy(gameObject);
        }
    }
}
