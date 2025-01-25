using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// Used to transfer a variable to another color space.
    /// </summary>
    public static class ColorSpaceUtility
    {
        public static float gamma = 2.2f;

        public static float GammaToLinear(float gammaValue)
        {
            return Mathf.Pow(gammaValue, gamma);
        }

        public static float GammaToLinearUnity(float gammaValue)
        {
            return Mathf.Pow(gammaValue + 0.055f, 2.4f) / 1.13711896582f;
        }

        //public static Color GammaToLinear(Color gammaColor) // Color.linear

        public static float LinearToGamma(float linearValue)
        {
            return Mathf.Pow(linearValue, 1.0f / gamma);
        }

        public static float LinearToGammaUnity(float linearValue)
        {
            return Mathf.Max(1.055f * Mathf.Pow(linearValue, 0.416666667f) - 0.055f, 0.0f);
        }

        //public static Color LinearToGamma(Color linearColor) // Color.gamma

        // From LumenLight.
        public static Color KelvinToRGB(float tmpKelvin)
        {
            Vector3 a, b, c;

            if (tmpKelvin <= 6500.0)
            {
                a = new Vector3(0, -2902.1955373783176f, -8257.7997278925690f);
                b = new Vector3(0, 1669.5803561666639f, 2575.2827530017594f);
                c = new Vector3(1, 1.3302673723350029f, 1.8993753891711275f);
            }
            else
            {
                a = new Vector3(1745.0425298314172f, 1216.6168361476490f, -8257.7997278925690f);
                b = new Vector3(-2666.3474220535695f, -2173.1012343082230f, 2575.2827530017594f);
                c = new Vector3(0.55995389139931482f, 0.70381203140554553f, 1.8993753891711275f);
            }

            Vector3 tmp;
            tmp.x = a.x / (b.x + tmpKelvin) + c.x;
            tmp.y = a.y / (b.y + tmpKelvin) + c.y;
            tmp.z = a.z / (b.z + tmpKelvin) + c.z;
            return new Color(Mathf.Clamp(tmp[0], 0, 1), Mathf.Clamp(tmp[1], 0, 1), Mathf.Clamp(tmp[2], 0, 1));
        }
    }
}
