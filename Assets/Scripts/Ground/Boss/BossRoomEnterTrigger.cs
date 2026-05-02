using UnityEngine;

public class BossRoomEnterTrigger : MonoBehaviour
{
    public GameObject Boss;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("entered room");    
        Boss.SetActive(true);
    }
}
