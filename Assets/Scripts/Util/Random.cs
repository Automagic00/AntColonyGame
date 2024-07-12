

using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class RandomUtil
{

    public static int weightedRandom(List<float> weights) => weightedRandom(weights.ToArray());
    public static int weightedRandom(float[] weights)
    {
        float totalWeight = 0;
        foreach (float weight in weights) totalWeight += weight;
        if (totalWeight <= 0) return UnityEngine.Random.Range(0, weights.Count());

        float remainingWeight = UnityEngine.Random.Range(0, totalWeight);

        for (int i = 0; i < weights.Length; i++)
        {
            remainingWeight -= weights[i];
            if (remainingWeight <= 0) return i;
        }
        return weights.Count() - 1;
    }
}