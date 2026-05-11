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
        Unit.onDeath += GroundEnemyDeathCustomEvent;
        Inventory.OnItemAdded += ItemPickUpCustomEvent;
    }

    private void OnDisable()
    {
        Player.onPlayerDeath -= PlayerDeathCustomEvent;
        Unit.onDeath -= GroundEnemyDeathCustomEvent;
        Inventory.OnItemAdded -= ItemPickUpCustomEvent;
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        GiveConsent(); //Get user consent according to various legislations
    }

    #region Metric Event Functions

    public void PlayerDeathCustomEvent(Unit unit)
    {
        CustomEvent myEvent = new CustomEvent("playerDeath")
        {
            { "levelName", SceneManager.GetActiveScene().name}
        };

        AnalyticsService.Instance.RecordEvent(myEvent);

    }
    
    public void GroundEnemyDeathCustomEvent(Unit unit)
    {
        if (unit is Player)
        {
            print("player killed");
            return;
        }

        CustomEvent myEvent = new CustomEvent("groundEnemyDeath")
        {
            {"enemyName", unit.name}
        };

        AnalyticsService.Instance.RecordEvent(myEvent);
    }

    public void ItemPickUpCustomEvent(Item item)
    {
        if (item is not Scrap)
        {
            print("item is not scrap");
            return;
        }

        CustomEvent myEvent = new CustomEvent("scrapGained")
        {
            
        };

        AnalyticsService.Instance.RecordEvent(myEvent);
    }

    #endregion

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