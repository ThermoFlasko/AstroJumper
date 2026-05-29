using UnityEngine;

[CreateAssetMenu(fileName = "DefaultSpaceSaveSO", menuName = "Scriptable Objects/DefaultSpaceSaveSO")]
public class DefaultSpaceSaveSO : ScriptableObject
{
    public Vector3 playerPosition = new Vector3(0,0,0);
    public string levelName = "";
}
