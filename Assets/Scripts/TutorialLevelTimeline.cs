using UnityEngine;
using UnityEngine.Playables;

public class TutorialLevelTimeline : MonoBehaviour
{
    public PlayableDirector playableDirector;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SaveManager.instance.CurrentLevelSaveData.completedEvents.Contains("TutorialStartTime"))
        {
            playableDirector.enabled = false;
        }
        else
        {
            SaveManager.instance.CurrentLevelSaveData.UpdateCompletedEvents("TutorialStartTime");
            playableDirector.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
