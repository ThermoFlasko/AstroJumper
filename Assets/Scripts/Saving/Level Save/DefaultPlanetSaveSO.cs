using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultPlanetSaveSO", menuName = "Scriptable Objects/DefaultPlanetSaveSO")]
public class DefaultPlanetSaveSO : ScriptableObject
{
    public string levelName = "";
        public Vector3 playerPosition = new Vector3(0,0,0);
        public List<GameObject> meleeEnemies = new();
        public List<GameObject> rangedEnemies = new();
}
