using UnityEngine;

public class PlanetUI : MonoBehaviour
{
    public string sceneToLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadScene()
    {
        print("Loading scene: " + sceneToLoad);
        SceneLoader.Instance.LoadNextScene(sceneToLoad);
    }
}
