using UnityEngine;

public class PlanetUIBtn : MonoBehaviour
{

    public GameObject planetUIGO;

    public void OnButtonClick()
    {
        Debug.Log("Planet UI Button Clicked");
        planetUIGO = GameObject.FindGameObjectsWithTag("PlanetUI")[0];
        Destroy(planetUIGO);
    }
}
