// Original: http://stackoverflow.com/questions/1303368/how-to-generate-normally-distributed-random-from-an-integer-range/1303512#1303512
// Edited by Alan Zucconi ( http://www.alanzucconi.com/?p=1713 )
using UnityEngine;
public class RandomGaussian
{
    private static bool uselast = true;
    private static float next_gaussian = 0.0f;

    // N(0,1)
    public static float NextGaussian()
    {
        if (uselast) 
        { 
            uselast = false;
            return next_gaussian;
        }
        else
        {
            float v1, v2, s;
            do
            {
                v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
                v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1.0f || s == 0f);

            s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

            next_gaussian = v2 * s;
            uselast = true;
            return v1 * s;
        }
    }

    // N(mean, standard_deviation)
    public static float NextGaussian(float mean, float standard_deviation)
    {
        return mean + NextGaussian() * standard_deviation;
    }

    // Will approximitely give a random gaussian integer between min and max so that min and max are at
    // 3.5 deviations from the mean (half-way of min and max).
    public static float Range (float min, float max)
    {
        float deviations = 3.5f;
        float r;

        float mean = min + (max - min) / 2.0f;
        float std = (max - min) / 2.0f / deviations;

        do
        {
            r = NextGaussian(mean, std);
        } while (r > max || r < min);

        return r;
    }
}