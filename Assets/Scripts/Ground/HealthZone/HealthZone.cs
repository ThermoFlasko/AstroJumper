using UnityEngine;

public class HealthZone : MonoBehaviour
{
    public bool isInZone = false;
    public float timeBeforeNextTick = 1f;
    public float currentPeriod = 0f;    // amount of time since a heal tick has occur or after the player entered the zone
    public Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isInZone)
        {
            currentPeriod += Time.deltaTime;
            if (currentPeriod >= timeBeforeNextTick)
            {
                currentPeriod = 0f;
                player.Health += 15;
            }
        }

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            print("not hit the player");
            return;
        }

        print($"healing {collision.name}");
        currentPeriod = 0f;
        isInZone = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }    
        print("Stopping healing");
        isInZone = false;
    }
}
