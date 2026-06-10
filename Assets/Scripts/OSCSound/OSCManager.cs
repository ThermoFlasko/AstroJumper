using UnityEngine;

public class OSCManager : MonoBehaviour
{

    public bool isActive = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true; //allows unity to update when not in focus

        //************* Instantiate the OSC Handler...
        OSCHandler.Instance.Init();
        OSCHandler.Instance.SendMessageToClient("pd", "/unity/trigger", "ready");
        OSCHandler.Instance.SendMessageToClient("pd", "/unity/playseq", 1);
        //*************

        // Activate DSP
        DSPActivate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DSPActivate()
    {
        if (isActive)
        {
            print("disable dsp");
            OSCHandler.Instance.SendMessageToClient("pd", "/unity/activatedsp", 0);
        }
        else
        {
            print("enable dsp");
            OSCHandler.Instance.SendMessageToClient("pd", "/unity/activatedsp", 1);
        }
        
        isActive = !isActive;
    }

    void OnApplicationQuit()
    {
        DSPActivate();
    }
}
