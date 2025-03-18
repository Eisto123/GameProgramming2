using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

[System.Serializable]
public class Variable
{
    public int resolution = 64;

    // Midpoint
    public int midResolution;
    public int width;
    public int length;
    public int midheight;
    public float minHeight = 10;
    public float maxHeight = 20;
    public float roughness = 1;
    public bool enableNormalization = false;

    // Perlin
    public int height = 10;
    public bool isUnityPerlin = true;
    public int octave = 3;
    public float persistance = 0.4f;
    public float lacunarity = 2;
    public int scale = 100;
    public int seed = 30;
    public Vector2 offset = new Vector2(0,0);

    // FFT
    public float gaussianMean = 0;
    public float gaussianDev = 1;
    public float filterR = 2;
}
