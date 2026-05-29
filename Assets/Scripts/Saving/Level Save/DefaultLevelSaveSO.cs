using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DefualtLevelSaveSO", menuName = "Scriptable Objects/DefualtLevelSaveSO")]
public class DefaultLevelSaveSO : ScriptableObject
{
    public DefaultSpaceSaveSO defaultSpaceSaveSO;

    public DefaultPlanetSaveSO defaultPlanetSaveSO;

    public List<string> completedEvents = new List<string>();
    
    public string currLevel = "";
}
