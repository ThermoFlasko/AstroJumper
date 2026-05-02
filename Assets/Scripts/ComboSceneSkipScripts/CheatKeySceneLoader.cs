using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatKeySceneLoader : MonoBehaviour
{



    void Update()
    {
        // Example: Press Control + Shift + B to load "Boss Scene by Alfredo"
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.B))
        {
            SceneManager.LoadScene("Boss Scene");
        }
        // Example: Press Control + Shift + M to load "Boss Scene by Alfredo"

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.M))
        {
            SceneManager.LoadScene("Menus");
        }

        // load tutorial ground
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("Tutorial Ground");
        }

        // load tutorial space
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("Space Level 1");
        }

        // load planet level 1
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene("Planet 1");
        }

        // load planet level 1
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadScene("PCG_Sample");
        }

        // load level selector
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene("Level Selector 2");
        }
    }

}
