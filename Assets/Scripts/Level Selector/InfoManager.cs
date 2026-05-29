using System;
using UnityEngine;
// NOTE: csv files can be found in Assets/Level/Prefabs/Level Selector/Planet CSV

public class InfoManager : MonoBehaviour
{
    public TextAsset textAssetData;
    public GameObject[] planets;

    void Start()
    {
        readCSV();
    }

    private void readCSV()
    {
        string[] data = textAssetData.text.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < planets.Length; i++)
        {
            Planet planet = planets[i].GetComponent<Planet>();
            planet.planetName = data[(i + 1)];
            planet.planetDescription = data[(i + 1) + 5 * 1];
            //planet.resources = data[(i + 1) + 5 * 2];
            //planet.dificulty = data[(i + 1) + 5 * 3];
            //planet.faction = data[(i + 1) + 5 * 4];
            planet.sceneToLoad = data[(i + 1) + 5 * 5];
            planet.displayName();
        }
    }
}
