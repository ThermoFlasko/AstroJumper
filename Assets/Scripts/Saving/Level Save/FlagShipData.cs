using UnityEngine;

[System.Serializable]
public class FlagShipData
{
    public int health = 0;
    public float shield = 0;
    public int[] shieldNodes = new int[3];
    public int[] shieldNodesShield = new int[3];
    // 0 is top node
    // 1 is bottom left
    // 2 is bottom right

    public Vector3 position;


}
