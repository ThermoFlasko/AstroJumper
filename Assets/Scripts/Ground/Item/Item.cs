using System;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public abstract class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
}
