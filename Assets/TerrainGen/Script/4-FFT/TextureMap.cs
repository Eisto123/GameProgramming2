using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureMap : MonoBehaviour
{
    public Terrain terrain; // 地形对象
    public float lowHeight = 0.1f; // 低材质的高度阈值
    public float midHeightLow = 0.2f; // 低材质的高度阈值
    public float midHeightHigh = 0.6f; // 低材质的高度阈值
    public float highHeight = 0.8f; // 高材质的高度阈值

    // Start is called before the first frame update
    void Start()
    {
        ApplyMaterialByHeight();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ApplyMaterialByHeight()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        float maxHeight = terrainData.size.y;
        lowHeight *= maxHeight;
        midHeightLow *= maxHeight;
        midHeightHigh *= maxHeight;
        highHeight *= maxHeight;


        Debug.Log(width+","+height);

        float[,] heights = terrainData.GetHeights(0, 0, width, height);

        // 3
        int numOfLayers = terrainData.alphamapLayers;

        // 创建 Alpha Maps（每个材质层的混合权重）
        float[,,] alphaMaps = new float[width, height, numOfLayers];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float terrainHeight = heights[x, y] * terrainData.size.y; // 计算实际地形高度

                // 初始化材质权重
                float weight1 = 0f;
                float weight2 = 0f;
                float weight3 = 0f;

                // 根据高度分配材质权重
                if (terrainHeight < lowHeight)
                {
                    weight1 = 1f; // 材质1（低高度）完全覆盖
                }
                else if (terrainHeight >= lowHeight && terrainHeight < midHeightLow)
                {
                    // 材质1和材质2之间线性插值
                    float t = (terrainHeight - lowHeight) / (midHeightLow - lowHeight);
                    weight1 = 1 - t;
                    weight2 = t;
                }
                else if (terrainHeight >= midHeightLow && terrainHeight < midHeightHigh)
                {
                    float t = (terrainHeight - midHeightLow) / (midHeightHigh - midHeightLow);
                    weight2 = 1;
                }
                else if (terrainHeight >= midHeightHigh && terrainHeight < highHeight)
                {
                    // 材质2和材质3之间线性插值
                    float t = (terrainHeight - midHeightHigh) / (highHeight - midHeightHigh);
                    weight2 = 1 - t;
                    weight3 = t;
                }
                else
                {
                    weight3 = 1f; // 材质3（高高度）完全覆盖
                }

                // 应用到 Alpha Map
                alphaMaps[x, y, 0] = weight1; // 材质1
                alphaMaps[x, y, 1] = weight2; // 材质2
                alphaMaps[x, y, 2] = weight3; // 材质3
            }
        }

        // 设置混合权重
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }
}
