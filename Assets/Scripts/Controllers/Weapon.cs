using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    enum WeaponType 
    { 
        FOAM_DART,
        RUBBER_BAND,
        BUBBLE
    }

    [SerializeField] private WeaponType type;

    public void Shoot() 
    { 
        GameManager.Instance.Shoot(GetWeaponType());
    }

    public byte GetWeaponType() 
    {
        switch (type)
        {
            case WeaponType.FOAM_DART:
                return 0;
            case WeaponType.RUBBER_BAND:
                return 1;
            case WeaponType.BUBBLE:
                return 2;
            default:
                return 255;
        }
    }
}