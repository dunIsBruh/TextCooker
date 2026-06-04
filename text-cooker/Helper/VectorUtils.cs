namespace text_cooker.Helper;

using System;
using System.Collections.Generic;
using System.Linq;

public static class VectorUtils
{
    public static float CosineSim(float[]? a, float[]? b)
    {
        if (a == null || b == null || a.Length != b.Length) return 0f;
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += (double)a[i] * b[i];
            na += (double)a[i] * a[i];
            nb += (double)b[i] * b[i];
        }
        if (na == 0 || nb == 0) return 0f;
        return (float)(dot / (Math.Sqrt(na) * Math.Sqrt(nb)));
    }

    // build TF vector for tokens according to vocabulary
    public static float[] TfVector(List<string> tokens, List<string> vocab)
    {
        var vec = new float[vocab.Count];
        var freq = tokens.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
        for (int i = 0; i < vocab.Count; i++)
        {
            freq.TryGetValue(vocab[i], out var c);
            vec[i] = c;
        }
        return vec;
    }

    // cosine for float arrays
    public static float CosineFloatArray(float[]? a, float[]? b) => CosineSim(a,b);
}
