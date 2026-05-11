using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;
using UnityEngine.UnityConsent;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
public class UGS_Analytics : MonoBehaviour
{

    private float currentSceneTimeDuration = 0f;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // metric event actions
        Player.onPlayerDeath += PlayerDeathCustomEvent;
        Unit.onDeath += GroundEnemyDeathCustomEvent;
        Inventory.OnItemAdded += ItemPickUpCustomEvent;
        SceneTransition.OnSceneChanged += LevelCompleteCustomEvent;

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void OnDisable()
    {
        // metric event actions
        Player.onPlayerDeath -= PlayerDeathCustomEvent;
        Unit.onDeath -= GroundEnemyDeathCustomEvent;
        Inventory.OnItemAdded -= ItemPickUpCustomEvent;
        SceneTransition.OnSceneChanged -= LevelCompleteCustomEvent;
    
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        GiveConsent(); //Get user consent according to various legislations
    }

    private void Update()
    {
        currentSceneTimeDuration += Time.deltaTime;
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

    public void LevelCompleteCustomEvent(string levelName)
    {
        CustomEvent myEvent = new CustomEvent("levelComplete")
        {
            {"levelName", levelName},
            {"levelDuration", currentSceneTimeDuration}
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

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneTimeDuration = 0f;
    }

}