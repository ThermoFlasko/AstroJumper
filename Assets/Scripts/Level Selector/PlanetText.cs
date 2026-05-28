using UnityEngine;

public class PlanetText : MonoBehaviour
{
    [SerializeField] public GameObject planetGO;

    private void Start()
    {
        UpdateTransform();
    }

    void UpdateTransform()
    {
        Vector3 textPos = Camera.main.WorldToScreenPoint(planetGO.transform.position + new Vector3(0f, -1.5f, 0));
        transform.position = textPos;
    }
}
