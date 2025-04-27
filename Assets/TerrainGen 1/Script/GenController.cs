using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenController : MonoBehaviour
{
    public PlacePath paths;
    public TerrainFFTGen up;
    public TerrainFFTGen down;
    public TerrainFFTGen left;
    public TerrainFFTGen right;
    public TerrainPerlinGen mainTerrain;
    public BiomeGen biomeGen;
    public GenerateRabbits generateRabbits;
    public UnityEvent onGenerationStart;
    public UnityEvent onGenerationComplete;
    // Start is called before the first frame update
    void Start()
    {
        onGenerationStart?.Invoke();
        mainTerrain.ifRandomSeed = true;
        mainTerrain.StartGenerate();
        up.StartGenerate();
        down.StartGenerate();
        left.StartGenerate();
        right.StartGenerate();

        paths.Generate();

        biomeGen.Generate();

        generateRabbits.Generate();

        onGenerationComplete?.Invoke();
    }
}
