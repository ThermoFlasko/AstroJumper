using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundMoveScript : MonoBehaviour
{
    [SerializeField] private RawImage backGround;
    //how much to move it
    public float moveValue = .1f;
    public float time = .5f;
    public bool moveAlongYAxis = false;
    private float platoe = 1.5f;
    private Rect moveBackground;

    private void Start()
    {
        if (moveAlongYAxis)
        {
            InvokeRepeating("MoveBackgroundDiagonal", 0, time);
            return;
        }

        InvokeRepeating("MoveBackground", 0, time);
    }


    // call every .5 seconds or whatever time is set to

    void MoveBackground()
    {
        moveBackground = new Rect(backGround.uvRect.x + moveValue, backGround.uvRect.y, backGround.uvRect.width, backGround.uvRect.height);
        backGround.uvRect = moveBackground;
    }

    void MoveBackgroundDiagonal()
    {
        moveBackground = new Rect(backGround.uvRect.x + moveValue, backGround.uvRect.y + moveValue, backGround.uvRect.width, backGround.uvRect.height);
        backGround.uvRect = moveBackground;
    }
}
