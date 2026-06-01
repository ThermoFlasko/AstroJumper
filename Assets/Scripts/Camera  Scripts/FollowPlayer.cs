using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FollowPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform player;
    private Vector3 offset;
    public float xValAdjust;
    public float yValAdjust;
    public Vector3 posValAdjust;


    void Start()
    {
        offset = transform.position - player.position;
        posValAdjust = new Vector3(xValAdjust, yValAdjust);
        
    }

    // Update is called once per frame
    void Update()
    {
      posValAdjust.Set(xValAdjust, yValAdjust, 0f);
        transform.position = player.position + offset + posValAdjust;
    }
}
