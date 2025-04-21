using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BiomeType
{
                      // Temp, Humid
    SnowyTundra,      // Low,  Low
    TemperateForest,  // L-M,  M-H
    Wetlands,         // L-M,  High
    DryPlateau,       // M-H,  Low
    Tropical,         // High, M-H
}

[System.Serializable]
[CreateAssetMenu(menuName = "BiomeGen/BiomeConfig")]
public class BiomeConfig : ScriptableObject
{
    public BiomeType biomeType;
    public List<GameObject> treePrefabs;
    public GameObject grassPrefab;
    public List<GameObject> plantPrefabs;
    public List<GameObject> rockPrefabs;
    public Color treeColor;
    public Material grassMat;
    [Range(0f, 1f)] public float treeDensity = 0.5f;
    
    [Range(0f, 1f)] public float grassDensity = 0.7f;
    [Range(0f, 1f)] public float plantDensity = 0.7f;
}
