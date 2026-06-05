using UnityEngine;
using MilkShake;

public class MeleeCameraShake : MonoBehaviour
{
    public Shaker CameraShaker;
    public ShakePreset MeleeShake;


    public void MeleeShakeCamera() {
        CameraShaker.Shake(MeleeShake);
    }
}
