using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Scrap", menuName = "Scriptable Objects/Scrap")]
public class Scrap : Item
{
    public bool isFirstDrop = false;

    public override void OnItemPickUp()
    {
        return;
    }
}
