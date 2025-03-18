using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public bool isMenuOn = false;
    public bool isArtOn = false;

    public Button menuButton;
    public GameObject mainMenu;
    public GameObject artMenu;

    public TMP_Dropdown algorithm;
    public GameObject Midpoint;
    public GameObject improvedMidpoint;
    public GameObject unityPerlin;
    public GameObject perlinPerlin;
    public GameObject FFT;

    private int algorithmChosen;
    public Variable variables = new Variable();

    public MidPointTerrain midTerrains;
    public MidPointImprove mid2Terrains;
    public TerrainPerlin perlinTerrains;
    public TerrainFFT FFTTerrains;

    // Start is called before the first frame update
    void Start()
    {
        mainMenu.SetActive(false);
        artMenu.SetActive(false);
        algorithm.onValueChanged.AddListener(AlgorithmChosen);
    }

    public void MenuButtonClick()
    {
        isMenuOn = !isMenuOn;
        mainMenu.SetActive(isMenuOn);

        if (isMenuOn && isArtOn)
        {
            isArtOn = false;
            artMenu.SetActive(false);
        }
    }

    public void ArtButtonClick()
    {
        isArtOn = !isArtOn;
        artMenu.SetActive(isArtOn);

        if (isMenuOn && isArtOn)
        {
            isMenuOn = false;
            mainMenu.SetActive(false);
        }
    }

    public void AlgorithmChosen(int value)
    {
        Midpoint.SetActive(value == 0 || value == 1);

        unityPerlin.SetActive(value == 2 || value == 3);
        variables.isUnityPerlin = value == 2;


        FFT.SetActive(value == 4);

        algorithmChosen = value;
    }


    public void Generate()
    {
        midTerrains.gameObject.SetActive(algorithmChosen == 0);
        mid2Terrains.gameObject.SetActive(algorithmChosen == 1);
        perlinTerrains.gameObject.SetActive(algorithmChosen == 2 || algorithmChosen == 3);
        FFTTerrains.gameObject.SetActive(algorithmChosen == 4);

        switch (algorithmChosen)
        {
            case 0:
                midTerrains.gameObject.GetComponent<Terrain>().terrainData.heightmapResolution = variables.midResolution;
                midTerrains.gameObject.GetComponent<Terrain>().terrainData.size = new Vector3(variables.width, variables.midheight, variables.length);
                
                midTerrains.roughness = variables.roughness;
                midTerrains.minHeight = variables.minHeight;
                midTerrains.maxHeight = variables.maxHeight;
                midTerrains.enableNormalization = variables.enableNormalization;

                midTerrains.GenerateNewTerrain();
                midTerrains.gameObject.transform.position = new Vector3(-variables.width/2, 0, -variables.length/2);

                artMenu.GetComponent<UI_Material>().terrain = midTerrains.gameObject.GetComponent<Terrain>();
                artMenu.GetComponent<UI_Material>().InitializeMaterials();
                
                break;
            case 1:
                mid2Terrains.gameObject.GetComponent<Terrain>().terrainData.heightmapResolution = variables.midResolution;
                mid2Terrains.gameObject.GetComponent<Terrain>().terrainData.size = new Vector3(variables.width, variables.midheight, variables.length);
                
                mid2Terrains.roughness = variables.roughness;
                mid2Terrains.minHeight = variables.minHeight;
                mid2Terrains.maxHeight = variables.maxHeight;
                mid2Terrains.enableNormalization = variables.enableNormalization;

                mid2Terrains.GenerateNewTerrain();
                mid2Terrains.gameObject.transform.position = new Vector3(-variables.width/2, 0, -variables.length/2);

                artMenu.GetComponent<UI_Material>().terrain = mid2Terrains.gameObject.GetComponent<Terrain>();
                artMenu.GetComponent<UI_Material>().InitializeMaterials();

                break;

            case int v when v == 2 || v == 3:
                perlinTerrains.terrainWidth = perlinTerrains.terrainHeight = variables.resolution;
                perlinTerrains.maxHeight = variables.height;
                perlinTerrains.ifUnityPerlin = variables.isUnityPerlin;
                perlinTerrains.octaves = variables.octave;
                perlinTerrains.persistance = variables.persistance;
                perlinTerrains.lacunarity = variables.lacunarity;
                perlinTerrains.scale = variables.scale;
                perlinTerrains.seed = variables.seed;
                perlinTerrains.offset = variables.offset;

                perlinTerrains.StartGenerate();
                perlinTerrains.gameObject.transform.position = new Vector3(-variables.resolution/2, 0, -variables.resolution/2);

                artMenu.GetComponent<UI_Material>().terrain = perlinTerrains.gameObject.GetComponent<Terrain>();
                artMenu.GetComponent<UI_Material>().InitializeMaterials();

                break;

            case 4:
                FFTTerrains.terrainWidth = FFTTerrains.terrainHeight = variables.resolution;
                FFTTerrains.maxHeight = variables.height;
                FFTTerrains.gaussianMean = variables.gaussianMean;
                FFTTerrains.gaussianDev = variables.gaussianDev;

                FFTTerrains.StartGenerate();
                FFTTerrains.gameObject.transform.position = new Vector3(-variables.resolution/2, 0, -variables.resolution/2);

                artMenu.GetComponent<UI_Material>().terrain = FFTTerrains.gameObject.GetComponent<Terrain>();
                artMenu.GetComponent<UI_Material>().InitializeMaterials();

                break;

        }
    }
}
