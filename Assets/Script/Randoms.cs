using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Randoms
{
    public static bool CoinToss()
    {
        return Random.value < 0.5f;
    }

    public static bool Chance(float chance)
    {
        return Random.value < chance;
    }

    public static float RandomRange(float min, float max)
    {
        return Random.value * (max - min) + min;
    }

    public static int WeightedRandom(IEnumerable<int> weights)
    {
        int totalWeight = 0;
        foreach (var n in weights)
        {
            totalWeight += n;
        }

        float rnd = Random.value * totalWeight;
        foreach (var n in weights)
        {
            rnd -= n;
            if (rnd <= 0) return n;
        }
        return -1;
    }

    public static T RandomWeightedObject<T>(Dictionary<T,int> objectWeightPair)
    {
        int totalWeight = 0;
        foreach (var w in objectWeightPair.Values)
        {
            totalWeight += w;
        }

        float rnd = Random.value * totalWeight;
        foreach (var pair in objectWeightPair)
        {
            rnd -= pair.Value;
            if (rnd <= 0) return pair.Key;
        }
        return default;
    }

    public static T RandomFromArray<T>(this T[] array)
    {
        return array[Mathf.FloorToInt(Random.value * array.Length)];
    }

    public static T RandomFromMatrix<T>(this T[,] matrix)
    {
        int rndX =  Mathf.FloorToInt(Random.value * matrix.GetLength(0));
        int rndY =  Mathf.FloorToInt(Random.value * matrix.GetLength(1));
        return matrix[rndX, rndY];

        int lengthX = matrix.GetLength(0);
        int rnd = Mathf.FloorToInt(Random.value * matrix.Length);
        int x = rnd % lengthX;
        return matrix[x, (rnd - x) / lengthX];
    }

    public static Vector3 NoY(this Vector3 v3)
    {
        v3.y = 0;
        return v3;
    }
}