#define COLOR_SPACE_LINEAR

using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Diagnostics;
using System;

namespace CliffLeeCL
{
    /// <summary>
    /// Is used to compute lighting without shaders. (will return linear-space color if is in linear color space)
    /// </summary>
    public class RenderingSystem : MonoBehaviour
    {
        /// <summary>
        /// The variable is used to access this class.
        /// </summary>
        public static RenderingSystem instance;

        /// <summary>
        /// The dictionary is used for reusing light contribution vector.
        /// </summary>
        public Dictionary<LightData, Color[]> lightContributionVectorByLightData;
        [HideInInspector]
        /// <summary>
        /// Is used for outside to reuse or setup resusing.
        /// </summary>
        public Color[] reusedVector;
        /// <summary>
        /// Is reuse for setup light date for key searching.
        /// </summary>
        LightData tempLightData = new LightData();

        /// <summary>
        /// Compute shader that helps to accelerate the rendering process.
        /// </summary>
        public ComputeShader renderingShader;
        [HideInInspector]
        /// <summary>
        /// Kernel ID for ComputeLambertianSingle.
        /// </summary>
        public int kernelComputeLambertianSingle;
        [HideInInspector]
        /// <summary>
        /// Kernel ID for ComputeLambertianSingleReuseLightContributionVector.
        /// </summary>
        public int kernelComputeLambertianSingleReuseLightContributionVector;
        [HideInInspector]
        /// <summary>
        /// Kernel ID for ComputeLambertianAll.
        /// </summary>
        public int kernelComputeLambertianAll;
        [HideInInspector]
        /// <summary>
        /// Kernel ID for ComputeLambertianWithoudIdSingle.
        /// </summary>
        public int kernelComputeLambertianWithoudIdSingle;
        [HideInInspector]
        /// <summary>
        /// Kernel ID for ComputeProbeLighting.
        /// </summary>
        public int kernelComputeProbeLighting;
        [HideInInspector]
        /// <summary>
        /// Kernel ID for ComputeLightContribution.
        /// </summary>
        public int kernelComputeLightContribution;

        /// <summary>
        /// Compute buffer that stores data of sample points.
        /// </summary>
        ComputeBuffer samplePointBuffer;
        /// <summary>
        /// Compute buffer that stores data of lights.
        /// </summary>
        ComputeBuffer lightBuffer;
        /// <summary>
        /// Compute buffer that stores data of shadow testing.
        /// </summary>
        ComputeBuffer shadowBuffer;
        /// <summary>
        /// Compute buffer that stores data of Spherical Harmonics coefficients for probe lighting.
        /// </summary>
        ComputeBuffer SHCoefficientBuffer;
        /// <summary>
        /// Compute buffer that stores data of light contribution for reusing.
        /// </summary>
        ComputeBuffer lightContributionBuffer;
        /// <summary>
        /// Compute buffer that stores data to compute light contribution.
        /// </summary>
        ComputeBuffer computeLightContributionBuffer;
        /// <summary>
        /// Compute buffer that stores data of illumination.
        /// </summary>
        ComputeBuffer illuminationBuffer;
        /// <summary>
        /// Compute buffer that stores data of float returned by GPU.
        /// </summary>
        ComputeBuffer floatBuffer1;
        /// <summary>
        /// Compute buffer that stores data of float returned by GPU.
        /// </summary>
        ComputeBuffer floatBuffer2;
        /// <summary>
        /// The array is used to store result from GPU.
        /// </summary>
        Color[] illumination;
        /// <summary>
        /// The array is used to store result from GPU.
        /// </summary>
        float[] floatData1;
        /// <summary>
        /// The array is used to store result from GPU.
        /// </summary>
        float[] floatData2;
        /// <summary>
        /// The array is used to store light contribution data.
        /// </summary>
        LightContributionData[] lightContributionData;
        /// <summary>
        /// The array is used to store data to compute light contribution.
        /// </summary>
        ComputeLightContributionData[] computeLightContributionData;

        /// <summary>
        /// Store the index of the sample points which are currently in the shadow of the Light.
        /// </summary>
        private Dictionary<Light, List<int>> underShadowRelation;

        RenderTexture renderTexture;
        public Texture2D targetTexture;

        /// <summary>
        /// Used to transfer data of sample point to compute shader.
        /// </summary>
        struct SamplePointData
        {
            public Color Kd;
            public Vector3 worldPosition;
            public Vector3 worldNormal;
        };

        /// <summary>
        /// Used to transfer data of lights to compute shader and also used as key for reusing.
        /// </summary>
        public struct LightData : IEqualityComparer<LightData>
        {
            public Color Id;
            public Vector3 worldPosition;
            public Vector3 forward;
            public float range;
            public float spotAngle;
            public int type;


            // Copy constructor
            public LightData(LightData instanceToCopy)
            {
                Id = instanceToCopy.Id;
                worldPosition = instanceToCopy.worldPosition;
                forward = instanceToCopy.forward;
                range = instanceToCopy.range;
                spotAngle = instanceToCopy.spotAngle;
                type = instanceToCopy.type;
            }

            public bool Equals(LightData x, LightData y)
            {
                // For reusing the lights, we don't care Id.
                if (x.type == y.type)
                {
                    if (x.type == 0) // Spotlight
                        return (x.worldPosition == y.worldPosition) && (x.forward == y.forward)
                            && (x.spotAngle == y.spotAngle) && (x.range == y.range);
                    else if (x.type == 2) // Point light
                        return (x.worldPosition == y.worldPosition) && (x.range == y.range);
                    else if (x.type == 1) // Directional light
                        return x.forward == y.forward;
                }
                return false;
            }

            public int GetHashCode(LightData obj)
            {
                if (obj.type == 0) // Spotlight
                    return 1000 + obj.worldPosition.GetHashCode() + obj.forward.GetHashCode() + obj.spotAngle.GetHashCode() + obj.range.GetHashCode();
                else if (obj.type == 2) // Point light
                    return obj.worldPosition.GetHashCode() + obj.range.GetHashCode();
                else if (obj.type == 1) // Directional light
                    return 1000000 + obj.forward.GetHashCode();
                else
                    return 0;
            }
        };

        /// <summary>
        /// Used to transfer data of shadow testing to compute shader.
        /// </summary>
        struct ShadowData
        {
            public int isUnderShadowOfLight; // Use int for 4 byte alignment and no dynamic array allowed for passing to compute shader.
        };

        /// <summary>
        /// Used to transfer data of SHCoefficients to compute shader.
        /// </summary>
        struct SHCoefficientData
        {
            public Vector4 SHA1;
            public Vector4 SHA2;
            public Vector4 SHA3;
            public Vector4 SHB1;
            public Vector4 SHB2;
            public Vector4 SHB3;
            public Vector4 SHC;
        };

        /// <summary>
        /// Used to transfer data of light contribution vector to compute shader.
        /// </summary>
        struct LightContributionData
        {
            public Color lightContribution;
        }

        /// <summary>
        /// Used to transfer data to compute light contribution to compute shader.
        /// </summary>
        struct ComputeLightContributionData
        {
            public Color currentIllumination;
            public Color goalIllumination;
            public Vector3 worldPosition;
        }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += ResetOnSceneLoaded;

            kernelComputeLambertianSingle = renderingShader.FindKernel("ComputeLambertianSingle");
            kernelComputeLambertianSingleReuseLightContributionVector = renderingShader.FindKernel("ComputeLambertianSingleReuseLightContributionVector");
            kernelComputeLambertianAll = renderingShader.FindKernel("ComputeLambertianAll");
            kernelComputeLambertianWithoudIdSingle = renderingShader.FindKernel("ComputeLambertianWithoutIdSingle");
            kernelComputeProbeLighting = renderingShader.FindKernel("ComputeProbeLighting");
            kernelComputeLightContribution = renderingShader.FindKernel("ComputeLightContribution");

            underShadowRelation = new Dictionary<Light, List<int>>();

            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

            targetTexture = new Texture2D(renderTexture.width, renderTexture.height );
        }

        /// <summary>
        /// This function is called after a new level was loaded.
        /// </summary>
        /// <param name="level">The index of the level that was loaded.</param>
        public void ResetOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
          
        }

        public void OnDisable()
        {
            samplePointBuffer.Release();
            illuminationBuffer.Release();
            lightContributionBuffer.Release();
            computeLightContributionBuffer.Release();
            floatBuffer1.Release();
            floatBuffer2.Release();
        }

        /// <summary>
        /// Compute ambient term with Kd.
        /// </summary>
        /// <param name="Kd">Material color of the vertex. (often as albedo * texel color)</param>
        /// <returns>Computed ambient color.</returns>
        public Color ComputeAmbient(Color Kd)
        {
#if COLOR_SPACE_LINEAR
            return RenderSettings.ambientLight.linear * Kd.linear;
#else
            return RenderSettings.ambientLight * Kd;
#endif
        }

        /// <summary>
        /// Compute Lambertian term without Id. (for inverse lighting)
        /// </summary>
        /// <param name="worldPosition">World position.</param>
        /// <param name="N">Normalized world normal of the veretx.</param>
        /// <param name="Kd">Material color of the vertex. (often as albedo * texel color)</param>
        /// <param name="light">The light cause the Lambertian effect.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        /// <returns>Computed lighting color.(without light intensity)</returns>
        public Color ComputeLambertianWithoutId(Vector3 worldPosition, Vector3 N, Color Kd, Light light, bool canCastShadow)
        {
            Vector3 L;
            float attenuation = ComputeLightAttenuation(worldPosition, light.transform.position, light.range, light.type);

            if (attenuation <= (0.0f + Mathf.Epsilon)) // Filter points that are not in range of light.
                return Color.black;

            if ((light.type == LightType.Spot) && !IsUnderSpotlight(worldPosition, light)) // Filter points that are not under spotlight.
                return Color.black;

            if (canCastShadow && IsUnderShadowOfLight(worldPosition, light)) // Filter points that are under shadow.
                return Color.black;

            L = ComputeLightDirection(worldPosition, light.transform, light.type);

#if COLOR_SPACE_LINEAR
            return Kd.linear * Mathf.Max(Vector3.Dot(N, L), 0.0f) * attenuation;
#else
            return Kd * Mathf.Max(Vector3.Dot(N, L), 0.0f) * attenuation;
#endif
        }

        /// <summary>
        /// Compute full Lambertian term.
        /// </summary>
        /// <param name="worldPosition">World position.</param>
        /// <param name="N">Normalized world normal of the veretx.</param>
        /// <param name="Kd">Material color of the vertex. (often as albedo * texel color)</param>
        /// <param name="light">The light cause the Lambertian effect.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        /// <returns>Computed lighting color.</returns>
        public Color ComputeLambertian(Vector3 worldPosition, Vector3 N, Color Kd, Light light, bool canCastShadow)
        {
            Color Id;
            Vector3 L;
            float attenuation = ComputeLightAttenuation(worldPosition, light.transform.position, light.range, light.type);

            if (attenuation <= (0.0f + Mathf.Epsilon)) // Filter points that are not in range of light.
                return Color.black;

            if ((light.type == LightType.Spot) && !IsUnderSpotlight(worldPosition, light)) // Filter points that are not under spotlight.
                return Color.black;

            if (canCastShadow && IsUnderShadowOfLight(worldPosition, light)) // Filter points that are under shadow.
                return Color.black;

            Id = light.color * light.intensity;
            L = ComputeLightDirection(worldPosition, light.transform, light.type);

#if COLOR_SPACE_LINEAR
            return Id.linear * Kd.linear * Mathf.Max(Vector3.Dot(N, L), 0.0f) * attenuation;
#else
            return Id * Kd * Mathf.Max(Vector3.Dot(N, L), 0.0f) * attenuation;
#endif
        }

        /// <summary>
        /// Compute full Lambertian term.
        /// </summary>
        /// <param name="lightContribution">The illumination without computing Id.</param>
        /// <param name="light">The light cause the Lambertian effect.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        /// <returns>Computed lighting color.</returns>
        public Color ComputeLambertian(Color lightContribution, Light light, bool canCastShadow)
        {
            Color Id = light.color * light.intensity;

#if COLOR_SPACE_LINEAR
            return Id.linear * lightContribution;
#else
            return Id * lightContribution;
#endif
        }

        /// <summary>
        /// Compute a light direction (from interest point to light) according to light's type.
        /// </summary>
        /// <param name="interestWorldPosition">The position as start point.</param>
        /// <param name="lightTransform">The transform of light to get information about position or direction.</param>
        /// <param name="lightType">The type of target light.</param>
        /// <returns>Computed light direction (from interest point to light).</returns>
        Vector3 ComputeLightDirection(Vector3 interestWorldPosition, Transform lightTransform, LightType lightType)
        {
            if (lightType == LightType.Directional)
                return -lightTransform.forward;
            else
                return (lightTransform.position - interestWorldPosition).normalized;
        }

        /// <summary>
        /// A fucntion to compute light's attenuation.
        /// attenuation (point lights and spotlights) = 1.0 / (1.0 + 25.0 * r * r), r goes from 0 at light position to 1 at it's range
        /// attenuation (directional lights) will be 1.0f;
        /// </summary>
        /// <param name="interestWorldPosition">The position that you want to know how the attenuation is.</param>
        /// <param name="lightWorldPosition">The position of target light.</param>
        /// <param name="lightRange">The range of target light.</param>
        /// <param name="lightType">The type of target light.</param>
        /// <returns>Computed light's attenuation.</returns>
        public float ComputeLightAttenuation(Vector3 interestWorldPosition, Vector3 lightWorldPosition, float lightRange, LightType lightType)
        {
            if (lightType == LightType.Directional)
                return 1.0f;
            else
            {
                float attenFactor = (lightWorldPosition - interestWorldPosition).magnitude / lightRange;

                if (attenFactor > 1.0f)
                    return 0.0f;
                else
                    return 1.0f / (1.0f + 25.0f * attenFactor * attenFactor);
            }
        }

        /// <summary>
        /// Check whether a position is under the spotlight.
        /// </summary>
        /// <param name="positionToCheck">The position to check whether it is under the spotlight.</param>
        /// <param name="spotlight">The spotlight that is to checked with.</param>
        /// <returns>Is true when the position is under the spotlight.</returns>
        public bool IsUnderSpotlight(Vector3 positionToCheck, Light spotlight)
        {
            Vector3 lightToPointDirection = (positionToCheck - spotlight.transform.position).normalized;
            float theta = Mathf.Acos(Vector3.Dot(lightToPointDirection, spotlight.transform.forward)) * Mathf.Rad2Deg;
            float attenuation = ComputeLightAttenuation(positionToCheck, spotlight.transform.position, spotlight.range, LightType.Spot);

            return (spotlight.spotAngle >= (theta * 2)) && (attenuation > 0.0f);
        }

        /// <summary>
        /// Check whether a position is under the spotlight. (Don't consider attenuation.)
        /// </summary>
        /// <param name="positionToCheck">The position to check whether it is under the spotlight.</param>
        /// <param name="spotlight">The spotlight that is to checked with.</param>
        /// <returns>Is true when the position is under the spotlight.</returns>
        public bool IsUnderSpotlightNoAtten(Vector3 positionToCheck, Light spotlight)
        {
            Vector3 lightToPointDirection = (positionToCheck - spotlight.transform.position).normalized;
            float theta = Mathf.Acos(Vector3.Dot(lightToPointDirection, spotlight.transform.forward)) * Mathf.Rad2Deg;

            return (spotlight.spotAngle >= (theta * 2));
        }

        /// <summary>
        /// Check whether a position is under the shadow of a light.
        /// </summary>
        /// <param name="positionToCheck">The position to check whether it is under the shadow of a light.</param>
        /// <param name="shadowLight">The light that cast the shadow which may occlude some points.</param>
        /// <returns>Is true when the positionToCheck is under the shadow of the shadowLight.</returns>
        bool IsUnderShadowOfLight(Vector3 positionToCheck, Light shadowLight)
        {
            float maxDistance = 0.0f;

            if (shadowLight.type == LightType.Directional)
                maxDistance = Mathf.Infinity;
            else
                maxDistance = Vector3.Distance(shadowLight.transform.position, positionToCheck);

            return Physics.Raycast(positionToCheck, ComputeLightDirection(positionToCheck, shadowLight.transform, shadowLight.type), maxDistance, 1);
        }

        #region GPU computing related
        /// <summary>
        /// Setup sample point's compute buffer and send to compute shader.
        /// </summary>
        /// <param name="samplePoint">The sample points to be computed.</param>
        public void SetupSamplePointBuffer(List<SamplePoint> samplePoint)
        {
            SamplePointData[] samplePointData = new SamplePointData[samplePoint.Count];

            lightContributionData = new LightContributionData[samplePoint.Count];
            computeLightContributionData = new ComputeLightContributionData[samplePoint.Count];
            lightContributionBuffer = new ComputeBuffer(samplePoint.Count, Marshal.SizeOf(typeof(LightContributionData)));
            computeLightContributionBuffer = new ComputeBuffer(samplePoint.Count, Marshal.SizeOf(typeof(ComputeLightContributionData)));
            samplePointBuffer = new ComputeBuffer(samplePoint.Count, Marshal.SizeOf(typeof(SamplePointData)));
            illuminationBuffer = new ComputeBuffer(samplePoint.Count, sizeof(float) * 4);
            illumination = new Color[samplePoint.Count];
            floatBuffer1 = new ComputeBuffer(samplePoint.Count, sizeof(float));
            floatBuffer2 = new ComputeBuffer(samplePoint.Count, sizeof(float));
            floatData1 = new float[samplePoint.Count];
            floatData2 = new float[samplePoint.Count];
            for (int i = 0; i < samplePoint.Count; i++)
            {
                samplePointData[i].Kd = samplePoint[i].Kd;
                samplePointData[i].worldPosition = samplePoint[i].worldPosition;
                samplePointData[i].worldNormal = samplePoint[i].worldNormal;
            }
            samplePointBuffer.SetData(samplePointData);
            renderingShader.SetBuffer(kernelComputeLambertianSingle, "samplePoint", samplePointBuffer);
            renderingShader.SetBuffer(kernelComputeLambertianSingle, "illumination", illuminationBuffer);
            renderingShader.SetBuffer(kernelComputeLambertianSingleReuseLightContributionVector, "illumination", illuminationBuffer);
            renderingShader.SetBuffer(kernelComputeLambertianAll, "samplePoint", samplePointBuffer);
            renderingShader.SetBuffer(kernelComputeLambertianAll, "illumination", illuminationBuffer);
            renderingShader.SetBuffer(kernelComputeLambertianWithoudIdSingle, "samplePoint", samplePointBuffer);
            renderingShader.SetBuffer(kernelComputeLambertianWithoudIdSingle, "illumination", illuminationBuffer); 
            renderingShader.SetBuffer(kernelComputeProbeLighting, "samplePoint", samplePointBuffer);
            renderingShader.SetBuffer(kernelComputeLightContribution, "floatBuffer1", floatBuffer1);
            renderingShader.SetBuffer(kernelComputeLightContribution, "floatBuffer2", floatBuffer2);
            renderingShader.SetInt("samplePointNumber", samplePoint.Count);
            renderingShader.SetFloat("epsilon", Mathf.Epsilon);
        }

        /// <summary>
        /// Setup light's compute buffer and send to compute shader.
        /// </summary>
        /// <param name="kernelId">Is the kernel that should the buffer to be applied.</param>
        /// <param name="light">The light to be computed.</param>
        void SetupLightBuffer(int kernelId, Light light)
        {
            LightData[] lightData = new LightData[1];

            lightBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(LightData)));
#if COLOR_SPACE_LINEAR
            lightData[0].Id = (light.color * light.intensity).linear;
#else
            lightData[0].Id = light.color * light.intensity;
#endif
            lightData[0].worldPosition = light.transform.position;
            lightData[0].forward = light.transform.forward;
            lightData[0].range = light.range;
            lightData[0].spotAngle = light.spotAngle;
            lightData[0].type = (int)light.type;
            lightBuffer.SetData(lightData);
            renderingShader.SetBuffer(kernelId, "light", lightBuffer);
        }

        /// <summary>
        /// Setup light's compute buffer and send to compute shader.
        /// </summary>
        /// <param name="kernelId">Is the kernel that should the buffer to be applied.</param>
        /// <param name="light">The light to be computed.</param>
        void SetupLightBuffer(int kernelId, List<Light> light)
        {
            LightData[] lightData = new LightData[light.Count];

            lightBuffer = new ComputeBuffer(light.Count, Marshal.SizeOf(typeof(LightData)));
            for (int i = 0; i < light.Count; i++)
            {
#if COLOR_SPACE_LINEAR
                lightData[i].Id = (light[i].color * light[i].intensity).linear;
#else
                lightData[i].Id = light[i].color * light[i].intensity;
#endif
                lightData[i].worldPosition = light[i].transform.position;
                lightData[i].forward = light[i].transform.forward;
                lightData[i].range = light[i].range;
                lightData[i].spotAngle = light[i].spotAngle;
                lightData[i].type = (int)light[i].type;
            }
            lightBuffer.SetData(lightData);
            renderingShader.SetBuffer(kernelId, "light", lightBuffer);
        }

        /// <summary>
        /// Setup shadow's compute buffer and send to compute shader.
        /// </summary>
        /// <param name="kernelId">Is the kernel that should the buffer to be applied.</param>
        /// <param name="samplePoint">The sample points to be computed.</param>
        /// <param name="light">The light to be computed.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        void SetupShadowBuffer(int kernelId, List<SamplePoint> samplePoint, List<Light> light, bool canCastShadow)
        {
            ShadowData[] shadowTesting = new ShadowData[samplePoint.Count * light.Count];

            shadowBuffer = new ComputeBuffer(samplePoint.Count * light.Count, sizeof(int));
            for (int i = 0; i < light.Count; i++)
            {
                for (int j = 0; j < samplePoint.Count; j++)
                    if (canCastShadow)
                        if ((light[i].type == LightType.Spot) && !IsUnderSpotlightNoAtten(samplePoint[j].worldPosition, light[i])) // Also filter points that are not under spotlight.
                            shadowTesting[j + i * samplePoint.Count].isUnderShadowOfLight = 1;
                        else
                            shadowTesting[j + i * samplePoint.Count].isUnderShadowOfLight = (IsUnderShadowOfLight(samplePoint[j].worldPosition, light[i]) ? 1 : 0);
                    else
                        shadowTesting[j + i * samplePoint.Count].isUnderShadowOfLight = 0;
            }
            shadowBuffer.SetData(shadowTesting);
            renderingShader.SetBuffer(kernelId, "shadowTesting", shadowBuffer);
        }

        /// <summary>
        /// Setup SHCoefficient's compute buffer and send to compute shader.
        /// </summary>
        /// <param name="kernelId">Is the kernel that should the buffer to be applied.</param>
        /// <param name="samplePoint">The sample points to be computed.</param>
        void SetupSHCoefficientBuffer(int kernelId, List<SamplePoint> samplePoint)
        {
            SHCoefficientData[] SHCoefficientData = new SHCoefficientData[samplePoint.Count];
            SphericalHarmonicsL2 sh;

            SHCoefficientBuffer = new ComputeBuffer(samplePoint.Count, Marshal.SizeOf(typeof(SHCoefficientData)));
            for(int i = 0; i < samplePoint.Count; i++)
            {
                LightProbes.GetInterpolatedProbe(samplePoint[i].worldPosition, null, out sh);
                LightProbeUtility.ComputeSHCoefficients(sh);
                SHCoefficientData[i].SHA1 = LightProbeUtility.SHA[0];
                SHCoefficientData[i].SHA2 = LightProbeUtility.SHA[1];
                SHCoefficientData[i].SHA3 = LightProbeUtility.SHA[2];
                SHCoefficientData[i].SHB1 = LightProbeUtility.SHB[0];
                SHCoefficientData[i].SHB2 = LightProbeUtility.SHB[1];
                SHCoefficientData[i].SHB3 = LightProbeUtility.SHB[2];
                SHCoefficientData[i].SHC = LightProbeUtility.SHC;
            }
            SHCoefficientBuffer.SetData(SHCoefficientData);
            renderingShader.SetBuffer(kernelId, "SHCoefficient", SHCoefficientBuffer);
        }

        /// <summary>
        /// Setup ComputeLightContributionBuffer compute buffer and send to compute shader.
        /// </summary>
        /// <param name="kernelId">Is the kernel that should the buffer to be applied.</param>
        /// <param name="samplePoint">The sample points to be computed.</param>
        void SetupComputeLightContributionBuffer(int kernelId, List<SamplePoint> samplePoint)
        {
            for (int i = 0; i < samplePoint.Count; i++)
            {
                computeLightContributionData[i].currentIllumination = samplePoint[i].currentIllumination;
                computeLightContributionData[i].goalIllumination = samplePoint[i].GoalIllumination;
                computeLightContributionData[i].worldPosition = samplePoint[i].worldPosition;
            }
            computeLightContributionBuffer.SetData(computeLightContributionData);
            renderingShader.SetBuffer(kernelId, "computeLightContributionData", computeLightContributionBuffer);
        }

        /// <summary>
        /// Compute full Lambertian term with GPU and add to sample point's illumination.
        /// </summary>
        /// <param name="samplePoint">The sample points to be computed.</param>
        /// <param name="light">The light cause the Lambertian effect.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        public void ComputeAndAddLambertianGPU(List<SamplePoint> samplePoint, Light light, bool canCastShadow)
        {
            uint threadGroupSizeX;
            uint threadGroupSizeY;
            uint threadGroupSizeZ;

            renderingShader.SetInt("lightNumber", 1);
            SetupLightBuffer(kernelComputeLambertianSingle, light);
            renderingShader.GetKernelThreadGroupSizes(kernelComputeLambertianSingle, 
                out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            renderingShader.Dispatch(kernelComputeLambertianSingle, Mathf.CeilToInt((float)samplePoint.Count / threadGroupSizeX), 1, 1);
            illuminationBuffer.GetData(illumination);

            // Apply illumination result to sample points.
            for (int i = 0; i < samplePoint.Count; i++)
            {
                if (illumination[i].r <= 0.0f + Mathf.Epsilon &&
                    illumination[i].g <= 0.0f + Mathf.Epsilon &&
                    illumination[i].b <= 0.0f + Mathf.Epsilon) // Is filtered by attenuation or spotlight.
                    continue;

                if (canCastShadow && IsUnderShadowOfLight(samplePoint[i].worldPosition, light)) // Filter points that are under shadow.
                    continue;

                samplePoint[i].currentIllumination += illumination[i];
            }

            lightBuffer.Release();
        }

        /// <summary>
        /// Compute full Lambertian term with GPU and add to sample point's illumination.(all lights)
        /// </summary>
        /// <param name="samplePoint">The sample points to be computed.</param>
        /// <param name="light">The lights cause the Lambertian effect.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        public void ComputeAndAddLambertianGPU(List<SamplePoint> samplePoint, List<Light> light, bool canCastShadow)
        {
            uint threadGroupSizeX;
            uint threadGroupSizeY;
            uint threadGroupSizeZ;

            renderingShader.SetInt("lightNumber", light.Count);
            SetupLightBuffer(kernelComputeLambertianAll, light);
            SetupShadowBuffer(kernelComputeLambertianAll, samplePoint, light, canCastShadow);
            renderingShader.GetKernelThreadGroupSizes(kernelComputeLambertianAll,
                out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            renderingShader.Dispatch(kernelComputeLambertianAll, Mathf.CeilToInt((float)samplePoint.Count / threadGroupSizeX), 1, 1);
            illuminationBuffer.GetData(illumination);

            // Apply illumination result to sample points.
            for (int i = 0; i < samplePoint.Count; i++)
            {
                if (illumination[i].r <= 0.0f + Mathf.Epsilon &&
                    illumination[i].g <= 0.0f + Mathf.Epsilon &&
                    illumination[i].b <= 0.0f + Mathf.Epsilon) // Is filtered by attenuation, spotlight or shadow.
                    continue;

                samplePoint[i].currentIllumination = illumination[i];
            }

            lightBuffer.Release();
            shadowBuffer.Release();
        }

        /// <summary>
        /// Compute Lambertian term without Id. (for inverse lighting)
        /// </summary>
        /// <param name="samplePoint">The sample points to be computed.</param>
        /// <param name="light">The light cause the Lambertian effect.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        public void ComputeAndAddLambertianWithoudIdGPU(List<SamplePoint> samplePoint, Light light, bool canCastShadow)
        {
            uint threadGroupSizeX;
            uint threadGroupSizeY;
            uint threadGroupSizeZ;

            renderingShader.SetInt("lightNumber", 1);
            SetupLightBuffer(kernelComputeLambertianWithoudIdSingle, light);
            renderingShader.GetKernelThreadGroupSizes(kernelComputeLambertianWithoudIdSingle,
                out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            renderingShader.Dispatch(kernelComputeLambertianWithoudIdSingle, Mathf.CeilToInt((float)samplePoint.Count / threadGroupSizeX), 1, 1);
            illuminationBuffer.GetData(illumination);

            // Apply illumination result to sample points.
            for (int i = 0; i < samplePoint.Count; i++)
            {
                if (illumination[i].r <= 0.0f + Mathf.Epsilon &&
                    illumination[i].g <= 0.0f + Mathf.Epsilon &&
                    illumination[i].b <= 0.0f + Mathf.Epsilon) // Is filtered by attenuation or spotlight.
                {
                    samplePoint[i].currentIllumination = Color.black;
                    continue;
                }

                if (canCastShadow && IsUnderShadowOfLight(samplePoint[i].worldPosition, light)) // Filter points that are under shadow.
                {
                    samplePoint[i].currentIllumination = Color.black;
                    continue;
                }

                samplePoint[i].currentIllumination = illumination[i]; // No need to += for inverse lighting purpose.
            }

            lightBuffer.Release();
        }

        /// <summary>
        /// Compute probe lighting with GPU. (This dont perform better than CPU version.
        /// </summary>
        /// <param name="samplePoint">The sample points to be computed.</param>
        /// <param name="probeIllumination">Is used to save the computed illumination by probe lighting.</param>
        public void ComputeProbeLightingGPU(List<SamplePoint> samplePoint, ref Color[] probeLightingIllumination)
        {
            uint threadGroupSizeX;
            uint threadGroupSizeY;
            uint threadGroupSizeZ;

            SetupSHCoefficientBuffer(kernelComputeProbeLighting, samplePoint);
            renderingShader.GetKernelThreadGroupSizes(kernelComputeProbeLighting,
                out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            renderingShader.Dispatch(kernelComputeProbeLighting, Mathf.CeilToInt((float)samplePoint.Count / threadGroupSizeX), 1, 1);
            illuminationBuffer.GetData(probeLightingIllumination);

            SHCoefficientBuffer.Release();
        }

        /// <summary>
        /// Compute light contribution with GPU.
        /// </summary>
        /// <param name="samplePoint">The sample point to compute.</param>
        /// <param name="light">The light to compute.</param>
        /// <returns>The light contribution.</returns>
        public float ComputeLightContributionGPU(List<SamplePoint> samplePoint, Light light)
        {
            uint threadGroupSizeX;
            uint threadGroupSizeY;
            uint threadGroupSizeZ;
            float contribution = 0.0f;
            float goalAmount = 0.0f;

            renderingShader.SetInt("lightNumber", 1);
            SetupComputeLightContributionBuffer(kernelComputeLightContribution, samplePoint);
            SetupLightBuffer(kernelComputeLightContribution, light);
            renderingShader.GetKernelThreadGroupSizes(kernelComputeLightContribution,
                out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            renderingShader.Dispatch(kernelComputeLightContribution, Mathf.CeilToInt((float)samplePoint.Count / threadGroupSizeX), 1, 1);
            floatBuffer1.GetData(floatData1);
            floatBuffer2.GetData(floatData2);

            for(int i = 0; i < floatData1.Length; i++)
            {
                contribution += floatData1[i];
                goalAmount += floatData2[i];
            }

            lightBuffer.Release();
            return Mathf.Sqrt(contribution + Mathf.Epsilon) / Mathf.Sqrt(goalAmount + Mathf.Epsilon);
        }
        #endregion

        #region Reuse light contribution vector
        /// <summary>
        /// Initialize reused vector with size.
        /// </summary>
        /// <param name="size">The size of reused vector.</param>
        public void InitializeReusedVector(int size)
        {
            lightContributionVectorByLightData = new Dictionary<LightData, Color[]>();
            reusedVector = new Color[size];
        }

        /// <summary>
        /// Decide whether a light's contributino vector is reusable.
        /// </summary>
        /// <param name="light">The light to be checked.</param>
        /// <returns>Is true  when the light's contributino vector is reusable.</returns>
        public bool CanReuseLightContributionVector(Light light)
        {
            SetupLightDataKey(light);
            if (lightContributionVectorByLightData.ContainsKey(tempLightData))
                return true;

            return false;
        }

        /// <summary>
        /// Setup light contribution vector for reusing.
        /// </summary>
        /// <param name="light">The contribution vector of light to be reused.</param>
        /// <param name="samplePoint">The sample points affected by the light</param>
        public void SetupReusedLightContributionVector(Light light, List<SamplePoint> samplePoint)
        {
            SetupLightDataKey(light);
            ComputeLightContributionVectorGPU(samplePoint, light, InverseLightingSolver.instance.canCastShadow);
            lightContributionVectorByLightData.Add(tempLightData, (Color[])reusedVector.Clone());
        }

        /// <summary>
        /// Reuse a light's contribution vector.
        /// </summary>
        /// <param name="light">The light to reuse its contribution vector.</param>
        public void ReuseLightContributionVector(Light light)
        {
            Color[] contributionVector;

            SetupLightDataKey(light);
            lightContributionVectorByLightData.TryGetValue(tempLightData, out contributionVector); // The value will be reference.
            contributionVector.CopyTo(reusedVector, 0);
        }

        /// <summary>
        /// Setup light contribution's compute buffer and send to compute shader.
        /// </summary>
        /// <param name="kernelId">Is the kernel that should the buffer to be applied.</param>
        void SetupLightContributionBuffer(int kernelId)
        {
            for (int i = 0; i < lightContributionData.Length; i++)
                lightContributionData[i].lightContribution = reusedVector[i];

            lightContributionBuffer.SetData(lightContributionData);
            renderingShader.SetBuffer(kernelId, "lightContributionVector", lightContributionBuffer);
        }

        /// <summary>
        /// Compute light contribution vector. (for reusing it)
        /// </summary>
        /// <param name="samplePoint">The sample points to be computed.</param>
        /// <param name="light">The light to compute the contribution vector.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        public void ComputeLightContributionVectorGPU(List<SamplePoint> samplePoint, Light light, bool canCastShadow)
        {
            uint threadGroupSizeX;
            uint threadGroupSizeY;
            uint threadGroupSizeZ;

            renderingShader.SetInt("lightNumber", 1);
            SetupLightBuffer(kernelComputeLambertianWithoudIdSingle, light);
            renderingShader.GetKernelThreadGroupSizes(kernelComputeLambertianWithoudIdSingle,
                out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            renderingShader.Dispatch(kernelComputeLambertianWithoudIdSingle, Mathf.CeilToInt((float)samplePoint.Count / threadGroupSizeX), 1, 1);
            illuminationBuffer.GetData(illumination);

            // Apply illumination result to reused vector.
            for (int i = 0; i < samplePoint.Count; i++)
            {
                if (illumination[i].r <= 0.0f + Mathf.Epsilon &&
                    illumination[i].g <= 0.0f + Mathf.Epsilon &&
                    illumination[i].b <= 0.0f + Mathf.Epsilon) // Is filtered by attenuation or spotlight.
                {
                    reusedVector[i] = Color.black;
                    continue;
                }

                if (canCastShadow && IsUnderShadowOfLight(samplePoint[i].worldPosition, light)) // Filter points that are under shadow.
                {
                    reusedVector[i] = Color.black;
                    continue;
                }

                reusedVector[i] = illumination[i]; // No need to += for inverse lighting purpose.
            }

            lightBuffer.Release();
        }

        /// <summary>
        /// Compute full Lambertian term with GPU and add to sample point's illumination.
        /// </summary>
        /// <param name="samplePoint">The sample points to be computed.</param>
        /// <param name="light">The light cause the Lambertian effect.</param>
        /// <param name="canCastShadow">Is true when we consider shadows in lighting computation.</param>
        public void ComputeAndAddLambertianGPUReuseLightContributionVector(List<SamplePoint> samplePoint, Light light, bool canCastShadow)
        {
            uint threadGroupSizeX;
            uint threadGroupSizeY;
            uint threadGroupSizeZ;

            renderingShader.SetInt("lightNumber", 1);
            SetupLightContributionBuffer(kernelComputeLambertianSingleReuseLightContributionVector);
            SetupLightBuffer(kernelComputeLambertianSingleReuseLightContributionVector, light);
            renderingShader.GetKernelThreadGroupSizes(kernelComputeLambertianSingleReuseLightContributionVector,
                out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            renderingShader.Dispatch(kernelComputeLambertianSingleReuseLightContributionVector, Mathf.CeilToInt((float)samplePoint.Count / threadGroupSizeX), 1, 1);
            illuminationBuffer.GetData(illumination);

            // Apply illumination result to sample points.
            for (int i = 0; i < samplePoint.Count; i++)
                samplePoint[i].currentIllumination += illumination[i];

            lightBuffer.Release();
        }

        /// <summary>
        /// Use the provided light to setup light data for key searching. 
        /// </summary>
        /// <param name="light"></param>
        void SetupLightDataKey(Light light)
        {
            tempLightData.worldPosition = light.transform.position;
            tempLightData.forward = light.transform.forward;
            tempLightData.range = light.range;
            tempLightData.spotAngle = light.spotAngle;
            tempLightData.type = (int)light.type;
        }
        #endregion

        /// <summary>
        /// Check the lights in the whole scene to classify which sample points are in the shadow.
        /// </summary>
        public void UpdateUnderShadowRelation()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            watch.Stop();
            print("The time it took to update the shadow relation is : " + watch.ElapsedMilliseconds);

            foreach (KeyValuePair<Light, List<int>> val in underShadowRelation)
                print("Light : " + val.Key.GetHashCode() + " has " + val.Value.Count + " points under shadow.");
        }

        /// <summary>
        /// Use to query if certain sample point is under the shadow of certain light.
        /// </summary>
        /// <returns></returns>
        public bool IsUnderShadowOfLightShadowMap(int samplePointIndex, Light targetLight)
        {         
            return underShadowRelation[targetLight].Contains(samplePointIndex);
        }

        public bool IsUnderShadowOfLightShadowMap(Vector3 worldPosition, Light targetLight)
        {
            bool val = false;
            for (int i = 0; i < underShadowRelation[targetLight].Count; i++)
            {
                if (SamplePointManager.instance.generatedSamplePoint[underShadowRelation[targetLight][i]].worldPosition == worldPosition)
                {
                    val = true;
                    break;
                }                    
            }

            return val;
        }
        
    }
}
