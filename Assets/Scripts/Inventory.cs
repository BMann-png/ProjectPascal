using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<GameObject> primaryWeapons;
    [SerializeField] List<GameObject> secondaryWeapons;

    private bool hasPacifier = false;

    private int primaryIndex = 0;
    private int secondaryIndex = 0;
}
