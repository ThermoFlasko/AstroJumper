using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroundEnemySpawner : MonoBehaviour
{
    //private InputAction spawnEnemy;
    public GameObject MeleeEnemyPrefab;
    public GameObject RangedEnemyPrefab;

    public List<GameObject> MeleeSpawnLocations;
    public List<GameObject> RangedSpawnLocations;

    private void Awake()
    {
        MeleeSpawnLocations = new List<GameObject>();
        RangedSpawnLocations = new List<GameObject>();
        //spawnEnemy = InputSystem.actions.FindAction("SpawnEnemy");
    }
    private void Start()
    {
        SpawnEnemy(MeleeSpawnLocations, "MeleeSpawner", MeleeEnemyPrefab);
        SpawnEnemy(RangedSpawnLocations, "RangedSpawner", RangedEnemyPrefab);
    }

    private void Update()
    {
        //if(spawnEnemy.WasReleasedThisFrame())
        //{
        //    SpawnEnemy(MeleeSpawnLocations, "MeleeSpawner", MeleeEnemyPrefab);
        //    SpawnEnemy(RangedSpawnLocations, "RangedSpawner", RangedEnemyPrefab);
        //}
    }

    private void SpawnEnemy(List<GameObject> spawnLocations, string enemyTypeTagName, GameObject enemyPrefab)
    {
        GameObject.FindGameObjectsWithTag(enemyTypeTagName, spawnLocations);

        foreach (GameObject spawn in spawnLocations)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, spawn.transform);

            if (newEnemy.TryGetComponent(out EnemyAI enemyAI))
            {
                enemyAI.homePoint = spawn.transform;
            }
        }
    }
}
