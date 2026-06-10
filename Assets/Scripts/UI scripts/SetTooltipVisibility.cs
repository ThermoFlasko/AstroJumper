using UnityEngine;

public class SetTooltipVisibility : MonoBehaviour
{
    [SerializeField] private GameObject[] tooltips;
    private void Start()
    {
        tooltips = GameObject.FindGameObjectsWithTag("Tooltip");
    }
    // Update is called once per frame
    void Update()
    {
        if(PlayerPrefs.HasKey("Tooltips Active"))
        {
            // 0 is false, 1 is true
            if(PlayerPrefs.GetInt("Tooltips Active") == 0)
            {
                foreach (GameObject go in tooltips)
                {
                    go.SetActive(false);
                }
            }
            else
            {
                foreach (GameObject go in tooltips)
                {
                    go.SetActive(true);
                }
            }
        }
    }
}
