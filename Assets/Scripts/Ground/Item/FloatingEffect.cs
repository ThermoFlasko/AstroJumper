using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float speed = 2;
    public float height = 0.25f;

    public Vector3 startPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        startPosition.y = startPosition.y +0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * height;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

    }
}
