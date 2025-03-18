using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class FFTMenu : MonoBehaviour
{
    public MainMenu menu;

    public Slider resolution;
    public TextMeshProUGUI resolutionText;
    public Slider height;
    public TextMeshProUGUI heightText;

    public Slider mean;
    public TextMeshProUGUI meanText;
    public Slider deviation;
    public TextMeshProUGUI deviationText;
    public Slider r;
    public TextMeshProUGUI rText;


    // Start is called before the first frame update
    void Start()
    {
        resolution.onValueChanged.AddListener(ResolutionSetting);
        height.onValueChanged.AddListener(HeightSetting);
        mean.onValueChanged.AddListener(MeanSetting);
        deviation.onValueChanged.AddListener(DevSetting);
        r.onValueChanged.AddListener(RSetting);
    }

    void OnEnable()
    {
        menu.variables.resolution = (int)math.pow(2,resolution.value+6);
        menu.variables.height = (int)height.value;
        menu.variables.gaussianMean = (float)Math.Round(mean.value, 1);
        menu.variables.gaussianDev = (float)Math.Round(deviation.value, 1);
        menu.variables.filterR = (float)Math.Round(r.value, 1);
    }

    public void ResolutionSetting(float value)
    {
        resolutionText.text = math.pow(2,value+6).ToString();
        menu.variables.resolution = (int)math.pow(2,(value+6));
    }

    public void HeightSetting(float value)
    {
        heightText.text = value.ToString();
        menu.variables.height = (int)value;
    }

    public void MeanSetting(float value)
    {
        meanText.text = value.ToString("F1");
        menu.variables.gaussianMean = (float)Math.Round(value, 1);
    }

    public void DevSetting(float value)
    {
        deviationText.text = value.ToString("F1");
        menu.variables.gaussianDev = (float)Math.Round(value, 1);
    }

    public void RSetting(float value)
    {
        rText.text = value.ToString("F1");
        menu.variables.filterR = (float)Math.Round(value, 1);
    }


}
