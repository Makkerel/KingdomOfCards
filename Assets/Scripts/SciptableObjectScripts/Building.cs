using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class BuildingData
{
    [Header("What Each Tier Looks Like")]
    public Mesh building_look;
}

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building")]
public class Building : Card
{
    public GameObject building_prefab;
    public List<BuildingData> buildingTiers;
}
