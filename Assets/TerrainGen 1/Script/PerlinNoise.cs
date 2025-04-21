using UnityEngine;

public class PerlinNoiseMap
{
    private int octaves = 4;
    private float persistance = 0.5f;
    private float lacunarity = 2f;
    private int scale = 50;
    private int seed = 1;
    private Vector2 offset = new Vector2(100, 200);
    private Vector2[] octaveOffsets;
    private float maxNoiseHeight = float.MinValue;
    private float minNoiseHeight = float.MaxValue;

    public PerlinNoiseMap(int octaves, float persistance, float lacunarity, int scale, int seed, Vector2 offset)
    {
        this.octaves = octaves;
        this.persistance = persistance;
        this.lacunarity = lacunarity;
        this.scale = scale;
        this.seed = seed;
        this.offset = offset;

        System.Random prng = new System.Random(seed);
        octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000,100000) + offset.x;
            float offsetY = prng.Next(-100000,100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
    }

    public float CreatePerlin(int x, int y)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (float)x / scale * frequency + octaveOffsets[i].x;
            float sampleY = (float)y / scale * frequency + octaveOffsets[i].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        } 

        if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
        else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

        return noiseHeight;
    }

    public float Normalize(float noise)
    {
        return Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noise);
    }
}
