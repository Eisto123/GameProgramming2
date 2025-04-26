using System;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

public class TerrainFFTGen : MonoBehaviour
{
    public Terrain terrain;

    public int maxHeight = 600;
    public int terrainWidth = 512;  
    public int terrainHeight = 512;
    public int heightmapResolution = 512;
    public float[,] heightMap;

    [Header("Gaussian")]
    public float gaussianMean = 0f;
    public float gaussianDev = 1f;

    [Header("FFT")]
    private Complex[,] complexMap;
    private Dictionary<int, Complex[]> twiddleFactor;
    [Tooltip("0: no filter; >0: low-pass; <0: high-pass")]
    public float filterR = 0; 

    
    private float maxNoiseHeight = float.MinValue;
    private float minNoiseHeight = float.MaxValue;

    void Start()
    {

    }

    public void StartGenerate()
    {
        //OneDFFTTest();

        // Start Timer
        Stopwatch stopwatch = Stopwatch.StartNew();

        CreateNewTerrain();
        FastFourierTrans(heightmapResolution, heightmapResolution);
        FFTFilter(heightmapResolution, heightmapResolution);
        IFFT(heightmapResolution, heightmapResolution);

        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int y = 0; y < heightmapResolution; y++)
            {
                heightMap[x, y] = (float)complexMap[x, y].Magnitude;

                if (heightMap[x, y] > maxNoiseHeight) maxNoiseHeight = heightMap[x, y];
                else if (heightMap[x, y] < minNoiseHeight) minNoiseHeight = heightMap[x, y];
            }
        }

        // Normalization
        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int y = 0; y < heightmapResolution; y++)
            {
                heightMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, heightMap[x, y]);

                if (x == 0 || y == 0) heightMap[x, y] = 0;
            }
        }

        terrain.terrainData.SetHeights(0, 0, heightMap);
        terrain.terrainData.size = new UnityEngine.Vector3(terrainWidth, maxHeight, terrainHeight); 
        
        // Stop the timer
        stopwatch.Stop(); 
        UnityEngine.Debug.Log("FFT Execution Time: "+stopwatch.ElapsedMilliseconds+" ms");
    }

    /*private void OneDFFTTest()
    {
        int N = 8; // 信号长度
        double frequency = 1; // 正弦波的频率

        List<Complex> sineWaveSignal = new List<Complex>();
        for (int n = 0; n < N; n++)
        {
            double value = Math.Sin(2 * Math.PI * frequency * n / N);
            sineWaveSignal.Add(new Complex(value, 0));
        }
        for (int i = 0; i < sineWaveSignal.Count; i++)
        {
            Debug.Log(sineWaveSignal[i]);
        }

        Debug.Log(" ");
        InitializeTwiddleFactors(N);
        Complex[] result = OneDFFT(sineWaveSignal.ToArray());
        for (int i = 0; i < result.Length; i++)
        {
            Debug.Log(result[i]);
        }

        Complex[] result2 = OnedIFFT(result);
        for (int i = 0; i < result2.Length; i++)
        {
            Debug.Log(result2[i]);
        }
    }*/

    private void CreateNewTerrain()
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new UnityEngine.Vector3(terrainWidth, maxHeight, terrainHeight); 
        terrain.terrainData.heightmapResolution = heightmapResolution;

        heightMap = new float[heightmapResolution, heightmapResolution];

        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int y = 0; y < heightmapResolution; y++)
            {
                heightMap[x, y] = CreateGaussian(gaussianMean, gaussianDev);
            }
        }
        

        terrainData.SetHeights(0, 0, heightMap);
        terrainData.size = new UnityEngine.Vector3(terrainWidth, maxHeight, terrainHeight); 
        

        //terrainData.size = new UnityEngine.Vector3(width, maxHeight, height); 

    }

    public float CreateGaussian(float mean, float deviation)
    {
        float u1 = UnityEngine.Random.value;
        float u2 = UnityEngine.Random.value;

        // Box-Muller
        float z0 = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
        float randomY = mean + z0 * deviation;

        //return Mathf.Clamp(randomY, -1f, 1f);
        return randomY;
    }

    private void FastFourierTrans(int width, int height)
    {
        // Change from real number to complex number
        complexMap = new Complex[width,height];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                complexMap[row, col] = new Complex(heightMap[row,col], 0);
            }
        }

        // Apply FFT for each row
        InitializeTwiddleFactors(width);
        Complex[] x_n = new Complex[width];
        for (int row = 0; row < height; row++)
        {
            // Get the entire row xn[width]
            for (int col = 0; col < width; col++) x_n[col] = complexMap[row, col];
            // Apply 1D FFT to the row
            x_n = OneDFFT(x_n);
            for (int col = 0; col < width; col++) complexMap[row,col] = x_n[col];
        }

        // Apply FFT for each col
        InitializeTwiddleFactors(height);
        Complex[] y_n = new Complex[height];
        for (int col = 0; col < width; col++)
        {
            // Get the entire col xn[height]
            for (int row = 0; row < height; row++) y_n[row] = complexMap[row, col];

            // Apply 1D FFT to the col
            y_n = OneDFFT(y_n);
            for (int row = 0; row < height; row++) complexMap[row,col] = y_n[row];
        }
    }

    private void InitializeTwiddleFactors(int maxN)
    {
        twiddleFactor = new Dictionary<int, Complex[]>();

        // Complex.Exp(new Complex(0, -2 * Math.PI * i / N))
        // N: W_N[0] - W_N[n/2-1]
        // W_N[k] = W_N[k + N/2] when k oven
        // W_N[k] = -W_N[k + N/2] when k odd
        for (int n = 2; n <= maxN; n *= 2)
        {
            Complex[] factors = new Complex[n / 2];
            for (int k = 0; k < n / 2; k++)
            {
                factors[k] = Complex.Exp(new Complex(0, -2 * Math.PI * k / n));
            }
            twiddleFactor[n] = factors;
        }
    }

    private Complex[] OneDFFT(Complex[] x_n)
    {
        int N = x_n.Length;
        
        // The smallest unit
        if (N <= 1) return x_n;

        // Even
        Complex[] evenPart = new Complex[N/2];
        for (int i = 0; i < N / 2; i++)
        {
            evenPart[i] = x_n[2 * i];
        }
        evenPart = OneDFFT(evenPart); 

        // Odd
        Complex[] oddPart = new Complex[N/2];
        for (int i = 0; i < N / 2; i++)
        {
            oddPart[i] = x_n[2 * i + 1];
        }
        oddPart = OneDFFT(oddPart);

        // Merge
        Complex[] mergeList = new Complex[N];
        for (int i = 0; i < N/2; i++)
        {
            //Complex T = oddPart[i] * Complex.Exp(new Complex(0, -2 * Math.PI * i / N));
            Complex T = oddPart[i] * twiddleFactor[N][i];
            mergeList[i] = evenPart[i] + T;
            mergeList[i+N/2] = evenPart[i] - T;
        }

        return mergeList;

    }

    private void FFTFilter(int width, int height)
    {
        // Frequency Centralization
        Complex[,] shiftedComplexMap = new Complex[width, height];

        //(u', v') = ( (u+N/2)modN, (v+M/2)modM )
        for (int row = 0; row < height; row++)
        {
            int u = (row + height/2) % height;
            for (int col = 0; col < width; col++)
            {
                int v = (col + width/2) % width;
                //float f_u = (u - height / 2) / (float)height;
                //float f_v = (v - width / 2) / (float)width;
                float f_u = u - height / 2;
                float f_v = v - width / 2;
                float frequency = Mathf.Sqrt(f_u*f_u + f_v*f_v);
                //float frequency = Mathf.Sqrt((float)row/height * row/height + (float)col/width * col/width);
                float filter = (frequency > 10e-6) ? 1 / Mathf.Pow(frequency, filterR) : 1;

                shiftedComplexMap[u,v] = complexMap[row,col] * filter;
            }
        }

        // shift back
        for (int row = 0; row < height; row++)
        {
            int u = (row + height / 2) % height;
            for (int col = 0; col < width; col++)
            {
                int v = (col + width / 2) % width;
                complexMap[row, col] = shiftedComplexMap[u, v];
            }
        }
    }

    private void IFFT(int width, int height)
    {
        // Apply IFFT for each row
        Complex[] x_n = new Complex[width];
        for (int row = 0; row < height; row++)
        {
            // Get the entire row xn[width]
            for (int col = 0; col < width; col++) x_n[col] = complexMap[row, col];
            // Apply 1D FFT to the row
            x_n = OnedIFFT(x_n);
            for (int col = 0; col < width; col++) complexMap[row,col] = x_n[col];
        }

        // Apply FFT for each col
        Complex[] y_n = new Complex[height];
        for (int col = 0; col < width; col++)
        {
            // Get the entire col xn[height]
            for (int row = 0; row < height; row++) y_n[row] = complexMap[row, col];

            // Apply 1D FFT to the col
            y_n = OnedIFFT(y_n);
            for (int row = 0; row < height; row++) complexMap[row,col] = y_n[row];
        }
    }

    private Complex[] OnedIFFT(Complex[] x_n)
    {
        // conj (x[n])
        for (int i = 0; i < x_n.Length; i++)
            x_n[i] = Complex.Conjugate(x_n[i]);

        // FFT( conj(x[n]) )
        x_n = OneDFFT(x_n);

        // 1/N * conj ( FFT( conj(x[n]) ) )
        for (int i = 0; i < x_n.Length; i++)
            x_n[i] = Complex.Conjugate(x_n[i]) / x_n.Length;

        return x_n;
    }
}

// [CustomEditor(typeof(TerrainFFTGen))]
// public class TerrainFFTGenEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         TerrainFFTGen terrainFFT = (TerrainFFTGen)target;

//         if (GUILayout.Button("Start Generate"))
//         {
//             terrainFFT.StartGenerate();
//         }
//     }
// }
