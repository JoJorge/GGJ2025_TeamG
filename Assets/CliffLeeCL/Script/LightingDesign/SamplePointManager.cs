#define DEBUG_CL
#define COLOR_SPACE_LINEAR

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

namespace CliffLeeCL {
    /// <summary>
    /// The class can generate sample points with different methods and manage all sample points status.
    /// </summary>
    public class SamplePointManager : MonoBehaviour{
        /// <summary>
        /// The variable is used to access this class.
        /// </summary>
        public static SamplePointManager instance;

        /// <summary>
        /// token prefab to be spawned.
        /// </summary>
        public GameObject tokenPrefab;
        /// <summary>
        /// Is true when you want to spawn sample points' tokens.
        /// </summary>
        public bool canSpawnToken = true;
        /// <summary>
        /// Is true when you want to see sample points' tokens.
        /// </summary>
        public bool canShowToken = true;
        /// <summary>
        /// Maximum number of sample points. (or current number of sample points for mesh vertex)
        /// </summary>
        public int samplePointNumber = 10000;
        [HideInInspector]
        /// <summary>
        /// The list stores all generated sample points.
        /// </summary>
        public List<SamplePoint> generatedSamplePoint = new List<SamplePoint>();

        [Header("Camera Sampler")]
        [Range(1, 179)]
        /// <summary>
        /// Define field of view of each camera sampler.
        /// </summary>
        public float cameraFieldOfView = 90.0f;
        /// <summary>
        /// Indicate which layers that camera sampler can see.
        /// </summary>
        public LayerMask cameraCullingMask = 1;
        public Camera[] outsideCamera;
        /// <summary>
        /// Is used for ground truth generation.
        /// </summary>
        Texture2D targetTexture;
        /// <summary>
        /// The single camera sampler.
        /// </summary>
        GameObject[] cameraSamplerObject;
        /// <summary>
        /// The objects to generate camera sampler in a cube shape.
        /// </summary>
        GameObject[] cameraSamplerCubeObject;
        /// <summary>
        /// The camera that are used to sample the global lighting.
        /// </summary>
        List<Camera> cameraSampler = new List<Camera>();
        /// <summary>
        /// It's used to be target texture of every camera samplers.
        /// </summary>
        RenderTexture renderTexture;

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
        }

        /// <summary>
        /// This function is called after a new level was loaded.
        /// </summary>
        /// <param name="level">The index of the level that was loaded.</param>
        public void ResetOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (instance == this)
            {
                PossibleLightBoundingVolume possibleLightVolume;
                Stopwatch watch = new Stopwatch();
                List<Light> existingLight;
                List<Light> existingLightWithoutDirectionalLight;
                int outsideSamplePointCount;
                int insideSamplePointCount;

                watch.Reset();
                watch.Start();
                GenerateSamplePointFromMeshTriangle();
                outsideSamplePointCount = generatedSamplePoint.Count;
                FilterUnseenSamplePoint();
                insideSamplePointCount = generatedSamplePoint.Count;
                outsideSamplePointCount -= insideSamplePointCount;
                watch.Stop();
                print("Support GPU instancing? " + SystemInfo.supportsInstancing);
                print("Generate and filter sample points time: " + watch.ElapsedMilliseconds + "ms");
                print("Outside sample points: " + outsideSamplePointCount);
                print("Inside sample points: " + insideSamplePointCount);

                GameObject preprocessLight = new GameObject("PreprocessLight");
                possibleLightVolume = GameObject.FindGameObjectWithTag("PossibleLightBoundingVolume").GetComponent<PossibleLightBoundingVolume>();
                RenderingSystem.instance.SetupSamplePointBuffer(generatedSamplePoint);
                RenderingSystem.instance.InitializeReusedVector(generatedSamplePoint.Count);
                InverseLightingSolver.instance.AddLightWithPosition(InverseLightingSolver.instance.pointLightPrefab, 
                    possibleLightVolume.possibleCoarseLightPosition, preprocessLight.transform);
                InverseLightingSolver.instance.AddLightWithPosition(InverseLightingSolver.instance.pointLightPrefab,
                    possibleLightVolume.possibleSpreadLightPosition, preprocessLight.transform);
                existingLight = new List<Light>(FindObjectsOfType<Light>());
                existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);
                SetupCameraSampler();
                StartCoroutine(ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, generatedSamplePoint, true,
                    InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
                preprocessLight.SetActive(false);

                if (canSpawnToken && canShowToken)
                {
                    for (int i = 0; i < generatedSamplePoint.Count; i++)
                        generatedSamplePoint[i].ToggleTokenVisibility();
                }
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (canSpawnToken && Input.GetKeyDown(KeyCode.V))
            {
                for (int i = 0; i < generatedSamplePoint.Count; i++)
                {
                    generatedSamplePoint[i].isAlwaysVisible = !generatedSamplePoint[i].isAlwaysVisible;
                    generatedSamplePoint[i].ToggleTokenVisibility();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                List<Light> existingLight = new List<Light>(FindObjectsOfType<Light>());
                List<Light> existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);

                StartCoroutine(ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, generatedSamplePoint, true,
                    InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
            }

            if(Input.GetKeyDown(KeyCode.Q))
            {
                RenderingSystem.instance.UpdateUnderShadowRelation();
            }
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        void OnDisable()
        {
            SceneManager.sceneLoaded -= ResetOnSceneLoaded;
        }

        /// <summary>
        /// Reset all painted sample point status and color.
        /// </summary>
        public void ResetPaintedSamplePoint(List<SamplePoint> samplePoint)
        {
            for (int i = 0; i < generatedSamplePoint.Count; i++)
                if (generatedSamplePoint[i].isPainted() )
                {
                    generatedSamplePoint[i].SetTokenVisibility(false);
                    generatedSamplePoint[i].currentIllumination = Color.black;
                    generatedSamplePoint[i].GoalIllumination = Color.white;
                    generatedSamplePoint[i].paintingStatus = 0;
                }
        }

        /// <summary>
        /// Compute all sample points' illumination with RenderingSystem or camera sampler.
        /// </summary>
        /// <param name="light">The lights illuminate all sample points.</param>
        /// <param name="samplePoint">All sample points to be computed.</param>
        /// <param name="canCopyToGoalIllumination">Is true when we want to copy current illumination to goal illumination.</param>
        /// <param name="canCastShadow">Is true when we want to consider shadows in local lighting.</param>
        /// <param name="CanUseGlobalIllumination">Is true when we want to use global illumination to compute illumiation of sample points'.</param>
        public IEnumerator ComputeCurrentIlluminationOfSamplePoint(List<Light> light, List<SamplePoint> samplePoint, bool canCopyToGoalIllumination, bool canCastShadow, bool CanUseGlobalIllumination)
        {
            if (CanUseGlobalIllumination)
            {
                //yield return StartCoroutine(ComputeIlluminationWithPixelGlobalLighting(samplePoint));
                yield return StartCoroutine(ComputeIlluminationWithLocalAndProbeLightingGPU(light, samplePoint, canCastShadow));
            }
            else
                yield return StartCoroutine(ComputeIlluminationWithLocalLightingGPU(light, samplePoint, canCastShadow));

            if (canCopyToGoalIllumination)
                yield return StartCoroutine(CopyCurrentIlluminationToGoalIllumination(samplePoint, true));
        }

        /// <summary>
        /// Compute all sample points' illumination with RenderingSystem.
        /// </summary>
        /// <param name="light">The lights illuminate all sample points.</param>
        /// <param name="samplePoint">All sample points to be computed.</param>
        /// <param name="canCastShadow">Is true when we want to consider shadows in local lighting.</param>
        IEnumerator ComputeIlluminationWithLocalLighting(List<Light> light, List<SamplePoint> samplePoint, bool canCastShadow)
        {
            Stopwatch totalWatch = new Stopwatch();

            totalWatch.Start();
            for (int i = 0; i < samplePoint.Count; i++)
                samplePoint[i].currentIllumination = Color.black;

            for (int i = 0; i < light.Count; i++)
            {
                if (RenderingSystem.instance.CanReuseLightContributionVector(light[i]))
                    RenderingSystem.instance.ReuseLightContributionVector(light[i]);
                else
                    RenderingSystem.instance.SetupReusedLightContributionVector(light[i], samplePoint);

                for (int j = 0; j < samplePoint.Count; j++)
                    samplePoint[j].currentIllumination += RenderingSystem.instance.ComputeLambertian(
                        RenderingSystem.instance.reusedVector[j], light[i], canCastShadow);
            }

            for (int i = 0; i < samplePoint.Count; i++)
            {
                samplePoint[i].currentIllumination += RenderingSystem.instance.ComputeAmbient(samplePoint[i].Kd);

#if COLOR_SPACE_LINEAR
                samplePoint[i].currentIllumination = samplePoint[i].currentIllumination.gamma;
#endif
            }
            totalWatch.Stop();
#if DEBUG_CL
            print("Compute local lighting: " + totalWatch.ElapsedMilliseconds + "ms");
#endif 
            yield break;
        }

        /// <summary>
        /// Compute all sample points' illumination with RenderingSystem.
        /// </summary>
        /// <param name="light">The lights illuminate all sample points.</param>
        /// <param name="samplePoint">All sample points to be computed.</param>
        /// <param name="canCastShadow">Is true when we want to consider shadows in local lighting.</param>
        IEnumerator ComputeIlluminationWithLocalLightingGPU(List<Light> light, List<SamplePoint> samplePoint, bool canCastShadow)
        {
            Stopwatch totalWatch = new Stopwatch();

            totalWatch.Start();
            for (int i = 0; i < samplePoint.Count; i++)
                samplePoint[i].currentIllumination = Color.black;

            for (int i = 0; i < light.Count; i++)
            {
                if (RenderingSystem.instance.CanReuseLightContributionVector(light[i]))
                    RenderingSystem.instance.ReuseLightContributionVector(light[i]);
                else
                    RenderingSystem.instance.SetupReusedLightContributionVector(light[i], samplePoint);

                RenderingSystem.instance.ComputeAndAddLambertianGPUReuseLightContributionVector(samplePoint, light[i], canCastShadow);
            }
            /*
            for (int i = 0; i < samplePoint.Count; i++)
            {
                samplePoint[i].currentIllumination += RenderingSystem.instance.ComputeAmbient(samplePoint[i].Kd);
#if COLOR_SPACE_LINEAR
                samplePoint[i].currentIllumination = samplePoint[i].currentIllumination.gamma;
#endif
            }*/
            totalWatch.Stop();
#if DEBUG_CL
            print("Compute local lighting: " + totalWatch.ElapsedMilliseconds + "ms");
#endif
            yield break;
        }

        /// <summary>
        /// Generate pixel global lighting result for a set of sample points.
        /// </summary>
        /// <param name="samplePoint">All sample points to be generated.</param>
        /// <param name="canSkipPaintedSamplePoint">Is true when you want to generation to pass painted sample points.</param>
        IEnumerator ComputeIlluminationWithPixelGlobalLighting(List<SamplePoint> samplePoint)
        {
            Stopwatch totalWatch = new Stopwatch();

            totalWatch.Start();
            for (int i = 0; i < 5; i++)
                yield return new WaitForEndOfFrame();

            for (int i = 0; i < cameraSampler.Count; i++)
            {
                targetTexture = GenerateRenderTexture(cameraSampler[i]);

                // Apply pixel ground truth.
                for (int j = 0; j < samplePoint.Count; j++)
                {
                    Vector3 worldSpacePoint = samplePoint[j].worldPosition;
                    Vector3 viewportSpacePoint = cameraSampler[i].WorldToViewportPoint(worldSpacePoint);

                    if (IsVisibleToCamera(samplePoint[j].worldPosition, cameraSampler[i]))
                        samplePoint[j].currentIllumination = targetTexture.GetPixelBilinear(viewportSpacePoint.x, viewportSpacePoint.y);
                }
            }
            totalWatch.Stop();
#if DEBUG_CL
            print("Compute pixel global lighting: " + totalWatch.ElapsedMilliseconds + "ms");
#endif
        }

        /// <summary>
        /// Compute all sample points' illumination with RenderingSystem and light probes.
        /// </summary>
        /// <param name="light">The lights illuminate all sample points.</param>
        /// <param name="samplePoint">All sample points to be computed.</param>
        /// <param name="canCastShadow">Is true when we want to consider shadows in local lighting.</param>
        /// <returns></returns>
        IEnumerator ComputeIlluminationWithLocalAndProbeLighting(List<Light> light, List<SamplePoint> samplePoint, bool canCastShadow)
        {
            List<Color> probeLight = new List<Color>();
            Stopwatch totalWatch = new Stopwatch();
            int waitForUpdateTime = 0;
            int localLightingTime = 0;

            totalWatch.Reset();
            totalWatch.Start();
            for (int i = 0; i < samplePoint.Count; i++)
                samplePoint[i].currentIllumination = Color.black;

            for (int i = 0; i < light.Count; i++) {
                if (RenderingSystem.instance.CanReuseLightContributionVector(light[i]))
                    RenderingSystem.instance.ReuseLightContributionVector(light[i]);
                else
                    RenderingSystem.instance.SetupReusedLightContributionVector(light[i], samplePoint);

                for (int j = 0; j < samplePoint.Count; j++)
                    samplePoint[j].currentIllumination += RenderingSystem.instance.ComputeLambertian(
                        RenderingSystem.instance.reusedVector[j], light[i], canCastShadow);
            }
            totalWatch.Stop();
            localLightingTime = (int)totalWatch.ElapsedMilliseconds;

            totalWatch.Reset();
            totalWatch.Start();
            for (int i = 0; i < 5; i++)
                yield return new WaitForEndOfFrame();
            totalWatch.Stop();
            waitForUpdateTime = (int)totalWatch.ElapsedMilliseconds;

            totalWatch.Reset();
            totalWatch.Start();
            // For computing probe light.(use for-loop because we want to count probe lighting time.)
            for (int i = 0; i < samplePoint.Count; i++)
                probeLight.Add(LightProbeUtility.ShadeSH9(samplePoint[i].worldPosition, samplePoint[i].worldNormal));
            totalWatch.Stop();

            for (int i = 0; i < samplePoint.Count; i++)
            {
#if COLOR_SPACE_LINEAR
                samplePoint[i].currentIllumination += (probeLight[i] * samplePoint[i].Kd.linear);
                samplePoint[i].currentIllumination = samplePoint[i].currentIllumination.gamma;
#else
                samplePoint[i].currentIllumination += (probeLight[i] * samplePoint[i].Kd);
#endif
            }

#if DEBUG_CL
            print("Wait for update: " + waitForUpdateTime + "ms");
            print("Compute local lighting: " + localLightingTime + "ms, " + "probe lighting: " + totalWatch.ElapsedMilliseconds + "ms");
            print("Compute global lighting: " + (waitForUpdateTime + localLightingTime + totalWatch.ElapsedMilliseconds) + "ms");
#endif
            yield break;
        }

        /// <summary>
        /// Compute all sample points' illumination with RenderingSystem and light probes.
        /// </summary>
        /// <param name="light">The lights illuminate all sample points.</param>
        /// <param name="samplePoint">All sample points to be computed.</param>
        /// <param name="canCastShadow">Is true when we want to consider shadows in local lighting.</param>
        /// <returns></returns>
        IEnumerator ComputeIlluminationWithLocalAndProbeLightingGPU(List<Light> light, List<SamplePoint> samplePoint, bool canCastShadow)
        {
            List<Color> probeLight = new List<Color>();
            Stopwatch totalWatch = new Stopwatch();
            int waitForUpdateTime = 0;
            int localLightingTime = 0;

            totalWatch.Reset();
            totalWatch.Start();
            for (int i = 0; i < samplePoint.Count; i++)
                samplePoint[i].currentIllumination = Color.black;

            for (int i = 0; i < light.Count; i++)
            {
                if (RenderingSystem.instance.CanReuseLightContributionVector(light[i]))
                    RenderingSystem.instance.ReuseLightContributionVector(light[i]);
                else
                    RenderingSystem.instance.SetupReusedLightContributionVector(light[i], samplePoint);

                RenderingSystem.instance.ComputeAndAddLambertianGPUReuseLightContributionVector(samplePoint, light[i], canCastShadow);
            }
            totalWatch.Stop();
            localLightingTime = (int)totalWatch.ElapsedMilliseconds;

            totalWatch.Reset();
            totalWatch.Start();
            for (int i = 0; i < 5; i++)
                yield return new WaitForEndOfFrame();
            totalWatch.Stop();
            waitForUpdateTime = (int)totalWatch.ElapsedMilliseconds;

            totalWatch.Reset();
            totalWatch.Start();
            // For computing probe light.(use for-loop because we want to count probe lighting time.)
            for (int i = 0; i < samplePoint.Count; i++)
            {
                probeLight.Add(LightProbeUtility.ShadeSH9(samplePoint[i].worldPosition, samplePoint[i].worldNormal));
                probeLight[i] = new Color(Mathf.Min(1.0f, probeLight[i].r), Mathf.Min(1.0f, probeLight[i].g), Mathf.Min(1.0f, probeLight[i].b));
            }
            totalWatch.Stop();

            for (int i = 0; i < samplePoint.Count; i++)
            {
#if COLOR_SPACE_LINEAR
                samplePoint[i].currentIllumination += (probeLight[i] * samplePoint[i].Kd.linear);
                samplePoint[i].currentIllumination = samplePoint[i].currentIllumination.gamma;
#else
                samplePoint[i].currentIllumination += (probeLight[i] * samplePoint[i].Kd);
#endif
            }

#if DEBUG_CL
            print("Reusing size: " + RenderingSystem.instance.lightContributionVectorByLightData.Count);
            print("Wait for update: " + waitForUpdateTime + "ms");
            print("Compute local lighting: " + localLightingTime + "ms, " + "probe lighting: " + totalWatch.ElapsedMilliseconds + "ms");
            print("Compute global lighting: " + (waitForUpdateTime + localLightingTime + totalWatch.ElapsedMilliseconds) + "ms");
#endif
            yield break;
        }

        /// <summary>
        /// Copy current illumination to goal illumination for all sample points'.
        /// </summary>
        /// <param name="samplePoint">All sample points to be copied.</param>
        /// <param name="canSkipPaintedSamplePoint">Is true when you want to pass painted sample points.</param>
        IEnumerator CopyCurrentIlluminationToGoalIllumination(List<SamplePoint> samplePoint, bool canSkipPaintedSamplePoint)
        {
            for(int i = 0; i < samplePoint.Count; i++)
            {
                if (canSkipPaintedSamplePoint && samplePoint[i].isPainted())
                    continue;

                samplePoint[i].GoalIllumination = samplePoint[i].currentIllumination;
            }

            yield break;
        }

        /// <summary>
        /// Generate sample point from every vertex of some object's mesh.
        /// samplePointNumber will represent all vertices' number.
        /// P.S. The function will generate more than its needs because FindTargetToSample consider submesh but this one doesn't.
        /// </summary>
        public void GenerateSamplePointFromMeshVertex()
        {
            List<GameObject> objectToSample = new List<GameObject>(GameObject.FindGameObjectsWithTag("Highlightable"));
            List<Mesh> meshToSample = new List<Mesh>();
            List<Transform> transformToSample = new List<Transform>();
            List<Material> materialToSample = new List<Material>();
            List<int> subMeshIndex = new List<int>();

            FindTargetToSample(objectToSample, ref meshToSample, ref transformToSample, ref materialToSample, ref subMeshIndex);

            // Main generation process.
            generatedSamplePoint.Clear();
            ClearSamplePointToken();
            for (int i = 0; i < meshToSample.Count; i++)
            {
                Vector3[] vertices = meshToSample[i].vertices;
                Vector3[] normals = meshToSample[i].normals;
                Vector2[] uv = meshToSample[i].uv;

                for (int j = 0; j < vertices.Length; j++)
                    GenerateSingleSamplePoint(transformToSample[i].TransformPoint(vertices[j]),
                                                transformToSample[i].TransformDirection(normals[j]),
                                                uv[j],
                                                materialToSample[i]);
            }

            samplePointNumber = generatedSamplePoint.Count;
        }

        /// <summary>
        /// Generate sample point randomly from every triangle of some object's mesh. 
        /// The larger the triangle is, the more points of the triangle will be sampled.
        /// samplePointNumber limit the total generated sample points. (will almost the number)
        /// P.S. This function don't guarantee that the sample points won't duplicate.
        /// P.S. Can handle mutiple material but not efficient (dont reuse vertices, normals, uvs infomation when dealing with sub meshes.)
        /// </summary>
        public void GenerateSamplePointFromMeshTriangle()
        {
            List<GameObject> objectToSample = new List<GameObject>(GameObject.FindGameObjectsWithTag("Highlightable"));
            List<Mesh> meshToSample = new List<Mesh>();
            List<Transform> transformToSample = new List<Transform>();
            List<Material> materialToSample = new List<Material>();
            List<int> subMeshIndex = new List<int>();
            List<List<Vector3>> meshVertex = new List<List<Vector3>>();
            List<List<Vector3>> meshNormal = new List<List<Vector3>>();
            List<List<Vector2>> meshTexcoord = new List<List<Vector2>>();
            List<List<int>> meshVertexIndexOfTriangle = new List<List<int>>();
            List<List<float>> meshTriangleAreaPdf = new List<List<float>>();
            List<List<float>> meshTriangleAreaCdf = new List<List<float>>();
            float totalMeshTriangleArea = 0.0f;

            FindTargetToSample(objectToSample, ref meshToSample, ref transformToSample, ref materialToSample, ref subMeshIndex);

            // Save all vertices and triangles. (for reusing data afterwards.)
            for (int i = 0; i < meshToSample.Count; i++)
            {
                meshVertex.Add(new List<Vector3>());
                meshNormal.Add (new List<Vector3>());
                meshTexcoord.Add(new List<Vector2>());
                meshVertexIndexOfTriangle.Add(new List<int>());
                meshToSample[i].GetVertices(meshVertex[i]);
                meshToSample[i].GetNormals(meshNormal[i]);
                meshToSample[i].GetUVs(0, meshTexcoord[i]);
                meshToSample[i].GetTriangles(meshVertexIndexOfTriangle[i], subMeshIndex[i]);
            }

            // Compute all triangle area pdf and cdf.
            for (int i = 0; i < meshToSample.Count; i++)
            {
                List<float> triangleAreaPdf = new List<float>();
                List<float> triangleAreaCdf = new List<float>();

                for (int triangleFirstIndex = 0; triangleFirstIndex < meshVertexIndexOfTriangle[i].Count; triangleFirstIndex += 3)
                {
                    int vertexIndexA = meshVertexIndexOfTriangle[i][triangleFirstIndex];
                    int vertexIndexB = meshVertexIndexOfTriangle[i][triangleFirstIndex + 1];
                    int vertexIndexC = meshVertexIndexOfTriangle[i][triangleFirstIndex + 2];
                    Vector3 AWorld = transformToSample[i].TransformPoint(meshVertex[i][vertexIndexA]);
                    Vector3 BWorld = transformToSample[i].TransformPoint(meshVertex[i][vertexIndexB]);
                    Vector3 CWorld = transformToSample[i].TransformPoint(meshVertex[i][vertexIndexC]);
                    Vector3 AB = BWorld - AWorld;
                    Vector3 AC = CWorld - AWorld;

                    // Use world coordinate to compute actual area after scaling.
                    triangleAreaPdf.Add((Vector3.Cross(AB, AC).magnitude / 2));
                }

                triangleAreaCdf.Add(triangleAreaPdf[0]);
                for (int j = 1; j < triangleAreaPdf.Count; j++)
                    triangleAreaCdf.Add(triangleAreaPdf[j] + triangleAreaCdf[j - 1]);

                meshTriangleAreaPdf.Add(triangleAreaPdf);
                meshTriangleAreaCdf.Add(triangleAreaCdf);
            }

            // Compute total triangle area of all mesh.
            for(int i = 0; i < meshToSample.Count; i++)
                totalMeshTriangleArea += meshTriangleAreaCdf[i][meshTriangleAreaCdf[i].Count - 1];

            // Main generation process.
            generatedSamplePoint.Clear();
            ClearSamplePointToken();
            for (int i = 0; i < meshToSample.Count; i++)
            {
                // The larger the triangle is, the more points of the triangle will be sampled.
                float currentTotalTriangleArea = meshTriangleAreaCdf[i][meshTriangleAreaCdf[i].Count - 1];
                int currentSamplePointNumber = Mathf.FloorToInt(samplePointNumber * currentTotalTriangleArea / totalMeshTriangleArea);

                for(int j = 0; j < currentSamplePointNumber; j++)
                {
                    // Random select a point in the triangle. (u >= 0.0f, v >= 0.0f, u + v <= 1)
                    Vector3 worldPosition, worldNormal, A, B, C, AB, AC, NA, NB, NC, ANormal, BNormal, CNormal;
                    Vector2 texcoord;
                    float areaA, areaB, areaC;
                    float selectedTriangleArea = Random.value * currentTotalTriangleArea;
                    int selectedTriangle = meshTriangleAreaCdf[i].FindIndex((area) => (area >= selectedTriangleArea));
                    int selectedTriangleFirstIndex = selectedTriangle * 3;
                    float totalTriangleArea = meshTriangleAreaPdf[i][selectedTriangle];
                    int vertexIndexA = meshVertexIndexOfTriangle[i][selectedTriangleFirstIndex];
                    int vertexIndexB = meshVertexIndexOfTriangle[i][selectedTriangleFirstIndex + 1];
                    int vertexIndexC = meshVertexIndexOfTriangle[i][selectedTriangleFirstIndex + 2];
                    float u = Random.value;
                    float v = Random.value;

                    if(u + v > 1.0f)
                    {
                        u = 1.0f - u;
                        v = 1.0f - v;
                    }

                    // Compute world coordinates
                    A = transformToSample[i].TransformPoint(meshVertex[i][vertexIndexA]);
                    B = transformToSample[i].TransformPoint(meshVertex[i][vertexIndexB]);
                    C = transformToSample[i].TransformPoint(meshVertex[i][vertexIndexC]);
                    ANormal = transformToSample[i].TransformDirection(meshNormal[i][vertexIndexA]);
                    BNormal = transformToSample[i].TransformDirection(meshNormal[i][vertexIndexB]);
                    CNormal = transformToSample[i].TransformDirection(meshNormal[i][vertexIndexC]);

                    // Vertex
                    AB = B - A;
                    AC = C - A;
                    worldPosition = A + (u * AB) + (v * AC);
                   
                    // Calculate barycentric coordinates
                    NA = A - worldPosition;
                    NB = B - worldPosition;
                    NC = C - worldPosition;
                    areaA = Vector3.Cross(NB, NC).magnitude / 2.0f;
                    areaB = Vector3.Cross(NA, NC).magnitude / 2.0f;
                    areaC = Vector3.Cross(NA, NB).magnitude / 2.0f;

                    // Normal
                    worldNormal = (areaA * ANormal + areaB * BNormal + areaC * CNormal) / totalTriangleArea;
                    worldNormal = worldNormal.normalized;

                    // Texcoord
                    texcoord = (areaA * meshTexcoord[i][vertexIndexA] + areaB * meshTexcoord[i][vertexIndexB] + areaC * meshTexcoord[i][vertexIndexC]) / totalTriangleArea;

                    GenerateSingleSamplePoint(worldPosition, worldNormal, texcoord, materialToSample[i]);
                }
            }
        }

        /// <summary>
        /// Find all meshes, transforms and material from objectToSample and its children.
        /// Same index represents same set of (mesh, transform, material).
        /// </summary>
        /// <param name="objectToSample">Gameobject to be sample. (Find all meshes, transforms and material from these objects and its children)</param>
        /// <param name="meshToSample">Mesh to be sample.</param>
        /// <param name="transformToSample">Transform to be sample. (Transform space)</param>
        /// <param name="materialToSample">Material to be sample. (Get Kd)</param>
        /// <param nmae="subMeshIndex">To know which sub mesh is currently used.</param>
        void FindTargetToSample(List<GameObject> objectToSample, ref List<Mesh> meshToSample, ref List<Transform> transformToSample, ref List<Material> materialToSample, ref List<int> subMeshIndex)
        {
            // Find materials and meshes from some objects and under objects.
            for (int i = 0; i < objectToSample.Count; i++)
            {
                List<MeshFilter> filter = new List<MeshFilter>(objectToSample[i].GetComponentsInChildren<MeshFilter>());

                // For child mesh underneath.
                for (int j = 0; j < filter.Count; j++)
                {
                    Mesh tempMesh = filter[j].sharedMesh;
                    Material[] tempMaterial = filter[j].GetComponent<Renderer>().sharedMaterials;

                    // Handle one object with more than one materials.
                    for (int k = 0; k < tempMesh.subMeshCount; k++) 
                    {
                        meshToSample.Add(tempMesh);
                        transformToSample.Add(filter[j].transform);
                        materialToSample.Add(tempMaterial[k]);
                        subMeshIndex.Add(k);
                    }
                }
            }
        }

        /// <summary>
        /// Generate single sample point and add to generatedSamplePoint.
        /// </summary>
        /// <param name="worldPosition">World position of the sample point.</param>
        /// <param name="worldNormal">World normal of the sample point.</param>
        /// <param name="texCoord">Texture coordinate of the sample point.</param>
        /// <param name="meshMaterial">The material of the mesh which the sample point was generated on.</param>
        void GenerateSingleSamplePoint(Vector3 worldPosition, Vector3 worldNormal, Vector2 texCoord, Material meshMaterial)
        {
            SamplePoint samplePoint = new SamplePoint();

            samplePoint.worldPosition = worldPosition;
            samplePoint.worldNormal = worldNormal;
            samplePoint.texCoord = texCoord;
            samplePoint.meshMaterial = meshMaterial;
            samplePoint.ComputeKd();

            if (canSpawnToken)
            {
                GameObject token = Instantiate(tokenPrefab, samplePoint.worldPosition, Quaternion.identity, transform);

                token.SetActive(false);
                samplePoint.SetToken(token);
                samplePoint.SetPointRenderer(token.GetComponent<Renderer>());
            }

            generatedSamplePoint.Add(samplePoint);
        }

        /// <summary>
        /// Destroy all sample point token under SamplePointManager.
        /// </summary>
        void ClearSamplePointToken()
        {
            for (int i = transform.childCount - 1; i >= 0; --i)
               Destroy(transform.GetChild(i).gameObject);
            transform.DetachChildren();
        }

        /// <summary>
        /// Basic setup operation for camera samplers.
        /// </summary>
        void SetupCameraSampler()
        {      
            cameraSamplerCubeObject = GameObject.FindGameObjectsWithTag("CameraSamplerCube");
            cameraSamplerObject = GameObject.FindGameObjectsWithTag("CameraSampler");
            //Assert.IsTrue(cameraSamplerCubeObject.Length > 0, "There has to be at least one \"CameraSamplerCube\" in the scene!");

            for (int i = 0; i < cameraSamplerCubeObject.Length; i++)
            {
                // Horizontal
                for (int j = 0; j < 4; j++)
                {
                    GameObject obj = new GameObject("CameraSamplerHorizonal" + j.ToString());

                    obj.transform.parent = cameraSamplerCubeObject[i].transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.transform.rotation = Quaternion.Euler(0.0f, j * 90.0f, 0.0f);
                    cameraSampler.Add(obj.AddComponent<Camera>());
                    cameraSampler[i * 6 + j].enabled = false;
                    cameraSampler[i * 6 + j].fieldOfView = cameraFieldOfView;
                    cameraSampler[i * 6 + j].cullingMask = cameraCullingMask;
                }

                // Vertical
                for (int j = 0; j < 2; j++)
                {
                    GameObject obj = new GameObject("CameraSamplerVertical" + j.ToString());

                    obj.transform.parent = cameraSamplerCubeObject[i].transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.transform.rotation = Quaternion.Euler(j * 180.0f + 90.0f, 0.0f, 0.0f);
                    cameraSampler.Add(obj.AddComponent<Camera>());
                    cameraSampler[i * 6 + 4 + j].enabled = false;
                    cameraSampler[i * 6 + 4 + j].fieldOfView = cameraFieldOfView;
                    cameraSampler[i * 6 + 4 + j].cullingMask = cameraCullingMask;
                }
            }

            for(int i = 0; i < cameraSamplerObject.Length; i++)
            {
                Camera camera = cameraSamplerObject[i].GetComponent<Camera>();

                cameraSampler.Add(camera);
                camera.enabled = false;
                camera.cullingMask = cameraCullingMask;
            }

            // Initialize textures here to prevent memory explosion.
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            targetTexture = new Texture2D(renderTexture.width, renderTexture.height);
        }

        /// <summary>
        /// Render to render texture and generate a 2D texture.
        /// </summary>
        /// <param name="renderCamera">Camera to render.</param>
        /// <returns>Render texture's 2D texture.</returns>
        Texture2D GenerateRenderTexture(Camera renderCamera)
        {
            RenderTexture originActiveRenderTexture = RenderTexture.active;
            RenderTexture originCameraRenderTexture = renderCamera.targetTexture;
            renderCamera.targetTexture = renderTexture;
            renderCamera.Render();
            RenderTexture.active = renderCamera.targetTexture;
            targetTexture.ReadPixels(new Rect(0, 0, renderCamera.targetTexture.width, renderCamera.targetTexture.height), 0, 0, true);
            targetTexture.Apply(false);
            renderCamera.targetTexture = originCameraRenderTexture;
            RenderTexture.active = originActiveRenderTexture;

            return targetTexture;
        }

        /// <summary>
        /// Filter out unseen sample points.
        /// </summary>
        /// <param name="lightingGoalToFilter">The lighting goal to filter weak sample points.</param>
        /// <returns>The filtered lighting goal.</returns>
        void FilterUnseenSamplePoint()
        {
            for (int i = 0; i < outsideCamera.Length; i++)
                for (int j = 0; j < generatedSamplePoint.Count; j++)
                {
                    Vector3 worldSpacePoint = generatedSamplePoint[j].worldPosition;
                    Vector3 viewportSpacePoint = outsideCamera[i].WorldToViewportPoint(worldSpacePoint);

                    if (IsVisibleToCamera(generatedSamplePoint[j].worldPosition, outsideCamera[i]))
                        generatedSamplePoint.RemoveAt(j--);
                }
        }

        /// <summary>
        /// Test if a point is visible to a camera.
        /// </summary>
        /// <param name="worldSpacePoint">The point to test if it is visible for the camera.</param>
        /// <param name="testCamera">The camera to test the visibility.</param>
        /// <returns>Is true when the point is visible for the camera.</returns>
        bool IsVisibleToCamera(Vector3 worldSpacePoint, Camera testCamera)
        {
            Rect viewport = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            Vector3 viewportSpacePoint = testCamera.WorldToViewportPoint(worldSpacePoint);
            float maxDistance = Vector3.Distance(testCamera.transform.position, worldSpacePoint);

            if (viewport.Contains(viewportSpacePoint) && viewportSpacePoint.z >= testCamera.nearClipPlane
                && !Physics.Raycast(testCamera.transform.position, worldSpacePoint - testCamera.transform.position, maxDistance - 0.01f)) // Is visible to camera.
                return true;
            return false;
        }

        /// <summary>
        /// Test if a point is in the view frustum of the camera.
        /// </summary>
        /// <param name="worldSpacePoint">The point to test if it is visible for the camera.</param>
        /// <param name="testCamera">The camera to test the visibility.</param>
        /// <returns>Is true when the point is visible for the camera.</returns>
        public bool IsInViewFrustumOfCamera(Vector3 worldSpacePoint, Camera testCamera)
        {
            Rect viewport = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            Vector3 viewportSpacePoint = testCamera.WorldToViewportPoint(worldSpacePoint);

            if (viewport.Contains(viewportSpacePoint) && viewportSpacePoint.z >= testCamera.nearClipPlane 
                && viewportSpacePoint.z <= testCamera.farClipPlane) // Is in view frustum of Camera.
                return true;
            return false;
        }
    }
}
