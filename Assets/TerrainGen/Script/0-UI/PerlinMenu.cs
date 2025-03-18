using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PerlinMenu : MonoBehaviour
{
    public MainMenu menu;

    public Slider resolution;
    public TextMeshProUGUI resolutionText;
    public Slider height;
    public TextMeshProUGUI heightText;

    public Slider octave;
    public TextMeshProUGUI octaveText;
    public Slider persistence;
    public TextMeshProUGUI persistenceText;
    public Slider lacunarity;
    public TextMeshProUGUI lacunarityText;
    public Slider scale;
    public TextMeshProUGUI scaleText;

    public TMP_InputField seed;
    public TMP_InputField offsetX;
    public TMP_InputField offsetY;



    // Start is called before the first frame update
    void Start()
    {
        resolution.onValueChanged.AddListener(ResolutionSetting);
        height.onValueChanged.AddListener(HeightSetting);


        octave.onValueChanged.AddListener(OctaveSetting);
        persistence.onValueChanged.AddListener(PersistSetting);
        lacunarity.onValueChanged.AddListener(LacSetting);
        scale.onValueChanged.AddListener(ScaleSetting);
        seed.onEndEdit.AddListener(SeedSetting);
        offsetX.onEndEdit.AddListener(OffsetXSetting);
        offsetY.onEndEdit.AddListener(OffsetYSetting);
    }

    void OnEnable()
    {
        menu.variables.resolution = (int)math.pow(2,resolution.value+6);
        menu.variables.height = (int)height.value;
        menu.variables.octave = (int)octave.value;
        menu.variables.persistance = persistence.value;
        menu.variables.lacunarity = lacunarity.value;
        menu.variables.scale = (int)scale.value;
        menu.variables.seed = int.Parse(seed.text);
        menu.variables.offset.x = int.Parse(offsetX.text);
        menu.variables.offset.y = int.Parse(offsetY.text);
    }

    
    public void ResolutionSetting(float value)
    {
        resolutionText.text = math.pow(2,(value+6)).ToString();
        menu.variables.resolution = (int)math.pow(2,(value+6));
    }

    public void HeightSetting(float value)
    {
        heightText.text = value.ToString();
        menu.variables.height = (int)value;
    }
    

    public void OctaveSetting(float value)
    {
        octaveText.text = value.ToString();
        menu.variables.octave = (int)value;
    }

    public void PersistSetting(float value)
    {
        persistenceText.text = value.ToString("F2");
        menu.variables.persistance = (float)Math.Round(value, 2);
    }

    public void LacSetting(float value)
    {
        lacunarityText.text = value.ToString("F2");
        menu.variables.lacunarity = (float)Math.Round(value, 2);
    }

    public void ScaleSetting(float value)
    {
        scaleText.text = value.ToString();
        menu.variables.scale = (int)value;
    }

    public void SeedSetting(string seedtext)
    {
        menu.variables.seed = int.Parse(seedtext);
    }

    public void OffsetXSetting(string text)
    {
        menu.variables.offset.x = int.Parse(text);
    }

    public void OffsetYSetting(string text)
    {
        menu.variables.offset.y = int.Parse(text);
    }




}
