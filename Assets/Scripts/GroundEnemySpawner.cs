using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroundEnemySpawner : MonoBehaviour
{
    //private InputAction spawnEnemy;
    public GameObject MeleeEnemyPrefab;
    public GameObject RangedEnemyPrefab;

    public List<Transform> MeleeSpawnLocations;
    public List<Transform> RangedSpawnLocations;

    private void Awake()
    {
        MeleeSpawnLocations = new List<Transform>();
        RangedSpawnLocations = new List<Transform>();
        //spawnEnemy = InputSystem.actions.FindAction("SpawnEnemy");
    }

    private IEnumerator Start()
    {
        yield return null; // Wait one frame to ensure all spawners are initialized

        //Find spawn locations for both melee and ranged enemies
        FindSpawnLocations("MeleeSpawner", MeleeSpawnLocations);
        FindSpawnLocations("RangedSpawner", RangedSpawnLocations);

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

    private void SpawnEnemy(List<Transform> spawnLocations, string enemyTypeTagName, GameObject enemyPrefab)
    {


        foreach (Transform spawn in spawnLocations)
        {
            if (spawn == null)
            {
                Debug.LogWarning($"Found null spawn location in {enemyTypeTagName} list.");
                continue;
            }

            GameObject newEnemy = Instantiate(enemyPrefab, spawn.transform);

            if (newEnemy.TryGetComponent(out EnemyAI enemyAI))
            {
                enemyAI.homePoint = spawn.transform;
            }
        }
    }


    //Call this beofre spawning enemies to make sure we get locaitons not included in the list yet
    private void FindSpawnLocations(string enemyTypeTagName, List<Transform> spawnLocations)
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag(enemyTypeTagName);

        foreach (GameObject spawner in spawners)
        {
            //If the spawner is not already in the list, add it
            if (!spawnLocations.Contains(spawner.transform))
            {
                Transform spawnerTransform = spawner.transform;

                if (spawnLocations.Contains(spawnerTransform))
                {
                    continue;
                }

                spawnLocations.Add(spawner.transform);
            }
        }
    }
}
