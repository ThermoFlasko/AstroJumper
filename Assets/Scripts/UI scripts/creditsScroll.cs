using UnityEngine;
using UnityEngine.UI;

public class creditsScroll : MonoBehaviour
{
    public float scrollSpeed = 100f;
    private bool IsActive = false;
    private RectTransform rectTransform;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive)
        {
            rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
        }
    }

    public void CreditsActive()
    {
        IsActive = true;
    }

    public void CreditsInactive()
    {
        IsActive = false;
    }
}
