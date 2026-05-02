using Unity.VisualScripting;
using UnityEngine;

public class StateMachine
{
    IState currentState;
    public void Start()
    {
        currentState.Enter();
    }

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    public void Update()
    {
        if (currentState != null)
        {
            currentState.Execute();
        }
    }
}
