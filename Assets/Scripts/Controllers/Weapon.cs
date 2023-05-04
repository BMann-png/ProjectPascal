using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform shoot;
    [SerializeField] private byte type = 0;
    [SerializeField] private float delay = 0.1f;
    [SerializeField] private int numsOfShots = 1;

    public void Shoot() 
    {
        StartCoroutine(Fire());
    }

    private IEnumerator Fire() 
    {
        Vector2 variation = Vector2.zero;
        for (int i = 0; i < numsOfShots; i++)
        {
            GameManager.Instance.Shoot(shoot, type, variation);
            variation.x = Random.Range(-15, 16);
            variation.y = Random.Range(-15, 16);
            yield return new WaitForSeconds(delay);
        }
    }
}