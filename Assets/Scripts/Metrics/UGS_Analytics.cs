using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;
using UnityEngine.UnityConsent;
using UnityEngine.SceneManagement;
public class UGS_Analytics : MonoBehaviour
{

    private void OnEnable()
    {
        Player.onPlayerDeath += PlayerDeathCustomEvent;
    }

    private void OnDisable()
    {
        Player.onPlayerDeath -= PlayerDeathCustomEvent;
    }

    async void Start()
    {
            await UnityServices.InitializeAsync();
            GiveConsent(); //Get user consent according to various legislations
    }

    public void PlayerDeathCustomEvent(Unit unit)
    {
        CustomEvent myEvent = new CustomEvent("playerDeath")
        {
            { "levelName", SceneManager.GetActiveScene().name}
        };

        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();

    }

    public void GiveConsent()
    {
        // Call if consent has been given by the user
        EndUserConsent.SetConsentState(new ConsentState
        {
             AnalyticsIntent = ConsentStatus.Granted,
        });
        Debug.Log($"Consent has been provided. The SDK is now collecting data!");
    }
}