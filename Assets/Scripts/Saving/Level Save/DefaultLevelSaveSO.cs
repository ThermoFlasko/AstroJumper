using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DefualtLevelSaveSO", menuName = "Scriptable Objects/DefualtLevelSaveSO")]
public class DefaultLevelSaveSO : ScriptableObject
{
    [Header("Upgrade Defualts   ")] public DefaultSpaceSaveSO defaultSpaceSaveSO = new DefaultSpaceSaveSO();

    [Header("Ground Trooper Defaults")] public DefaultPlanetSaveSO defaultPlanetSaveSO = new DefaultPlanetSaveSO();

    public List<string> completedEvents = new List<string>();
}
