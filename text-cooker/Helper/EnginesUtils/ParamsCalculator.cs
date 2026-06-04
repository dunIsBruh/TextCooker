namespace text_cooker.Helper.EnginesUtils;

public class ParamsCalculator
{
    public static double StyleStrength(float[] a, float[] b)
    {
        double sim = Cosine(a, b);
        double styleStrength = 1.0 - sim;
        return Math.Clamp(styleStrength, 0.1, 1.0);
    }
    
    private static double Cosine(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }
}