using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Mid : MonoBehaviour
{
    public Terrain terrain;
    private TerrainData terrainData;
    public MidPointTerrain midpointTerrain;

    [Header("Terrain Basic Setting")]
    public TMP_InputField widthInput;
    public TMP_InputField lengthInput;
    public TMP_InputField heightInput;

    [Header("MidPoint Settings")]
    public float minHeight = 0.2f;
    public float maxHeight = 0.8f;
    public Slider minHeightSlider;
    public Slider maxHeightSlider;
    public TMP_Text minHeightText;
    public TMP_Text maxHeightText;
    public Toggle normalizationToggle;
    public Slider roughnessSlider;
    public TMP_Text roughnessText;

    private void Start()
    {
        terrainData = terrain.terrainData;

        //basic terrain settings
        widthInput.text = terrainData.size.x.ToString("F0");
        lengthInput.text = terrainData.size.z.ToString("F0");
        heightInput.text = terrainData.size.y.ToString("F0");

        widthInput.onEndEdit.AddListener(UpdateWidth);
        lengthInput.onEndEdit.AddListener(UpdateLength);
        heightInput.onEndEdit.AddListener(UpdateHeight);

        //mid point settings
        normalizationToggle.isOn = midpointTerrain.enableNormalization;
        normalizationToggle.onValueChanged.AddListener(OnNormalizationToggleChanged);
        roughnessSlider.value = midpointTerrain.roughness;
        roughnessSlider.onValueChanged.AddListener(OnRoughnessChanged);
        roughnessText.text = $"{midpointTerrain.roughness:F2}";

        //initial midpointimprove
        midpointTerrain.minHeight = minHeight;
        midpointTerrain.maxHeight = maxHeight;
        maxHeightSlider.value = maxHeight;
        minHeightSlider.value = minHeight;
        maxHeightSlider.onValueChanged.AddListener(OnMaxHeightChanged);
        minHeightSlider.onValueChanged.AddListener(OnMinHeightChanged);

    }

    /// <summary>
    /// terrain basic setting
    /// </summary>
    /// <param name="value"></param>

    public void UpdateWidth(string value)
    {
        if (float.TryParse(value, out float width))
        {
            terrainData.size = new Vector3(width, terrainData.size.y, terrainData.size.z);
        }
    }

    public void UpdateLength(string value)
    {
        if (float.TryParse(value, out float length))
        {
            terrainData.size = new Vector3(terrainData.size.x, terrainData.size.y, length);
        }
    }

    public void UpdateHeight(string value)
    {
        if (float.TryParse(value, out float height))
        {
            terrainData.size = new Vector3(terrainData.size.x, height, terrainData.size.z);
        }
    }

    ///<summary>
    /// Mid point settings
    /// </summary>
    /// 
    public void OnNormalizationToggleChanged(bool isActive)
    {
        midpointTerrain.enableNormalization = isActive;
        midpointTerrain.GenerateNewTerrain();
    }


    public void OnMinHeightChanged(float value)
    {
        minHeight = value;
        minHeightText.text = $"{minHeight:F2}";
        midpointTerrain.minHeight = minHeight;
        //midpointImprove.GenerateNewTerrain();
    }
    public void OnMaxHeightChanged(float value)
    {
        maxHeight = value;
        maxHeightText.text = $"{maxHeight:F2}";
        midpointTerrain.maxHeight = maxHeight;
        //midpointImprove.GenerateNewTerrain();
    }

    public void OnRoughnessChanged(float value)
    {
        midpointTerrain.roughness = value;
        roughnessText.text = $"{value:F2}";
    }
}
