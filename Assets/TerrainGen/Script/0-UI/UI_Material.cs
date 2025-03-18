using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Material : MonoBehaviour
{
    [Header("Terrain")]
    public Terrain terrain;
    public Material[] materials;
    public TMP_Dropdown materialDropdown;

    [Header("Water")]
    public GameObject waterPlane;
    public Material[] waterMaterials;
    public TMP_Dropdown waterDropdown;
    public TMP_InputField waterHeightInput;

    [Header("Sky")]
    public Camera targetCamera;
    public Color[] sky;
    public TMP_Dropdown skyDropdown;

    

    private void Start()
    {
        materialDropdown.onValueChanged.AddListener(OnTerrainMaterialSelected);
        waterDropdown.onValueChanged.AddListener(OnWaterMaterialSelected);
        skyDropdown.onValueChanged.AddListener(OnColorSelected);

        //waterheight
        waterHeightInput.onEndEdit.AddListener(OnWaterHeightInputChanged);
        
        InitializeMaterialDropdown(materialDropdown, materials);
        InitializeMaterialDropdown(waterDropdown, waterMaterials);
        InitializeSkyDropdown();

    }

    void OnEnable()
    {
        float initialHeight = waterPlane.transform.position.y;
        waterHeightInput.text = initialHeight.ToString("F2");
        
        //InitializeMaterialDropdown(materialDropdown, materials);
        //InitializeMaterialDropdown(waterDropdown, waterMaterials);
        //InitializeSkyDropdown();
    }

    public void InitializeMaterials()
    {
        ApplyMaterial(materials[materialDropdown.value]);
        ApplyWaterMaterial(waterMaterials[waterDropdown.value]);
        targetCamera.backgroundColor = sky[skyDropdown.value];
    }

    private void InitializeMaterialDropdown(TMP_Dropdown dropdown, Material[] materials)
    {
        dropdown.ClearOptions();
        foreach (var material in materials)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(material.name));
        }
        dropdown.RefreshShownValue();
    }

    public void OnTerrainMaterialSelected(int index)
    {
        if (index >= 0 && index < materials.Length)
        {
            ApplyMaterial(materials[index]);
        }
    }

    public void OnWaterMaterialSelected(int index)
    {
        if (index >= 0 && index < materials.Length)
        {
            ApplyWaterMaterial(waterMaterials[index]);
        }
    }

    private void ApplyMaterial(Material selectedMaterial)
    {
        terrain.materialTemplate = selectedMaterial;
    }

    private void ApplyWaterMaterial(Material selectedMaterial)
    {
        MeshRenderer renderer = waterPlane.GetComponent<MeshRenderer>();
        renderer.material = selectedMaterial;
    }

    private void InitializeSkyDropdown()
    {
        skyDropdown.ClearOptions();

        foreach (var color in sky)
        {
            string colorName = ColorUtility.ToHtmlStringRGB(color);
            skyDropdown.options.Add(new TMP_Dropdown.OptionData($"#{colorName}"));
        }

        skyDropdown.RefreshShownValue();
    }

    private void OnColorSelected(int index)
    {
        if (index >= 0 && index < sky.Length)
        {
            targetCamera.backgroundColor = sky[index];
        }
    }

    //waterheight
    public void OnWaterHeightInputChanged(string value)
    {
        if (float.TryParse(value, out float newHeight))
        {
            if (waterPlane != null)
            {
                Vector3 newPosition = waterPlane.transform.position;
                newPosition.y = newHeight;
                waterPlane.transform.position = newPosition;
            }
        }
    }

}
