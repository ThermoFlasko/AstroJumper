using TMPro;
using UnityEngine;

public class VersionNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI verionNumText;

    private void Awake()
    {
        verionNumText.text = $"v{Application.version}";
    }
}
