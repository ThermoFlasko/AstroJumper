using UnityEngine;

public class AnnunakiMeleeOSC : MonoBehaviour
{
    public void PlayMeleeScreech()
    {
        OSCHandler.Instance.SendMessageToClient("pd", "/unity/swing", 1);
    }
}
