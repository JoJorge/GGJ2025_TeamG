using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Revise from Stupid Spherical Harmonics (SH) Tricks's and LightProbeUtility(Keijiro)'s code.
/// </summary>
public static class LightProbeUtility
{
    // Reuse these variable for better performance.
    public static Vector4[] SHA = new Vector4[3];
    public static Vector4[] SHB = new Vector4[3];
    public static Vector4 SHC = new Vector4();
    public static Color[] color = new Color[3];

    // Reuse these variable for better performance.
    public static SphericalHarmonicsL2 sh;
    public static Vector4 worldNormal4 = new Vector4();
    public static Vector4 vB = new Vector4();
    public static float vC;

    /// <summary>
    /// Get probe lighting's color. (seems to be in linear space in this project.)
    /// </summary>
    /// <param name="worldPosition">The position of point to compute.</param>
    /// <param name="worldNormal">The normal of point to compute.</param>
    /// <returns>The probe lighting color on the given position.</returns>
    public static Color ShadeSH9(Vector3 worldPosition, Vector3 worldNormal)
    {
        LightProbes.GetInterpolatedProbe(worldPosition, null, out sh);
        worldNormal4.Set(worldNormal.x, worldNormal.y, worldNormal.z, 1.0f);
        vB.Set(worldNormal.x * worldNormal.y, worldNormal.y * worldNormal.z, 
                 worldNormal.z * worldNormal.z, worldNormal.z * worldNormal.x);
        vC = worldNormal.x * worldNormal.x - worldNormal.y * worldNormal.y;

        ComputeSHCoefficients(sh);

        // Constant + Linear polynomials
        color[0].r = Vector4.Dot(SHA[0], worldNormal4);
        color[0].g = Vector4.Dot(SHA[1], worldNormal4);
        color[0].b = Vector4.Dot(SHA[2], worldNormal4);

        // Quadratic polynomials
        color[1].r = Vector4.Dot(SHB[0], vB);
        color[1].g = Vector4.Dot(SHB[1], vB);
        color[1].b = Vector4.Dot(SHB[2], vB);

        // Final quadratic polynomial
        color[2] = SHC * vC;

        return color[0] + color[1] + color[2];
    }

    // Set SH coefficients to MaterialPropertyBlock
    public static void SetSHCoefficients(Vector3 position, MaterialPropertyBlock properties)
    {
        SphericalHarmonicsL2 sh;
        LightProbes.GetInterpolatedProbe(position, null, out sh);
        Vector4[] SHA = new Vector4[3];
        Vector4[] SHB = new Vector4[3];
        Vector4 SHC = new Vector4();

        ComputeSHCoefficients(sh);

        // Constant + Linear
        for (var i = 0; i < 3; i++)
            properties.SetVector(_idSHA[i], SHA[i]);

        // Quadratic polynomials
        for (var i = 0; i < 3; i++)
            properties.SetVector(_idSHB[i], SHB[i]);

        // Final quadratic polynomial
        properties.SetVector(_idSHC, SHC);
    }

    // Set SH coefficients to Material
    public static void SetSHCoefficients(Vector3 position, Material material)
    {
        SphericalHarmonicsL2 sh;
        LightProbes.GetInterpolatedProbe(position, null, out sh);
        Vector4[] SHA = new Vector4[3];
        Vector4[] SHB = new Vector4[3];
        Vector4 SHC = new Vector4();

        ComputeSHCoefficients(sh);

        // Constant + Linear
        for (var i = 0; i < 3; i++)
            material.SetVector(_idSHA[i], SHA[i]);

        // Quadratic polynomials
        for (var i = 0; i < 3; i++)
            material.SetVector(_idSHB[i], SHB[i]);

        // Final quadratic polynomial
        material.SetVector(_idSHC, SHC);
    }

    public static void ComputeSHCoefficients(SphericalHarmonicsL2 sh)
    {
        for (int i = 0; i < 3; i++)
        {
            SHA[i].Set(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6]);
            SHB[i].Set(sh[i, 4], sh[i, 5], 3.0f * sh[i, 6], sh[i, 7]);
        }
        SHC.Set(sh[0, 8], sh[1, 8], sh[2, 8], 1.0f);
    }

    static int[] _idSHA = {
        Shader.PropertyToID("unity_SHAr"),
        Shader.PropertyToID("unity_SHAg"),
        Shader.PropertyToID("unity_SHAb")
    };

    static int[] _idSHB = {
        Shader.PropertyToID("unity_SHBr"),
        Shader.PropertyToID("unity_SHBg"),
        Shader.PropertyToID("unity_SHBb")
    };

    static int _idSHC =
        Shader.PropertyToID("unity_SHC");
}
