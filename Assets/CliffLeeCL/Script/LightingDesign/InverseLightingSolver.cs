#define DEBUG_CL
#define COLOR_SPACE_LINEAR

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CliffLeeCL
{
    /// <summary>
    /// Difference stage of adding lights.
    /// </summary>
    public enum AddLightStage
    {
        Coarse,
        FilteredCoarse,
        Spread,
        FilteredSpread,
        ILSA,
        FilteredILSA,
        RefineWithSpotlight,
        FilteredRefineWithSpotlight,
        MAX_STAGE_NUM
    }

    /// <summary>
    /// The class unify all inverse lighting solver.
    /// </summary>
    public class InverseLightingSolver : MonoBehaviour
    {
        /// <summary>
        /// The variable is used to access this class.
        /// </summary>
        public static InverseLightingSolver instance;

        /// <summary>
        /// The variable is used to limit the range of color temperatures in SolveColorTemperatureOfLightingSetup. (according to ANSI C78.377-2008)
        /// </summary>
        public static int[] possibleColorTemperature = new int[]{2700, 3000, 3500, 4000, 4500, 5000, 5700, 6500};

        /// <summary>
        /// Is true when there are some inverse lighting process is executing. (set from outside) temporary variable for ground truth testing 
        /// </summary>
        public bool isSolvingInverseLighting = false;

        [Header("Add Light")]
        public GameObject pointLightPrefab;
        /// <summary>
        /// Current viewing add light stage.
        /// </summary>
        public AddLightStage currentAddLightStage = AddLightStage.FilteredILSA;
        /// <summary>
        /// The variable is for FilterWeakLight, the contribution lower than this variable will be filtered out.
        /// </summary>
        public float contributionThreshold = 1.0f;
        /// <summary>
        /// The variable is for RefineWithSpotlight, the difference between a sample point and a light lower than this variable will be assume as it's under spotlight.
        /// </summary>
        public float underSpotlightDifferenceThreshold = 0.6f;
        /// <summary>
        /// The variable is for RefineWithSpotlight. if the improvement of spotlight refine is higher than this variable, the refine process will continue.
        /// </summary>
        public float spotlightImprovementThreshold = 1.05f;
        /// <summary>
        /// The variable is used for isLightingGoalReached to determine whether a sample point reaches the lighting goal.
        /// </summary>
        public float differenceThreshold = 1.5f;
        /// <summary>
        /// The variable is used for isLightingGoalReached to determine whether the solved result reaches the lighting goal.
        /// </summary>
        public float reachedRatioThreshold = 0.997f;
       
        [Header("Weight")]
        /// <summary>
        /// Define the weight that used in weighted least squares for painted sample points.
        /// </summary>
        public float paintedWeight = 5.0f;
        /// <summary>
        /// Define the weight that used in weighted least squares for unpainted sample points.
        /// </summary>
        public float unpaintedWeight = 1.0f;

        [Header("Optional")]
        /// <summary>
        /// Is true when we want to consider the effect of shadows.
        /// </summary>
        public bool canCastShadow = true;
        /// <summary>
        /// Is true when we want to use global illumination to compute illumiation of sample points'.
        /// </summary>
        public bool canUseGlobalIllumination = true;
        /// <summary>
        /// Is true when we want to use least squares to approximate global lighting.
        /// </summary>
        public bool canApproximateGlobalLighting = false;
        /// <summary>
        /// Determine how many times to approximate the global lighting.
        /// </summary>
        public int maxApproximateIteration = 3;
        /// <summary>
        /// Determine how fast the approximate will go. (affect the applyed intensity) 
        /// </summary>
        public float approximateStep = 0.5f;
        
        [HideInInspector]
        /// <summary>
        /// The list stores added lights' game object for every add light stage.
        /// </summary>
        public List<List<GameObject>> addLightStageLightObj = new List<List<GameObject>>();
        [HideInInspector]
        /// <summary>
        /// The list stores added lights' parent for every add light stage.
        /// </summary>
        public List<List<Light>> addLightStageLight = new List<List<Light>>();
        [HideInInspector]
        /// <summary>
        /// The list stores added lights' light for every add light stage.
        /// </summary>
        public List<GameObject> addLightStageParentObj = new List<GameObject>();

        /// <summary>
        /// This region provides positions of coarse lights.
        /// </summary>
        PossibleLightBoundingVolume possibleLightVolume;
        /// <summary>
        /// Used to filter coarse lights.
        /// </summary>
        Vector3 unreachedAreaAveragePosition;

        // Least squares solvers
        /// <summary>
        /// Matlab non-negative least squares solver.
        /// </summary>
        static MatlabLeastSquaresSolverWrapper matlabSolver = new MatlabLeastSquaresSolverWrapper();
        /// <summary>
        /// Tsnnls solver. (non-negative least squares)
        /// </summary>
        static TsnnlsLeastSquaresSolverWrapper tsnnlsSolver = new TsnnlsLeastSquaresSolverWrapper();
        /// <summary>
        /// Math.NET solver. (linear least squares with decomposition)
        /// </summary>
        static MathDotNetLeastSquaresSolver mathDotNetSolver = new MathDotNetLeastSquaresSolver();
        /// <summary>
        /// Choose which solver to use when solving with least squares method.
        /// </summary>
        static ILeastSquaresSolver leastSquaresSolver = matlabSolver;

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
            if(instance == this)
                possibleLightVolume = GameObject.FindGameObjectWithTag("PossibleLightBoundingVolume").GetComponent<PossibleLightBoundingVolume>();
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                currentAddLightStage = (AddLightStage)(((int)currentAddLightStage - 1) % (int)AddLightStage.MAX_STAGE_NUM);
                if (currentAddLightStage < 0)
                    currentAddLightStage = (AddLightStage)((int)AddLightStage.MAX_STAGE_NUM - 1);
            }

            if (Input.GetKeyDown(KeyCode.X))
                currentAddLightStage = (AddLightStage)(((int)currentAddLightStage + 1) % (int)AddLightStage.MAX_STAGE_NUM);

            if (!isSolvingInverseLighting)
            {
                for (int i = 0; i < (int)AddLightStage.MAX_STAGE_NUM; i++)
                    if (addLightStageParentObj.Count == (int)AddLightStage.MAX_STAGE_NUM)
                    {
                        if ((int)currentAddLightStage == i)
                            addLightStageParentObj[i].SetActive(true);
                        else
                            addLightStageParentObj[i].SetActive(false);
                    }
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
        /// Save result log to a file in certain file path.
        /// </summary>
        /// <param name="filePathRelativeToAssetFolder">A file path relative to the asset folder to save the result log to.</param>
        public void SaveResultLogToFile(string filePathRelativeToAssetFolder = "/Script/LeastSquaresSolver/Result.txt")
        {
            //File.WriteAllText(Application.dataPath + filePathRelativeToAssetFolder, DateTime.Now.ToString("MM-dd HH_mm_ss") + "\n" + resultLog);
        }

        /// <summary>
        /// A inverse lighting solver using least squares, solve Id.
        /// ID = (Id * Kd * (N * L) * atten) = lighting goal
        /// Id = light intensity * light color
        /// Kd = sample point's albedo * texel color
        /// N = normal of sample point
        /// L = ComputeLightDirection()
        /// atten = ComputeLightAttenuation()
        /// </summary>
        /// <param name="lightToSolved">The lights whose intensities needs to be solved.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /// <param name="solvedLightIntensity">The callback is used to return solved intensity. A list of list that store solved intensities. (R, G, B for a light)</param>
        /// <param name="isWeighted">Is true when we want to solve weighted least squares.</param>
        /// <param name="canApproximateGlobalLighting">Is true when we want to approximate global lighting.</param>
        /// <param name="maxApproximateIteration">Define the max iteration of global lighting approximation.</param>
        /// <param name="canSaveFile">Is true when we want to save A, B, X matries to separate files.</param>
        IEnumerator SolveLightIntensityNonNegativeLeastSquares(List<Light> lightToSolved, LightingGoal lightingGoal, System.Action<List<List<double>>> solvedLightIntensity, 
            bool isWeighted = true, bool canApproximateGlobalLighting = false, int maxApproximateIteration = 3, bool canSaveFile = false)
        {
            double[] Ar = new double[lightingGoal.goalSamplePoint.Count * lightToSolved.Count];
            double[] Ag = new double[lightingGoal.goalSamplePoint.Count * lightToSolved.Count];
            double[] Ab = new double[lightingGoal.goalSamplePoint.Count * lightToSolved.Count];
            double[] Xr = new double[lightToSolved.Count];
            double[] Xg = new double[lightToSolved.Count];
            double[] Xb = new double[lightToSolved.Count];
            double[] Br = new double[lightingGoal.goalSamplePoint.Count];
            double[] Bg = new double[lightingGoal.goalSamplePoint.Count];
            double[] Bb = new double[lightingGoal.goalSamplePoint.Count];
            List<List<double>> lightIntensity = new List<List<double>>();
            Stopwatch stopWatch = new Stopwatch();
            long ABComputationTime = 0;

            stopWatch.Reset();
            stopWatch.Start();
            // Build A and B matrix for all colors.
            // Set the A matrix. !!!!(This is column major array, need to use false in SolveAxEqB's last parameter for LS and ILSA)!!!!
            for (int i = 0; i < lightToSolved.Count; i++)
            {
                if (RenderingSystem.instance.CanReuseLightContributionVector(lightToSolved[i]))
                    RenderingSystem.instance.ReuseLightContributionVector(lightToSolved[i]);
                else
                    RenderingSystem.instance.SetupReusedLightContributionVector(lightToSolved[i], lightingGoal.goalSamplePoint);

                for (int j = 0; j < lightingGoal.goalSamplePoint.Count; j++)
                {
                    Color lightingComputation = RenderingSystem.instance.reusedVector[j];
                    int offset = i * lightingGoal.goalSamplePoint.Count;

                    if (isWeighted)
                        lightingComputation *= lightingGoal.goalSamplePoint[j].Weight;
                    Ar[j + offset] = lightingComputation.r;
                    Ag[j + offset] = lightingComputation.g;
                    Ab[j + offset] = lightingComputation.b;
                }
            }

            // Set the B matrix.
            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color goalColor = lightingGoal.goalSamplePoint[i].GoalIllumination;
                
#if COLOR_SPACE_LINEAR
                goalColor = goalColor.linear;
#endif
                if (isWeighted)
                    goalColor *= lightingGoal.goalSamplePoint[i].Weight;
                Br[i] = goalColor.r;
                Bg[i] = goalColor.g;
                Bb[i] = goalColor.b;
            }
            stopWatch.Stop();
            ABComputationTime = stopWatch.ElapsedMilliseconds;

            /*no GPU version.
            stopWatch.Reset();
            stopWatch.Start();
            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color goalColor = lightingGoal.goalSamplePoint[i].GoalIllumination;
                Color Kd = lightingGoal.goalSamplePoint[i].Kd;
                Vector3 worldVertexPosition = lightingGoal.goalSamplePoint[i].worldPosition;
                Vector3 N = lightingGoal.goalSamplePoint[i].worldNormal;
                float weight = lightingGoal.goalSamplePoint[i].Weight;

                // Set the B matrix.
#if COLOR_SPACE_LINEAR
                goalColor = goalColor.linear;
#endif
                if (isWeighted)
                    goalColor *= weight;

                Br.Add(goalColor.r);
                Bg.Add(goalColor.g);
                Bb.Add(goalColor.b);

                // Set the A matrix.
                for (int j = 0; j < lightToSolved.Count; j++)
                {
                    Color lightingComputation = RenderingSystem.instance.ComputeLambertianWithoutId(worldVertexPosition,
                                                                                                        N,
                                                                                                        Kd,
                                                                                                        lightToSolved[j],
                                                                                                        canCastShadow);
                    if (isWeighted)
                        lightingComputation *= weight;
                    Ar.Add(lightingComputation.r);
                    Ag.Add(lightingComputation.g);
                    Ab.Add(lightingComputation.b);
                }
            }
            stopWatch.Stop();
            ABComputationTime = stopWatch.ElapsedMilliseconds;
            */

            stopWatch.Reset();
            stopWatch.Start();
            // Solve Ax = B
            leastSquaresSolver = matlabSolver;
            leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ar, Br, Xr, false);
            leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ag, Bg, Xg, false);
            leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ab, Bb, Xb, false);
            lightIntensity.Add(new List<double>(Xr));
            lightIntensity.Add(new List<double>(Xg));
            lightIntensity.Add(new List<double>(Xb));
            stopWatch.Stop();
#if DEBUG_CL
            print("AB computation time: " + ABComputationTime + "ms, Solve Ax = B time: " + stopWatch.ElapsedMilliseconds + "ms");
#endif

            if (canSaveFile)
            {
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/AMat-R.txt", lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ar);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/AMat-G.txt", lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ag);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/AMat-B.txt", lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ab);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/BMat-R.txt", lightingGoal.goalSamplePoint.Count, 1, Br);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/BMat-G.txt", lightingGoal.goalSamplePoint.Count, 1, Bg);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/BMat-B.txt", lightingGoal.goalSamplePoint.Count, 1, Bb);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/XMat-R.txt", lightToSolved.Count, 1, Xr);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/XMat-G.txt", lightToSolved.Count, 1, Xg);
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/XMat-B.txt", lightToSolved.Count, 1, Xb);
            }

            if (canApproximateGlobalLighting)
            {
                double[] dGr = new double[lightingGoal.goalSamplePoint.Count];
                double[] dGg = new double[lightingGoal.goalSamplePoint.Count];
                double[] dGb = new double[lightingGoal.goalSamplePoint.Count];
                List<Color> originLightIntensity = new List<Color>();

                for (int i = 0; i < lightToSolved.Count; i++)
                    originLightIntensity.Add(lightToSolved[i].color * lightToSolved[i].intensity);

                stopWatch.Reset();
                stopWatch.Start();
                for (int iter = 0; iter < maxApproximateIteration; iter++)
                {  
                    yield return StartCoroutine(ApplyInverseLightingResult(lightToSolved, lightIntensity, true));
                    yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(lightToSolved,
                                                lightingGoal.goalSamplePoint, false, canCastShadow, true));
#if DEBUG_CL
                    if (iter == 0)
                    {
                        for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
                            lightingGoal.goalSamplePoint[i].SetTokenVisibility(false);
                        print("LS difference: " + ComputeNormalizedIlluminationDifferenceOfLightingGoal(lightingGoal));
                    }
#endif

                    // Set dg
                    for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
                    {
                        Color currentIllumination = lightingGoal.goalSamplePoint[i].currentIllumination;
                        Color goalIllumination = lightingGoal.goalSamplePoint[i].GoalIllumination;
                        float weight = lightingGoal.goalSamplePoint[i].Weight;

#if COLOR_SPACE_LINEAR
                        currentIllumination = currentIllumination.linear;
                        goalIllumination = goalIllumination.linear;
#endif

                        if (isWeighted)
                        {
                            dGr[i] = (currentIllumination.r - goalIllumination.r) * weight;
                            dGg[i] = (currentIllumination.g - goalIllumination.g) * weight;
                            dGb[i] = (currentIllumination.b - goalIllumination.b) * weight;
                        }
                        else
                        {
                            dGr[i] = currentIllumination.r - goalIllumination.r;
                            dGg[i] = currentIllumination.g - goalIllumination.g;
                            dGb[i] = currentIllumination.b - goalIllumination.b;
                        }
                    }

                    // Solve Adx = dg
                    leastSquaresSolver = mathDotNetSolver;
                    leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ar, dGr, Xr, false);
                    leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ag, dGg, Xg, false);
                    leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, Ab, dGb, Xb, false);

                    for (int i = 0; i < lightToSolved.Count; i++)
                    {
                        lightIntensity[0][i] = lightIntensity[0][i] - Xr[i] * approximateStep;
                        lightIntensity[1][i] = lightIntensity[1][i] - Xg[i] * approximateStep;
                        lightIntensity[2][i] = lightIntensity[2][i] - Xb[i] * approximateStep;

                        if (lightIntensity[0][i] < 0)
                            lightIntensity[0][i] = 0.0f;
                        if (lightIntensity[1][i] < 0)
                            lightIntensity[1][i] = 0.0f;
                        if (lightIntensity[2][i] < 0)
                            lightIntensity[2][i] = 0.0f;
                    }
                }
                stopWatch.Stop();

#if DEBUG_CL
                // Difference
                for (int i = 0; i < lightToSolved.Count; i++)
                {
                    Color solvedIntensity = Color.white;
                    Color colorDifference;

                    solvedIntensity.r = (float)lightIntensity[0][i];
                    solvedIntensity.g = (float)lightIntensity[1][i];
                    solvedIntensity.b = (float)lightIntensity[2][i];
#if COLOR_SPACE_LINEAR
                    solvedIntensity = solvedIntensity.gamma;
#endif
                    colorDifference = solvedIntensity - originLightIntensity[i];
                    print("light " + i.ToString() + ": (" 
                        + (colorDifference.r / (originLightIntensity[i].r + Mathf.Epsilon)) * 100 + "%, " 
                        + (colorDifference.g / (originLightIntensity[i].g + Mathf.Epsilon)) * 100 + "%, "
                        + (colorDifference.b / (originLightIntensity[i].b + Mathf.Epsilon)) * 100 + "%)" );
            }
                print("Solve Adx = dg time: " + stopWatch.ElapsedMilliseconds + "ms");
#endif
            }

            solvedLightIntensity(lightIntensity);
        }

        /// <summary>
        /// A inverse lighting solver using least squares, solve light intensity only. (light color is fixed)
        /// ID = (Id * Kd * (N * L) * atten) = lighting goal
        /// Id = light intensity * light color
        /// Kd = sample point's albedo * texel color
        /// N = normal of sample point
        /// L = ComputeLightDirection()
        /// atten = ComputeLightAttenuation()
        /// </summary>
        /// <param name="lightToSolved">The lights whose intensities needs to be solved.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /// <param name="solvedLightIntensity">The callback is used to return solved intensity. A list of list that store solved intensities. (R, G, B for a light)</param>
        /// <param name="isWeighted">Is true when we want to solve weighted least squares.</param>
        ///         /// <param name="canApproximateGlobalLighting">Is true when we want to approximate global lighting.</param>
        /// <param name="maxApproximateIteration">Define the max iteration of global lighting approximation.</param>
        /// <param name="canSaveFile">Is true when we want to save A, B, X matries to separate files.</param>
        /// <returns>A list of list that store solved intensities. (R, G, B for a light)</returns>
        IEnumerator SolveLightIntensityFixedColorNonNegativeLeastSquares(List<Light> lightToSolved, LightingGoal lightingGoal, System.Action<List<List<double>>> solvedLightIntensity, 
            bool isWeighted = true, bool canApproximateGlobalLighting = false, int maxApproximateIteration = 3, bool canSaveFile = false)
        {
            List<double> A = new List<double>();
            List<double> Ar = new List<double>();
            List<double> Ag = new List<double>();
            List<double> Ab = new List<double>();
            List<double> X = new List<double>();
            List<double> B = new List<double>();
            List<double> Br = new List<double>();
            List<double> Bg = new List<double>();
            List<double> Bb = new List<double>();
            List<List<double>> lightIntensity = new List<List<double>>();
            double[] XArray = new double[lightToSolved.Count];
            Stopwatch stopWatch = new Stopwatch();
            long ABComputationTime = 0;

            stopWatch.Reset();
            stopWatch.Start();
            // Build A and B matrix for all colors.
            // Set the A matrix. !!!!(This is column major array, need to use "false" in SolveAxEqB's last parameter for LS and ILSA)!!!!
            for (int i = 0; i < lightToSolved.Count; i++)
            {
                if (RenderingSystem.instance.CanReuseLightContributionVector(lightToSolved[i]))
                    RenderingSystem.instance.ReuseLightContributionVector(lightToSolved[i]);
                else
                    RenderingSystem.instance.SetupReusedLightContributionVector(lightToSolved[i], lightingGoal.goalSamplePoint);

                for (int j = 0; j < lightingGoal.goalSamplePoint.Count; j++)
                {
                    Color lightingComputation = RenderingSystem.instance.reusedVector[j];

                    if (isWeighted)
                        lightingComputation *= lightingGoal.goalSamplePoint[j].Weight;
                    Ar.Add(lightingComputation.r);
                    Ag.Add(lightingComputation.g);
                    Ab.Add(lightingComputation.b);
                }
            }

            // Set the B matrix.
            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color goalColor = lightingGoal.goalSamplePoint[i].GoalIllumination;

#if COLOR_SPACE_LINEAR
                goalColor = goalColor.linear;
#endif
                if (isWeighted)
                    goalColor *= lightingGoal.goalSamplePoint[i].Weight;
                Br.Add(goalColor.r);
                Bg.Add(goalColor.g);
                Bb.Add(goalColor.b);
            }
            B.AddRange(Br);
            B.AddRange(Bg);
            B.AddRange(Bb);
            A.AddRange(Ar);
            A.AddRange(Ag);
            A.AddRange(Ab);
            stopWatch.Stop();
            ABComputationTime = stopWatch.ElapsedMilliseconds;

            /* no GPU version.
            stopWatch.Reset();
            stopWatch.Start();
            // Build A and B matrix for all colors.
            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color goalColor = lightingGoal.goalSamplePoint[i].GoalIllumination;
                Color Kd = lightingGoal.goalSamplePoint[i].Kd;
                Vector3 worldVertexPosition = lightingGoal.goalSamplePoint[i].worldPosition;
                Vector3 N = lightingGoal.goalSamplePoint[i].worldNormal;
                float weight = lightingGoal.goalSamplePoint[i].Weight;

                // Set B matrix.
#if COLOR_SPACE_LINEAR
                goalColor = goalColor.linear;
#endif

                if (isWeighted)
                    goalColor *= weight;

                Br.Add(goalColor.r);
                Bg.Add(goalColor.g);
                Bb.Add(goalColor.b);

                // Set the A matrix.
                for (int j = 0; j < lightToSolved.Count; j++)
                {
                    Color lightingComputation = RenderingSystem.instance.ComputeLambertianWithoutId(worldVertexPosition,
                                                                                                        N,
                                                                                                        Kd,
                                                                                                        lightToSolved[j],
                                                                                                        canCastShadow);

                    lightingComputation *= lightToSolved[j].color.linear;
                    if (isWeighted)
                        lightingComputation *= weight;
                    Ar.Add(lightingComputation.r);
                    Ag.Add(lightingComputation.g);
                    Ab.Add(lightingComputation.b);
                }
            }
            B.AddRange(Br);
            B.AddRange(Bg);
            B.AddRange(Bb);
            A.AddRange(Ar);
            A.AddRange(Ag);
            A.AddRange(Ab);
            stopWatch.Stop();
            ABComputationTime = stopWatch.ElapsedMilliseconds;
            */

            stopWatch.Reset();
            stopWatch.Start();
            // Solve Ax = B
            leastSquaresSolver = matlabSolver;
            leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, A.ToArray(), B.ToArray(), XArray, false);
            X = new List<double>(XArray);
            lightIntensity.Add(X);
            stopWatch.Stop();
#if DEBUG_CL
            print("AB computation time: " + ABComputationTime + ", Solve Ax = B time: " + stopWatch.ElapsedMilliseconds);
#endif

            if (canSaveFile)
            {
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/AMat-R.txt", lightingGoal.goalSamplePoint.Count, lightToSolved.Count, A.ToArray());
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/BMat-R.txt", lightingGoal.goalSamplePoint.Count, 1, B.ToArray());
                FileUtility.WriteArray(Application.dataPath + "/Script/LeastSquaresSolver/XMat-R.txt", lightToSolved.Count, 1, X.ToArray());
            }

            if (canApproximateGlobalLighting)
            {
                List<float> originLightIntensity = new List<float>();

                for (int i = 0; i < lightToSolved.Count; i++)
                    originLightIntensity.Add(lightToSolved[i].intensity);

                stopWatch.Reset();
                stopWatch.Start();
                for (int iter = 0; iter < maxApproximateIteration; iter++)
                {
                    List<double> dGr = new List<double>(Br);
                    List<double> dGg = new List<double>(Bg);
                    List<double> dGb = new List<double>(Bb);
                    List<double> dG = new List<double>();

                    yield return StartCoroutine(ApplyInverseLightingResult(lightToSolved, lightIntensity, false));
                    yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(lightToSolved,
                                                lightingGoal.goalSamplePoint, false, canCastShadow, true));

                    // Set dg
                    for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
                    {
                        Color currentIllumination = lightingGoal.goalSamplePoint[i].currentIllumination;
                        Color goalIllumination = lightingGoal.goalSamplePoint[i].GoalIllumination;
                        float weight = lightingGoal.goalSamplePoint[i].Weight;

#if COLOR_SPACE_LINEAR
                        currentIllumination = currentIllumination.linear;
                        goalIllumination = goalIllumination.linear;
#endif

                        if (isWeighted)
                        {
                            dGr[i] = (currentIllumination.r - goalIllumination.r) * weight;
                            dGg[i] = (currentIllumination.g - goalIllumination.g) * weight;
                            dGb[i] = (currentIllumination.b - goalIllumination.b) * weight;
                        }
                        else
                        {
                            dGr[i] = currentIllumination.r - Br[i];
                            dGg[i] = currentIllumination.g - Bg[i];
                            dGb[i] = currentIllumination.b - Bb[i];
                        }
                    }

                    dG.AddRange(dGr);
                    dG.AddRange(dGg);
                    dG.AddRange(dGb);

                    // Solve Adx = dg
                    leastSquaresSolver = mathDotNetSolver;
                    leastSquaresSolver.SolveAxEqB(lightingGoal.goalSamplePoint.Count, lightToSolved.Count, A.ToArray(), dG.ToArray(), XArray, false);
                    X = new List<double>(XArray);

                    for (int i = 0; i < lightToSolved.Count; i++)
                    {
                        lightIntensity[0][i] = lightIntensity[0][i] - X[i];

                        if (lightIntensity[0][i] < 0)
                            lightIntensity[0][i] = 0.0f;
                    }
                }
                stopWatch.Stop();

#if DEBUG_CL
                // Difference
                for (int i = 0; i < lightToSolved.Count; i++)
                {
                    float solvedIntensity = (float)lightIntensity[0][i];

#if COLOR_SPACE_LINEAR
                    solvedIntensity = ColorSpaceUtility.LinearToGamma(solvedIntensity);
#endif
                    print("light " + i.ToString() + " " + ((solvedIntensity - originLightIntensity[i]) / (originLightIntensity[i] + Mathf.Epsilon)) * 100 + "%");
                }
                print("Solve Adx = dg time: " + stopWatch.ElapsedMilliseconds + "ms");
#endif
            }

            solvedLightIntensity(lightIntensity);
        }

        /// <summary>
        /// Solve lightingSetup's intensity with lightingGoal and apply to lightingSetup.
        /// </summary>
        /// <param name="lightingSetupToSolve">The lights whose intensities needs to be solved.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /// <param name="canAdjustLightColor">Is true when we can change light's color.</param>
        public IEnumerator SolveLightIntensityOfLightingSetup(List<Light> lightingSetupToSolve, LightingGoal lightingGoal, bool canAdjustLightColor, bool canApproximateGlobalLighting)
        {
            Stopwatch watch = new Stopwatch();
            List<List<double>> solvedLightIntensities = new List<List<double>>();

            if (lightingSetupToSolve.Count == 0)
                yield break;

            watch.Reset();
            watch.Start();
            if (canAdjustLightColor)
                yield return StartCoroutine(SolveLightIntensityNonNegativeLeastSquares(lightingSetupToSolve, lightingGoal, 
                    value => solvedLightIntensities = value, true, canApproximateGlobalLighting, maxApproximateIteration));
            else
                yield return StartCoroutine(SolveLightIntensityFixedColorNonNegativeLeastSquares(lightingSetupToSolve, lightingGoal,
                   value => solvedLightIntensities = value, true, canApproximateGlobalLighting, maxApproximateIteration));
            watch.Stop();

            print("---- Existing lights solving time: " + watch.ElapsedMilliseconds + "ms");

            yield return StartCoroutine(ApplyInverseLightingResult(lightingSetupToSolve, solvedLightIntensities, canAdjustLightColor));
        }

        /// <summary>
        /// Solve lightingSetup's lumen with lightingGoal and apply to lightingSetup.
        /// </summary>
        /// <param name="lightingSetupToSolve">The lights whose intensities needs to be solved.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /// <param name="canAdjustLightColor">Is true when we can change light's color.</param>
        /*public IEnumerator SolveLightLumenOfLightingSetup(List<Light> lightingSetupToSolve, LightingGoal lightingGoal, bool canAdjustLightColor)
        {
            int iterationCount = 0;
            Stopwatch watch = new Stopwatch();
            List<LumenLight> lightingSetupLumen = new List<LumenLight>();
            List<List<double>> solvedLightIntensities = new List<List<double>>();

            watch.Reset();
            watch.Start();

            for (int i = 0; i < lightingSetupToSolve.Count; i++)
                lightingSetupLumen.Add(lightingSetupToSolve[i].GetComponent<LumenLight>());

            do {
                iterationCount++;
                if (canAdjustLightColor)
                    yield return StartCoroutine(SolveLightIntensityNonNegativeLeastSquares(lightingSetupToSolve, lightingGoal,
                        value => solvedLightIntensities = value, true, canApproximateGlobalLighting, maxApproximateIteration));
                else
                    yield return StartCoroutine(SolveLightIntensityFixedColorNonNegativeLeastSquares(lightingSetupToSolve, lightingGoal,
                        value => solvedLightIntensities = value, true, canApproximateGlobalLighting, maxApproximateIteration));
                yield return StartCoroutine(ApplyInverseLightingResult(lightingSetupToSolve, solvedLightIntensities, canAdjustLightColor));
                UpdateLumenWithLightIntensity(lightingSetupLumen);
#if DEBUG_CL
                print("Iteration " + iterationCount);
#endif
            } while (!DoesAllLumenMatchLightRange(lightingSetupToSolve, lightingSetupLumen, 0.1f));

            for (int i = 0; i < lightingSetupToSolve.Count; i++)
                if (lightingSetupLumen[i].lumens < 0)
                {
                    lightingSetupToSolve[i].color = Color.black;
                    lightingSetupToSolve[i].range = 0.0f;
                    continue;
                }

            watch.Stop();
#if DEBUG_CL
            print("Lumen computation time: " + watch.ElapsedMilliseconds + "ms");
            print("Iteration count: " + iterationCount);
#endif     
        }*/

        /// <summary>
        /// Solve lightingSetup's color temperature with lightingGoal and apply to lightingSetup.
        /// </summary>
        /// <param name="lightingSetupToSolve">The lights whose intensities needs to be solved.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        public void SolveColorTemperatureOfLightingSetup(List<Light> lightingSetupToSolve, LightingGoal lightingGoal)
        {
            //List<LumenLight> lightingSetupLumen = new List<LumenLight>();
            List<float> originIntensity = new List<float>();
            List<int> colorTemperature = new List<int>();
            List<int> minDiffColorTemperature = new List<int>();
            float minDiff = Mathf.Infinity;

            for (int i = 0; i < lightingSetupToSolve.Count; i++)
            {
                //lightingSetupLumen.Add(lightingSetupToSolve[i].GetComponent<LumenLight>());
                originIntensity.Add(lightingSetupToSolve[i].intensity);
                colorTemperature.Add(0);
                minDiffColorTemperature.Add(0);
            }

            // This function needs to change to coroutine to work.
            /*yield return BruteForceSearchColorTemperature(0, colorTemperature, lightingSetupToSolve, 
                lightingSetupLumen, lightingGoal, ref minDiff, ref minDiffColorTemperature);*/
/*
            for (int i = 0; i < lightingSetupLumen.Count; i++)
            {
                //lightingSetupLumen[i].SetLightColorWithColorTemperature(minDiffColorTemperature[i]);
                lightingSetupToSolve[i].intensity = originIntensity[i];
            }*/
        }

        /// <summary>
        /// The fucntion is used recursively to do the brute-force search of all possible color temperatures.
        /// </summary>
        /// <param name="lightIndex">Current light's index.</param>
        /// <param name="colorTemperature">Current color temperatures of all lights.</param>
        /// <param name="lightingSetupToSolve">The lights whose intensities needs to be solved.</param>
        /// <param name="lightingSetupLumen">The LumenLight scirpt on all lights.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /// <param name="minDiff">Saves the min difference information.</param>
        /// <param name="minDiffColorTemperature">Saves the color temperatures of all lights when the min difference showed up.</param>
        /*void BruteForceSearchColorTemperature(int lightIndex, List<int> colorTemperature, List<Light> lightingSetupToSolve, List<LumenLight> lightingSetupLumen, LightingGoal lightingGoal, ref float minDiff, ref List<int> minDiffColorTemperature)
        {
            if(lightIndex == lightingSetupToSolve.Count)
            {
                float currentDiff = 0.0f;

                SolveLightIntensityOfLightingSetup(lightingSetupToSolve, lightingGoal, false, false);
                currentDiff = ComputeNormalizedIlluminationDifferenceOfLightingGoal(lightingGoal);
                if (minDiff > currentDiff)
                {
                    minDiff = currentDiff;
                    for(int i = 0; i < minDiffColorTemperature.Count; i++)
                        minDiffColorTemperature[i] = colorTemperature[i];
                }
                return;
            }

            for(int i = 0; i < possibleColorTemperature.Length; i++)
            {
                colorTemperature[lightIndex] = possibleColorTemperature[i];
                lightingSetupLumen[lightIndex].SetLightColorWithColorTemperature(colorTemperature[lightIndex]);
                BruteForceSearchColorTemperature(lightIndex + 1, colorTemperature, lightingSetupToSolve, 
                    lightingSetupLumen, lightingGoal, ref minDiff, ref minDiffColorTemperature);
            }
        }*/

        /// <summary>
        /// Filter out weak sample points. (current illumination lower than a threshold)
        /// </summary>
        /// <param name="lightingGoalToFilter">The lighting goal to filter weak sample points.</param>
        /// <returns>The filtered lighting goal.</returns>
        public LightingGoal FilterWeakSamplePoint(LightingGoal lightingGoalToFilter)
        {
            LightingGoal filteredLightingGoal = new LightingGoal();

            for (int i = 0; i < lightingGoalToFilter.goalSamplePoint.Count; i++)
            {
                if (canUseGlobalIllumination)
                {
                    if (lightingGoalToFilter.goalSamplePoint[i].currentIllumination.grayscale > 0.05f || lightingGoalToFilter.goalSamplePoint[i].isPainted())
                        filteredLightingGoal.goalSamplePoint.Add(lightingGoalToFilter.goalSamplePoint[i]);
                }
                else
                {
                    if(lightingGoalToFilter.goalSamplePoint[i].currentIllumination.grayscale - RenderingSystem.instance.ComputeAmbient(lightingGoalToFilter.goalSamplePoint[i].Kd).gamma.grayscale > 0.05f 
                        || lightingGoalToFilter.goalSamplePoint[i].isPainted())
                        filteredLightingGoal.goalSamplePoint.Add(lightingGoalToFilter.goalSamplePoint[i]);
                }
            }

            return filteredLightingGoal;
        }

        /// <summary>
        /// Filter out all directional lights for inverse lighting.
        /// </summary>
        /// <param name="lightToFilter">Light's list to filter.</param>
        /// <returns>Filtered lights with no directional lights.</returns>
        public List<Light> FilterDirectionalLight(List<Light> lightToFilter)
        {
            List<Light> lightWithoutDirectionalLight = new List<Light>(lightToFilter);

            for (int i = 0; i < lightWithoutDirectionalLight.Count; i++)
                if (lightWithoutDirectionalLight[i].type == LightType.Directional)
                    lightWithoutDirectionalLight.RemoveAt(i--);

            return lightWithoutDirectionalLight;
        }

        /// <summary>
        /// Solve a spotlight's orientation and spot angle for some sample points.
        /// </summary>
        /// <param name="litSamplePoint">The sample point that should be under the spotlight.</param>
        /// <param name="spotlight">The spotlight to be solved.</param>
        public void SolveSpotlightOrientationAndSpotAngle(List<SamplePoint> litSamplePoint, Light spotlight)
        {
            if (spotlight.type == LightType.Spot)
            {
                Vector3 lightOrientation = Vector3.zero;
                float minCosTheta = 1.0f;

                // Orientation.
                for (int j = 0; j < litSamplePoint.Count; j++)
                    lightOrientation += (litSamplePoint[j].worldPosition - spotlight.transform.position);
                spotlight.transform.forward = lightOrientation.normalized;

                // Spot angle.
                for (int j = 0; j < litSamplePoint.Count; j++)
                {
                    Vector3 lightToPointDirection = (litSamplePoint[j].worldPosition - spotlight.transform.position).normalized;
                    float cosTheta = Vector3.Dot(lightToPointDirection, spotlight.transform.forward);

                    minCosTheta = Mathf.Min(minCosTheta, cosTheta);
                }
                spotlight.spotAngle = Mathf.Acos(minCosTheta) * Mathf.Rad2Deg * 2;
            }
        }

        // This function needs to be rewritten.
        /// <summary>
        /// Select a light in lighting setup to replace it with a spotlight to fitting the lighting goal.
        /// </summary>
        /// <param name="lightingSetup">The lighting setup to search proper light to replace.</param>
        /// <param name="lightingGoal">The goal to fit with spotlight.</param>
        /// <param name="nearestLightNumToCheck">The number of nearest lights to search for spotlight replacement.</param>
        /// <param name="isPointLightOnly">Is true if only the point light can be replaced with a spotlight.</param>
        /*public IEnumerator SolveSpotlightReplacement(List<Light> lightingSetup, LightingGoal lightingGoal, int nearestLightNumToCheck, bool isPointLightOnly)
        {
            List<Light> lightingSetupToCompute = new List<Light>();
            List<SamplePoint> paintedSamplePoint = new List<SamplePoint>();
            List<float> lightToPaintedPointDistanceList = new List<float>();
            List<float> closestLightDiff = new List<float>();
            List<int> closestLightIndex = new List<int>();
            List<Vector3> solvedOrientation = new List<Vector3>();
            List<Color> solvedColor = new List<Color>();
            List<float> solvedSpotAngle = new List<float>();
            List<float> solvedIntensity = new List<float>();
            Color minDiffColor = Color.black;
            Vector3 minDiffOrientation = Vector3.zero;
            float minDiffSpotAngle = 0.0f;
            float minDiffIntensity = 0.0f;
            float minDiff = Mathf.Infinity;
            int minDiffIndex = 0;

            for (int i = 0; i < lightingSetup.Count; i++)
            {
                if (isPointLightOnly && lightingSetup[i].type == LightType.Spot)
                    continue;
                lightingSetupToCompute.Add(lightingSetup[i]);
            }
            nearestLightNumToCheck = (lightingSetupToCompute.Count < nearestLightNumToCheck) ? lightingSetupToCompute.Count : nearestLightNumToCheck;

            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
                if (lightingGoal.goalSamplePoint[i].isPainted() )
                    paintedSamplePoint.Add(lightingGoal.goalSamplePoint[i]);

            // Compute distance from light to painted sample points and sort index by distance.
            for (int i = 0; i < lightingSetupToCompute.Count; i++)
            {
                float lightToPaintedPointDistance = 0.0f;

                for (int j = 0; j < paintedSamplePoint.Count; j++)
                    lightToPaintedPointDistance += Vector3.Distance(lightingSetupToCompute[i].transform.position, paintedSamplePoint[j].worldPosition);
                lightToPaintedPointDistanceList.Add(lightToPaintedPointDistance);
                closestLightIndex.Add(i);
            }
            closestLightIndex.Sort((a, b) => lightToPaintedPointDistanceList[a].CompareTo(lightToPaintedPointDistanceList[b]));

            // Find first X point lights to try to change to spotlight.
            for (int i = 0; i < nearestLightNumToCheck; i++)
            {
                List<Light> existingLight = new List<Light>(FindObjectsOfType<Light>());
                List<Light> existingLightWithoutDirectionalLight = FilterDirectionalLight(existingLight);
                Light lightToReplace = lightingSetupToCompute[closestLightIndex[i]];
                LightType originType = lightToReplace.type;
                Color originColor = lightToReplace.color;
                Vector3 originOrientation = lightToReplace.transform.forward;
                float originSpotAngle = lightToReplace.spotAngle;
                float originIntensity = lightToReplace.intensity;

                lightToReplace.type = LightType.Spot;
                lightToReplace.intensity = 0.0f;
                SolveSpotlightOrientationAndSpotAngle(paintedSamplePoint, lightToReplace);
                yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, 
                                                lightingGoal.goalSamplePoint, true, canCastShadow, canUseGlobalIllumination));
                yield return SolveLightIntensityOfLightingSetup(lightingSetupToCompute.GetRange(closestLightIndex[i], 1), lightingGoal, true);
                yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight,
                                                lightingGoal.goalSamplePoint, true, canCastShadow, canUseGlobalIllumination));
                solvedColor.Add(lightToReplace.color);
                solvedOrientation.Add(lightToReplace.transform.forward);
                solvedSpotAngle.Add(lightToReplace.spotAngle);
                solvedIntensity.Add(lightToReplace.intensity);

                lightToReplace.type = originType;
                lightToReplace.color = originColor;
                lightToReplace.transform.forward = originOrientation;
                lightToReplace.spotAngle = originSpotAngle;
                lightToReplace.intensity = originIntensity;

                // Compute goal illumination as origin lights and compute difference.
                yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, 
                                                lightingGoal.goalSamplePoint, true, canCastShadow, canUseGlobalIllumination));
                lightToReplace.type = LightType.Spot;
                lightToReplace.color = solvedColor[i];
                lightToReplace.transform.forward = solvedOrientation[i];
                lightToReplace.spotAngle = solvedSpotAngle[i];
                lightToReplace.intensity = solvedIntensity[i];
                closestLightDiff.Add(ComputeDifferenceBetweenLightingGoalAndLightingSetup(lightingGoal, lightingSetup));

                lightToReplace.type = originType;
                lightToReplace.color = originColor;
                lightToReplace.transform.forward = originOrientation;
                lightToReplace.spotAngle = originSpotAngle;
                lightToReplace.intensity = originIntensity;
            }

            // Find min difference and save its properties.
            for (int i = 0; i < closestLightDiff.Count; i++)
            {
                print(lightingSetupToCompute[closestLightIndex[i]].name + " Diff: " + closestLightDiff[i]);

                // Find min difference light and save its properties.
                if (closestLightDiff[i] < minDiff)
                {
                    minDiff = closestLightDiff[i];
                    minDiffIndex = closestLightIndex[i];

                    minDiffColor = solvedColor[i];
                    minDiffOrientation = solvedOrientation[i];
                    minDiffSpotAngle = solvedSpotAngle[i];
                    minDiffIntensity = solvedIntensity[i];
                }
            }

            // Replace the min difference light with spotlight 
            if (minDiffIntensity > 0.0f)
            {
                lightingSetupToCompute[minDiffIndex].type = LightType.Spot;
                lightingSetupToCompute[minDiffIndex].color = minDiffColor;
                lightingSetupToCompute[minDiffIndex].transform.forward = minDiffOrientation;
                lightingSetupToCompute[minDiffIndex].spotAngle = minDiffSpotAngle;
                lightingSetupToCompute[minDiffIndex].intensity = minDiffIntensity;
            }
        }*/

        /// <summary>
        /// Coarse-to-fine strategy to add lights. (Similar to Lin et al., 2013)
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /// <param name="canReserveAddedLight">Is true when we want to save added light after restarting adding light process.</param>
        /// <param name="reservedStageLight">The stage of light that will be reserved.</param>
        public IEnumerator CoarseToFine(GameObject lightPrefab, LightingGoal lightingGoal, bool canReserveAddedLight, AddLightStage reservedStageLight = AddLightStage.FilteredILSA)
        {
            List<Light> existingLight = new List<Light>();
            List<Light> existingLightWithoutDirectionalLight = new List<Light>();
            Stopwatch stageWatch = new Stopwatch();
            Stopwatch totalWatch = new Stopwatch();

            ResetAddLightStageList(canReserveAddedLight, reservedStageLight);

            addLightStageParentObj[(int)AddLightStage.Coarse].SetActive(true);
            totalWatch.Reset();
            totalWatch.Start();
            stageWatch.Reset();
            stageWatch.Start();
            AddCoarseLight(lightPrefab, possibleLightVolume.possibleCoarseLightPosition, addLightStageLightObj[(int)AddLightStage.Coarse],
                            addLightStageParentObj[(int)AddLightStage.Coarse].transform, addLightStageLight[(int)AddLightStage.Coarse]);
            existingLight = new List<Light>(FindObjectsOfType<Light>());
            existingLightWithoutDirectionalLight = FilterDirectionalLight(existingLight);
            yield return StartCoroutine(SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, true, false));
            stageWatch.Stop();
#if DEBUG_CL
            print("Add and solve coarse light time: " + stageWatch.ElapsedMilliseconds + "ms");
#endif
            addLightStageParentObj[(int)AddLightStage.Coarse].SetActive(false);

            addLightStageParentObj[(int)AddLightStage.FilteredCoarse].SetActive(true);
            stageWatch.Reset();
            stageWatch.Start();
            yield return StartCoroutine(FilterWeakLight(lightPrefab, lightingGoal, addLightStageLight[(int)AddLightStage.Coarse], contributionThreshold, addLightStageLightObj[(int)AddLightStage.FilteredCoarse],
                            addLightStageParentObj[(int)AddLightStage.FilteredCoarse].transform, addLightStageLight[(int)AddLightStage.FilteredCoarse]));
            // Not necessary. (only useful for debug)
            //existingLight = new List<Light>(FindObjectsOfType<Light>());
            //existingLightWithoutDirectionalLight = FilterDirectionalLight(existingLight);
            //yield return SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, true);
            stageWatch.Stop();
#if DEBUG_CL
            print("Filter weak coarse light time: " + stageWatch.ElapsedMilliseconds + "ms");
#endif
            addLightStageParentObj[(int)AddLightStage.FilteredCoarse].SetActive(false);

            addLightStageParentObj[(int)AddLightStage.Spread].SetActive(true);
            stageWatch.Reset();
            stageWatch.Start();
            SpreadLight(lightPrefab, addLightStageLightObj[(int)AddLightStage.FilteredCoarse], addLightStageLightObj[(int)AddLightStage.Spread],
                        addLightStageParentObj[(int)AddLightStage.Spread].transform, addLightStageLight[(int)AddLightStage.Spread]);
            existingLight = new List<Light>(FindObjectsOfType<Light>());
            existingLightWithoutDirectionalLight = FilterDirectionalLight(existingLight);
            yield return StartCoroutine(SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, true, false));
            stageWatch.Stop();
#if DEBUG_CL
            print("Spread light and solve time: " + stageWatch.ElapsedMilliseconds + "ms");
#endif
            addLightStageParentObj[(int)AddLightStage.Spread].SetActive(false);

            addLightStageParentObj[(int)AddLightStage.FilteredSpread].SetActive(true);
            stageWatch.Reset();
            stageWatch.Start();
            yield return StartCoroutine(FilterWeakLight(lightPrefab, lightingGoal, addLightStageLight[(int)AddLightStage.Spread], contributionThreshold, addLightStageLightObj[(int)AddLightStage.FilteredSpread],
                            addLightStageParentObj[(int)AddLightStage.FilteredSpread].transform, addLightStageLight[(int)AddLightStage.FilteredSpread]));
            stageWatch.Stop();
#if DEBUG_CL
            print("Filter weak spread light time: " + stageWatch.ElapsedMilliseconds + "ms");
#endif
            addLightStageParentObj[(int)AddLightStage.FilteredSpread].SetActive(false);
            for (int i = 0; i < addLightStageLight[(int)AddLightStage.FilteredSpread].Count; i++)
                AddLight(lightPrefab, addLightStageLight[(int)AddLightStage.FilteredSpread][i], addLightStageLightObj[(int)AddLightStage.ILSA],
                    addLightStageParentObj[(int)AddLightStage.ILSA].transform, addLightStageLight[(int)AddLightStage.ILSA]);

            addLightStageParentObj[(int)AddLightStage.ILSA].SetActive(true);
            stageWatch.Reset();
            stageWatch.Start();
            existingLight = new List<Light>(FindObjectsOfType<Light>());
            existingLightWithoutDirectionalLight = FilterDirectionalLight(existingLight);
            yield return StartCoroutine(SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, true, canApproximateGlobalLighting));
            stageWatch.Stop();
#if DEBUG_CL
            print("Solve lights with ILSA time: " + stageWatch.ElapsedMilliseconds + "ms");
#endif
            addLightStageParentObj[(int)AddLightStage.ILSA].SetActive(false);

            addLightStageParentObj[(int)AddLightStage.FilteredILSA].SetActive(true);
            stageWatch.Reset();
            stageWatch.Start();
            yield return StartCoroutine(FilterWeakLight(lightPrefab, lightingGoal, addLightStageLight[(int)AddLightStage.ILSA], contributionThreshold, addLightStageLightObj[(int)AddLightStage.FilteredILSA],
                            addLightStageParentObj[(int)AddLightStage.FilteredILSA].transform, addLightStageLight[(int)AddLightStage.FilteredILSA]));
            stageWatch.Stop();

            totalWatch.Stop();
#if DEBUG_CL
            print("Filter weak ILSA lights time: " + stageWatch.ElapsedMilliseconds + "ms");
#endif
            print("---- Coarse-to-fine solving time: " + totalWatch.ElapsedMilliseconds + "ms");

            addLightStageParentObj[(int)AddLightStage.FilteredILSA].SetActive(false);
        }

        // This function needs to be rewritten.
        /// <summary>
        /// A strategy to reducing overall D(L, s) after coarse-to-fine strategy by replacing some lights with spotlights.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /*public void RefineWithSpotlight(GameObject lightPrefab, LightingGoal lightingGoal)
        {
            Stopwatch stageWatch = new Stopwatch();
            List<Light> existingLight = new List<Light>();
            List<Light> existingLightWithoutDirectionalLight = new List<Light>();
            List<SamplePoint> weakSamplePoint = new List<SamplePoint>();
            List<bool> isLightChecked = new List<bool>();
            List<float> lightContribution = new List<float>();
            bool isAllChecked = true;
            float improvement = 0.0f;

            addLightStageParentObj[(int)AddLightStage.RefineWithSpotlight].SetActive(true);
            stageWatch.Reset();
            stageWatch.Start();
            for (int i = 0; i < addLightStageLight[(int)AddLightStage.FilteredSpread].Count; i++)
            {
                AddLight(lightPrefab, addLightStageLight[(int)AddLightStage.FilteredSpread][i], addLightStageLightObj[(int)AddLightStage.RefineWithSpotlight],
                        addLightStageParentObj[(int)AddLightStage.RefineWithSpotlight].transform, addLightStageLight[(int)AddLightStage.RefineWithSpotlight]);
                isLightChecked.Add(false);
                lightContribution.Add(0.0f);
            }

            do
            {
                Color oldLightColor;
                float oldLightIntensity;
                float spotlightSpreadDifference = 0.0f;
                float lastSetupDifference = 0.0f;
                float maxContribution = -1.0f;
                int maxContributionIndex = 0;

                // Find max contribution light.
                for (int i = 0; i < addLightStageLight[(int)AddLightStage.RefineWithSpotlight].Count; i++)
                {
                    if (isLightChecked[i])
                        continue;

                    lightContribution[i] = ComputeLightContributionRatio(lightingGoal, addLightStageLight[(int)AddLightStage.RefineWithSpotlight][i]);
                    if (lightContribution[i] > maxContribution)
                    {
                        maxContribution = lightContribution[i];
                        maxContributionIndex = i;
                    }
                }
                isLightChecked[maxContributionIndex] = true;

                // Find sample points to be lit by spotlight.
                float minD = 999.0f;
                float maxD = -999.0f;
                weakSamplePoint.Clear();
                for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
                {
                    if (!lightingGoal.goalSamplePoint[i].isPainted() )
                        continue;

                    float difference;

                    difference = ComputeDifferenceBetweenSamplePointAndLight(lightingGoal.goalSamplePoint[i],
                                                                            addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex]);
                    minD = Mathf.Min(minD, difference);
                    maxD = Mathf.Max(maxD, difference);
                    if (difference < underSpotlightDifferenceThreshold)
                    {
                        //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //obj.transform.position = lightingGoal.goalSamplePoint[i].worldPosition;
                        //obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        weakSamplePoint.Add(lightingGoal.goalSamplePoint[i]);
                    }
                }
                //print("minD: " + minD + " maxD: " + maxD);

                lastSetupDifference = ComputeDifferenceBetweenLightingGoalAndLightingSetup(lightingGoal, addLightStageLight[(int)AddLightStage.RefineWithSpotlight]);

                // Transfer point light to spotlight and solve it.
                oldLightColor = addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex].color;
                oldLightIntensity = addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex].intensity;
                addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex].type = LightType.Spot;
                SolveSpotlightOrientationAndSpotAngle(weakSamplePoint, addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex]);
                existingLight = new List<Light>(FindObjectsOfType<Light>());
                SolveLightIntensityOfLightingSetup(existingLight, lightingGoal, true);

                spotlightSpreadDifference = ComputeDifferenceBetweenLightingGoalAndLightingSetup(lightingGoal, addLightStageLight[(int)AddLightStage.RefineWithSpotlight]);
                improvement = lastSetupDifference / (spotlightSpreadDifference + Mathf.Epsilon);
                //lightTestText.text += "\nSpotlight Spread D: " + spotlightSpreadDifference;
                //lightTestText.text += "\nImprove (Spot(n)/Spot(n+1)): " + improvement;
                if ((improvement < spotlightImprovementThreshold))
                {
                    addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex].type = LightType.Point;
                    addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex].color = oldLightColor;
                    addLightStageLight[(int)AddLightStage.RefineWithSpotlight][maxContributionIndex].intensity = oldLightIntensity;
                }

                isAllChecked = true;
                for (int i = 0; i < isLightChecked.Count; i++)
                    if (!isLightChecked[i])
                        isAllChecked = false;
            } while ((improvement > spotlightImprovementThreshold) && !isAllChecked);
            stageWatch.Stop();
            print("Refine with spotlight time: " + stageWatch.ElapsedMilliseconds + "ms");
            addLightStageParentObj[(int)AddLightStage.RefineWithSpotlight].SetActive(false);

            addLightStageParentObj[(int)AddLightStage.FilteredRefineWithSpotlight].SetActive(true);
            stageWatch.Reset();
            stageWatch.Start();
            FilterWeakLight(lightPrefab, lightingGoal, addLightStageLight[(int)AddLightStage.RefineWithSpotlight], contributionThreshold, addLightStageLightObj[(int)AddLightStage.FilteredRefineWithSpotlight],
                            addLightStageParentObj[(int)AddLightStage.FilteredRefineWithSpotlight].transform, addLightStageLight[(int)AddLightStage.FilteredRefineWithSpotlight]);
            existingLight = new List<Light>(FindObjectsOfType<Light>());
            existingLightWithoutDirectionalLight = FilterDirectionalLight(existingLight);
            SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, true);
            stageWatch.Stop();
            print("Filter refine with spotlight and solve time: " + stageWatch.ElapsedMilliseconds + "ms");
            addLightStageParentObj[(int)AddLightStage.FilteredRefineWithSpotlight].SetActive(false);
        }*/

        /// <summary>
        /// Reset all add light stage light for restarting adding light process.
        /// </summary>
        /// <param name="canReserveAddedLight">Is true when we want to save added light after restarting adding light process.</param>
        /// <param name="reservedStageLight">The stage of light that will be reserved.</param>
        void ResetAddLightStageList(bool canReserveAddedLight, AddLightStage reservedStageLight = AddLightStage.FilteredSpread)
        {
            for (int i = 0; i < (int)AddLightStage.MAX_STAGE_NUM; i++)
            {
                addLightStageLightObj.Clear();
                addLightStageLight.Clear();
                if (canReserveAddedLight && (addLightStageParentObj.Count > 0))
                    addLightStageParentObj[(int)reservedStageLight].transform.DetachChildren();
                foreach (GameObject obj in addLightStageParentObj)
                    Destroy(obj);
                addLightStageParentObj.Clear();
            }

            for (int i = 0; i < (int)AddLightStage.MAX_STAGE_NUM; i++)
            {
                addLightStageLightObj.Add(new List<GameObject>());
                addLightStageLight.Add(new List<Light>());
                addLightStageParentObj.Add(new GameObject(((AddLightStage)i).ToString()));
            }

        }

        /// <summary>
        /// Add light with some coarse light positions.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="coarseLightPosition">A region defines coarse light positions.</param>
        /// <param name="addedLightObj">Added light's gameObject will add to this list.</param>
        /// <param name="addedLightParent">The parent of added lights.</param>
        /// <param name="addedLightList">Added light's light will add to this list.</param>
        void AddCoarseLight(GameObject lightPrefab, List<Vector3> coarseLightPosition, 
            List<GameObject> addedLightObj, Transform addedLightParent, List<Light> addedLightList)
        {
            List<Light> existingLight = new List<Light>(FindObjectsOfType<Light>());
            List<Light> existingLightWithoutDirectionalLight = new List<Light>(FilterDirectionalLight(existingLight));
            Vector3 offsetBase = new Vector3(possibleLightVolume.Offset.x / 4.0f,
                possibleLightVolume.Offset.y / 4.0f, possibleLightVolume.Offset.z / 4.0f);

            for (int i = 0; i < coarseLightPosition.Count; i++)
            {
                bool isNearToExistingLight = false;

                for(int j = 0; j < existingLightWithoutDirectionalLight.Count; j++)
                {
                    Transform lightTransform = existingLightWithoutDirectionalLight[j].transform;

                    if (Mathf.Abs(lightTransform.position.x - coarseLightPosition[i].x) <= offsetBase.x &&
                        Mathf.Abs(lightTransform.position.y - coarseLightPosition[i].y) <= offsetBase.y &&
                        Mathf.Abs(lightTransform.position.z - coarseLightPosition[i].z) <= offsetBase.z)
                    {
                        isNearToExistingLight = true;
                        break;
                    }
                }

                if(!isNearToExistingLight)
                    if(unreachedAreaAveragePosition != Vector3.zero && Vector3.Distance(unreachedAreaAveragePosition, coarseLightPosition[i]) < 10.0f)
                        AddLight(lightPrefab, "CoarseLight " + i.ToString(), coarseLightPosition[i], addedLightObj, addedLightParent, addedLightList);
            }
        }

        /// <summary>
        /// Filter weak light (filter out low-contribution lights)
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="lightingGoal">The lighting goal that the user specified for wanted lighting effects.</param>
        /// <param name="lightToFilter">The list of light to be filtered.</param>
        /// <param name="contributionThreshold">The light's contribution over the this threshold will be filtered out.</param>
        /// <param name="filteredLightObj">Filtered light's gameObject will add to this list.</param>
        /// <param name="filteredLightParent">The parent of filtered lights.</param>
        /// <param name="filteredLightList">Filtered light's light will add to this list.</param>
        IEnumerator FilterWeakLight(GameObject lightPrefab, LightingGoal lightingGoal, List<Light> lightToFilter, float contributionThreshold,
            List<GameObject> filteredLightObj, Transform filteredLightParent, List<Light> filteredLightList)
        {
            float lightContribution = 0.0f;

#if DEBUG_CL
            print("====FilerWeakLight====");
#endif

            for (int i = 0; i < lightToFilter.Count; i++)
            {
                if (lightToFilter[i].intensity <= (0.0f + Mathf.Epsilon))
                    continue;

                yield return StartCoroutine(ComputeLightContributionRatioGPU(lightingGoal, lightToFilter[i], value => lightContribution = value));

                if (lightContribution >= contributionThreshold)
                {
#if DEBUG_CL
                    print(lightToFilter[i].name + ": " + lightContribution);
#endif
                    AddLight(lightPrefab, lightToFilter[i], filteredLightObj, filteredLightParent, filteredLightList);
                }
            }
#if DEBUG_CL
            print("======================");
#endif
        }

        /// <summary>
        /// Spread eight lights around the lightToSpread.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="lightToSpreadObj">The list of light to be spread.</param>
        /// <param name="spreadLightObj">Spread light's gameObject will add to this list.</param>
        /// <param name="spreadLightParent">The parent of spread lights.</param>
        /// <param name="spreadLightList">Spread light's light will add to this list.</param>
        void SpreadLight(GameObject lightPrefab, List<GameObject> lightToSpreadObj, 
            List<GameObject> spreadLightObj, Transform spreadLightParent, List<Light> spreadLightList)
        {
            Vector3 offsetBase = new Vector3(possibleLightVolume.Offset.x / 4.0f, 
                possibleLightVolume.Offset.y / 4.0f, possibleLightVolume.Offset.z / 4.0f);
            Vector3 offset = Vector3.zero;

            // Copy lights to spread
            for (int i = 0; i < lightToSpreadObj.Count; i++)
            {
                Light lightToSpread = lightToSpreadObj[i].GetComponent<Light>();
                AddLight(lightPrefab, lightToSpread, spreadLightObj, spreadLightParent, spreadLightList);

                offset.Set(offsetBase.x, offsetBase.y, offsetBase.z);
                AddLight(lightPrefab, lightToSpread.name + "Spread XYZ", lightToSpreadObj[i].transform.position + offset, spreadLightObj, spreadLightParent, spreadLightList);
                AddLight(lightPrefab, lightToSpread.name + "Spread -X-Y-Z", lightToSpreadObj[i].transform.position - offset, spreadLightObj, spreadLightParent, spreadLightList);

                offset.Set(-offsetBase.x, offsetBase.y, offsetBase.z);
                AddLight(lightPrefab, lightToSpread.name + "Spread -XYZ", lightToSpreadObj[i].transform.position + offset, spreadLightObj, spreadLightParent, spreadLightList);
                AddLight(lightPrefab, lightToSpread.name + "Spread X-Y-Z", lightToSpreadObj[i].transform.position - offset, spreadLightObj, spreadLightParent, spreadLightList);

                offset.Set(offsetBase.x, -offsetBase.y, offsetBase.z);
                AddLight(lightPrefab, lightToSpread.name + "Spread X-YZ", lightToSpreadObj[i].transform.position + offset, spreadLightObj, spreadLightParent, spreadLightList);
                AddLight(lightPrefab, lightToSpread.name + "Spread -XY-Z", lightToSpreadObj[i].transform.position - offset, spreadLightObj, spreadLightParent, spreadLightList);

                offset.Set(offsetBase.x, offsetBase.y, -offsetBase.z);
                AddLight(lightPrefab, lightToSpread.name + "Spread XY-Z", lightToSpreadObj[i].transform.position + offset, spreadLightObj, spreadLightParent, spreadLightList);
                AddLight(lightPrefab, lightToSpread.name + "Spread -X-YZ", lightToSpreadObj[i].transform.position - offset, spreadLightObj, spreadLightParent, spreadLightList);
            }
        }

        /// <summary>
        /// Add a light in scene.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="lightToCopy">The light to copy properties as new properties.</param>
        /// <param name="addedLightObj">Added light's gameObject will add to this list.</param>
        /// <param name="addedLightParent">The parent of added lights.</param>
        void AddLight(GameObject lightPrefab, Light lightToCopy, List<GameObject> addedLightObj, Transform addedLightParent)
        {
            GameObject obj = Instantiate(lightPrefab, lightToCopy.transform.position, Quaternion.identity);
            Light addedLight = obj.GetComponent<Light>();

            addedLight.type = lightToCopy.type;
            addedLight.range = lightToCopy.range;
            addedLight.intensity = lightToCopy.intensity;
            addedLight.color = lightToCopy.color;
            addedLight.spotAngle = lightToCopy.spotAngle;
            addedLightObj.Add(obj);
            obj.name = lightToCopy.name;
            obj.transform.parent = addedLightParent;
            obj.transform.forward = lightToCopy.transform.forward;
            obj.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Add a light in scene.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="lightToCopy">The light to copy properties as new properties.</param>
        /// <param name="addedLightObj">Added light's gameObject will add to this list.</param>
        /// <param name="addedLightParent">The parent of added lights.</param>
        /// <param name="addedLightList">Added light's light will add to this list.</param>
        void AddLight(GameObject lightPrefab, Light lightToCopy, List<GameObject> addedLightObj, Transform addedLightParent, List<Light> addedLightList)
        {
            GameObject obj = Instantiate(lightPrefab, lightToCopy.transform.position, Quaternion.identity);
            Light addedLight = obj.GetComponent<Light>();

            addedLight.type = lightToCopy.type;
            addedLight.range = lightToCopy.range;
            addedLight.intensity = lightToCopy.intensity;
            addedLight.color = lightToCopy.color;
            addedLight.spotAngle = lightToCopy.spotAngle;
            addedLightObj.Add(obj);
            addedLightList.Add(addedLight);
            obj.name = lightToCopy.name;
            obj.transform.parent = addedLightParent;
            obj.transform.forward = lightToCopy.transform.forward;
            obj.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Add a light in scene.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="objName">The name of the added light's game object.</param>
        /// <param name="lightPosition">The position of the added light.</param>
        /// <param name="addedLightObj">Added light's gameObject will add to this list.</param>
        /// <param name="addedLightParent">The parent of added lights.</param>
        void AddLight(GameObject lightPrefab, string objName, Vector3 lightPosition, List<GameObject> addedLightObj, Transform addedLightParent)
        {
            GameObject obj = Instantiate(lightPrefab, lightPosition, Quaternion.identity);
            Light addedLight = obj.GetComponent<Light>();

            addedLight.intensity = 0.0f;
            addedLightObj.Add(obj);
            obj.name = objName;
            obj.transform.parent = addedLightParent;
            obj.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Add a light in scene.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="objName">The name of the added light's game object.</param>
        /// <param name="lightPosition">The position of the added light.</param>
        /// <param name="addedLightObj">Added light's gameObject will add to this list.</param>
        /// <param name="addedLightParent">The parent of added lights.</param>
        /// <param name="addedLightList">Added light's light will add to this list.</param>
        void AddLight(GameObject lightPrefab, string objName, Vector3 lightPosition, List<GameObject> addedLightObj, Transform addedLightParent, List<Light> addedLightList)
        {
            GameObject obj = Instantiate(lightPrefab, lightPosition, Quaternion.identity);
            Light addedLight = obj.GetComponent<Light>();

            addedLight.intensity = 0.0f;
            addedLightObj.Add(obj);
            addedLightList.Add(addedLight);
            obj.name = objName;
            obj.transform.parent = addedLightParent;
            obj.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Add a light in scene with position.
        /// </summary>
        /// <param name="lightPrefab">The prefab to instaintiate when adding lights.</param>
        /// <param name="addedLightPosition">A region defines added light positions.</param>
        /// <param name="addedLightParent">The parent of added lights.</param>
        public void AddLightWithPosition(GameObject lightPrefab, List<Vector3> addedLightPosition, Transform addedLightParent)
        {
            for (int i = 0; i < addedLightPosition.Count; i++)
            {
                GameObject obj = Instantiate(lightPrefab, addedLightPosition[i], Quaternion.identity);
                Light addedLight = obj.GetComponent<Light>();

                addedLight.intensity = 0.0f;
                obj.name = "AddedLight" + i;
                obj.transform.parent = addedLightParent;
                obj.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            }
        }

        /// <summary>
        /// Apply inverse lighting result to a set of lights.
        /// </summary>
        /// <param name="lightToApply">The light to apply inverse lighting result.</param>
        /// <param name="solvedLightIntensities">A list of list that store solved intensities. (R, G, B for a light)</param>
        /// <param name="canAdjustLightColor">Is true when you can change the color of light.</param>
        public IEnumerator ApplyInverseLightingResult(List<Light> lightToApply, List<List<double>> solvedLightIntensities, bool canAdjustLightColor)
        {
            if (lightToApply.Count != solvedLightIntensities[0].Count)
                yield break;

            for (int i = 0; i < lightToApply.Count; i++)
            {
                if (canAdjustLightColor)
                {
                    Color solvedIntensity = Color.white;

                    solvedIntensity.r = (float)solvedLightIntensities[0][i];
                    solvedIntensity.g = (float)solvedLightIntensities[1][i];
                    solvedIntensity.b = (float)solvedLightIntensities[2][i];
#if COLOR_SPACE_LINEAR
                    solvedIntensity = solvedIntensity.gamma;
#endif

                    lightToApply[i].intensity = (solvedIntensity.maxColorComponent > 1.5f) ? 1.5f : solvedIntensity.maxColorComponent;
                    if (solvedIntensity.maxColorComponent > 0)
                        lightToApply[i].color = solvedIntensity / solvedIntensity.maxColorComponent;
                    else
                        lightToApply[i].color = Color.black;
                }
                else
                {
                    float solvedIntensity = (float)solvedLightIntensities[0][i];

#if COLOR_SPACE_LINEAR
                    solvedIntensity = ColorSpaceUtility.LinearToGamma(solvedIntensity);
#endif
                    lightToApply[i].intensity = solvedIntensity;
                }
            }
        }

        /// <summary>
        /// Compute provided lights' contribution ratio by L2 norm (||light's illumination||2 / ||lighting goal's goal illumination - ambient||2)
        /// The contribution don't consider the ambient term.
        /// </summary>
        /// <param name="lightingGoal">The goal that have all sample points information.</param>
        /// <param name="light">The light illuminate sample points.</param>
        public float ComputeLightContributionRatio(LightingGoal lightingGoal, Light light)
        {
            float contribution = 0.0f;
            float goalAmount = 0.0f;

            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color lightContribution = Color.black;
                Color goalIllumination = lightingGoal.goalSamplePoint[i].GoalIllumination;
                float attenuation = RenderingSystem.instance.ComputeLightAttenuation(lightingGoal.goalSamplePoint[i].worldPosition, light.transform.position, light.range, light.type);

                if (attenuation <= (0.0f + Mathf.Epsilon)) // Filter points that are not in range of light.
                    continue;
                if ((light.type == LightType.Spot) && !RenderingSystem.instance.IsUnderSpotlight(lightingGoal.goalSamplePoint[i].worldPosition, light)) // Filter points that are not under spotlight.
                    continue;

                lightContribution = RenderingSystem.instance.ComputeLambertian(lightingGoal.goalSamplePoint[i].worldPosition,
                                                                      lightingGoal.goalSamplePoint[i].worldNormal,
                                                                      lightingGoal.goalSamplePoint[i].Kd,
                                                                      light,
                                                                      canCastShadow);
#if COLOR_SPACE_LINEAR
                lightContribution = lightContribution.gamma;           
#endif

                contribution += lightContribution.r * lightContribution.r + lightContribution.g * lightContribution.g + lightContribution.b * lightContribution.b;
                goalAmount += goalIllumination.r * goalIllumination.r + goalIllumination.g * goalIllumination.g + goalIllumination.b * goalIllumination.b;
            }

            return Mathf.Sqrt(contribution + Mathf.Epsilon) / Mathf.Sqrt(goalAmount + Mathf.Epsilon);
        }

        /// <summary>
        /// Compute provided lights' contribution ratio by L2 norm (||light's illumination||2 / ||lighting goal's goal illumination - ambient||2)
        /// The contribution don't consider the ambient term.
        /// </summary>
        /// <param name="lightingGoal">The goal that have all sample points information.</param>
        /// <param name="light">The light illuminate sample points.</param>
        /// <param name="lightContributionRatio">The output light contribution ratio.</param>
        public IEnumerator ComputeLightContributionRatioGPU(LightingGoal lightingGoal, Light light, System.Action<float> lightContributionRatio)
        {
            float contribution = 0.0f;
            float goalAmount = 0.0f;

            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
                lightingGoal.goalSamplePoint[i].currentIllumination = Color.black;

            if (RenderingSystem.instance.CanReuseLightContributionVector(light))
                RenderingSystem.instance.ReuseLightContributionVector(light);
            else
                RenderingSystem.instance.SetupReusedLightContributionVector(light, lightingGoal.goalSamplePoint);

            RenderingSystem.instance.ComputeAndAddLambertianGPUReuseLightContributionVector(lightingGoal.goalSamplePoint, light, canCastShadow);

            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color lightContribution = lightingGoal.goalSamplePoint[i].currentIllumination;
                Color goalIllumination = lightingGoal.goalSamplePoint[i].GoalIllumination;
                float attenuation;

                if (lightContribution.r <= 0.0f + Mathf.Epsilon &&
                    lightContribution.g <= 0.0f + Mathf.Epsilon &&
                    lightContribution.b <= 0.0f + Mathf.Epsilon) // Is filtered by attenuation, spotlight or shadow.
                    continue;

                attenuation = RenderingSystem.instance.ComputeLightAttenuation(lightingGoal.goalSamplePoint[i].worldPosition,
                    light.transform.position, light.range, light.type);
                /*
#if COLOR_SPACE_LINEAR
                lightContribution = lightContribution + LightProbeUtility.ShadeSH9(lightingGoal.goalSamplePoint[i].worldPosition,
                    lightingGoal.goalSamplePoint[i].worldNormal) * lightingGoal.goalSamplePoint[i].Kd.linear;
#else
                lightContribution = lightContribution + LightProbeUtility.ShadeSH9(lightingGoal.goalSamplePoint[i].worldPosition,
                    lightingGoal.goalSamplePoint[i].worldNormal) * lightingGoal.goalSamplePoint[i].Kd;
#endif
                */
                contribution += (lightContribution.r * lightContribution.r + lightContribution.g * lightContribution.g + lightContribution.b * lightContribution.b);
                goalAmount += (goalIllumination.r * goalIllumination.r + goalIllumination.g * goalIllumination.g + goalIllumination.b * goalIllumination.b) * attenuation;
            }

            lightContributionRatio(Mathf.Sqrt(contribution + Mathf.Epsilon) / Mathf.Sqrt(goalAmount + Mathf.Epsilon));
            yield break;
        }

        /// <summary>
        /// Compute difference of a lighting goal by L2 norm (||lighting setup's illumination - lighting goal's goal illumination||2)
        /// </summary>
        /// <param name="lightingGoal">The goal that have all sample points information.</param>
        /// <param name="isWeighted">Is true when we want to solve weighted least squares.</param>
        public float ComputeIlluminationDifferenceOfLightingGoal(LightingGoal lightingGoal, bool isWeighted = true)
        {
            float totalDifference = 0.0f;

            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color difference = Color.black;

                difference = (lightingGoal.goalSamplePoint[i].currentIllumination - lightingGoal.goalSamplePoint[i].GoalIllumination);
                if (isWeighted)
                    totalDifference += (difference.r * difference.r + difference.g * difference.g + difference.b * difference.b) * lightingGoal.goalSamplePoint[i].Weight;
                else
                    totalDifference += (difference.r * difference.r + difference.g * difference.g + difference.b * difference.b);
            }

            return Mathf.Sqrt(totalDifference + Mathf.Epsilon);
        }

        /// <summary>
        /// Compute normalized difference of a lighting goal by L2 norm (||lighting setup's illumination - lighting goal's goal illumination||2) / ||lighting goal's goal illumination||2
        /// </summary>
        /// <param name="lightingGoal">The goal that have all sample points information.</param>
        /// <param name="isWeighted">Is true when we want to solve weighted least squares.</param>
        public float ComputeNormalizedIlluminationDifferenceOfLightingGoal(LightingGoal lightingGoal, bool isWeighted = true)
        {
            float totalDifference = 0.0f;
            float totalGoalIllumination = 0.0f;

            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color difference = Color.black;
                Color goalIllumination = lightingGoal.goalSamplePoint[i].GoalIllumination;

                difference = (lightingGoal.goalSamplePoint[i].currentIllumination - lightingGoal.goalSamplePoint[i].GoalIllumination);
                if (isWeighted)
                {
                    totalDifference += (difference.r * difference.r + difference.g * difference.g + difference.b * difference.b) * lightingGoal.goalSamplePoint[i].Weight;
                    totalGoalIllumination += (goalIllumination.r * goalIllumination.r + goalIllumination.g * goalIllumination.g + goalIllumination.b * goalIllumination.b) * lightingGoal.goalSamplePoint[i].Weight;
                }
                else
                {
                    totalDifference += (difference.r * difference.r + difference.g * difference.g + difference.b * difference.b);
                    totalGoalIllumination += (goalIllumination.r * goalIllumination.r + goalIllumination.g * goalIllumination.g + goalIllumination.b * goalIllumination.b);
                }
            }

            return Mathf.Sqrt(totalDifference + Mathf.Epsilon) / Mathf.Sqrt(totalGoalIllumination + Mathf.Epsilon);
        }

        /// <summary>
        /// Compute cosine similarity of a lighting goal.
        /// </summary>
        /// <param name="lightingGoal">The goal that have all sample points information.</param>
        /// <param name="isWeighted">Is true when we want to solve weighted least squares.</param>
        public float ComputeCosineSimilarityOfLightingGoal(LightingGoal lightingGoal, bool isWeighted = true)
        {
            float[] dotProduct = { 0.0f, 0.0f, 0.0f };
            float[] currentIlluminationMagnitude = { 0.0f, 0.0f, 0.0f };
            float[] goalIlluminationMagnitude = { 0.0f, 0.0f, 0.0f };
            float[] similarity = { 0.0f, 0.0f, 0.0f };

            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                dotProduct[0] += lightingGoal.goalSamplePoint[i].currentIllumination.r * lightingGoal.goalSamplePoint[i].GoalIllumination.r;
                dotProduct[1] += lightingGoal.goalSamplePoint[i].currentIllumination.g * lightingGoal.goalSamplePoint[i].GoalIllumination.g;
                dotProduct[2] += lightingGoal.goalSamplePoint[i].currentIllumination.b * lightingGoal.goalSamplePoint[i].GoalIllumination.b;

                currentIlluminationMagnitude[0] += lightingGoal.goalSamplePoint[i].currentIllumination.r * lightingGoal.goalSamplePoint[i].currentIllumination.r;
                currentIlluminationMagnitude[1] += lightingGoal.goalSamplePoint[i].currentIllumination.g * lightingGoal.goalSamplePoint[i].currentIllumination.g;
                currentIlluminationMagnitude[2] += lightingGoal.goalSamplePoint[i].currentIllumination.b * lightingGoal.goalSamplePoint[i].currentIllumination.b;

                goalIlluminationMagnitude[0] += lightingGoal.goalSamplePoint[i].GoalIllumination.r * lightingGoal.goalSamplePoint[i].GoalIllumination.r;
                goalIlluminationMagnitude[1] += lightingGoal.goalSamplePoint[i].GoalIllumination.g * lightingGoal.goalSamplePoint[i].GoalIllumination.g;
                goalIlluminationMagnitude[2] += lightingGoal.goalSamplePoint[i].GoalIllumination.b * lightingGoal.goalSamplePoint[i].GoalIllumination.b;
            }

            for(int i = 0; i < 3; i++)
            {
                currentIlluminationMagnitude[i] = Mathf.Sqrt(currentIlluminationMagnitude[i]);
                goalIlluminationMagnitude[i] = Mathf.Sqrt(goalIlluminationMagnitude[i]);
                similarity[i] = dotProduct[i] / (currentIlluminationMagnitude[i] * goalIlluminationMagnitude[i] + Mathf.Epsilon);
            }

            return (similarity[0] + similarity[1] + similarity[2]) / 3.0f;
        }

        /// <summary>
        /// Decide whether the lighting goal is reached or not.
        /// </summary>
        /// <param name="differenceThreshold">The threshold will be used to decide whether a point reaches the lighting goal.</param>
        /// <param name="reachedRatioThreshold">The threshold that decide whether the result reaches the lighting goal. (reached / total)</param> 
        /// <param name="lightingGoal">The goal that have all sample points information.</param>
        /// <param name="isWeighted">Is true when we want to solve weighted least squares.</param>
        public bool isLightingGoalReached(float differenceThreshold, float reachedRatioThreshold, LightingGoal lightingGoal, bool isWeighted = true)
        {
            int reachedCount = 0;
            int unreachedCount = 0;

            unreachedAreaAveragePosition = Vector3.zero;
            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
            {
                Color difference = (lightingGoal.goalSamplePoint[i].currentIllumination - lightingGoal.goalSamplePoint[i].GoalIllumination);

                if (isWeighted)
                {
                    if (Mathf.Sqrt((difference.r * difference.r + difference.g * difference.g + difference.b * difference.b) 
                        * lightingGoal.goalSamplePoint[i].Weight) <= differenceThreshold) 
                        reachedCount++;
                    else
                    {
                        unreachedAreaAveragePosition += lightingGoal.goalSamplePoint[i].worldPosition;
                        unreachedCount++;
                    }
                   /*else
                    {
                        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        temp.transform.position = lightingGoal.goalSamplePoint[i].worldPosition;
                        temp.transform.localScale = Vector3.one * 0.1f;
                    }*/
                }
                else
                {
                    if (Mathf.Sqrt((difference.r * difference.r + difference.g * difference.g + difference.b * difference.b)) <= differenceThreshold)
                        reachedCount++;
                }
            }
            unreachedAreaAveragePosition /= unreachedCount;

            print("<color=red>" + ((float)reachedCount / lightingGoal.goalSamplePoint.Count) + "</color>");
            if(((float)reachedCount / lightingGoal.goalSamplePoint.Count) >= reachedRatioThreshold)
                return true;
            return false;
        }

        /*
        /// <summary>
        /// Compute difference between a sample point and a light by L2 norm (||light's illumination - sample point's goal illumination||2)
        /// P.S. the computation of illumination is almost the same as ComputeSamplePointGoalIllumination in SamplePointManager.
        /// </summary>
        /// <param name="samplePoint">The goal that have a sample points information.</param>
        /// <param name="light">The light illuminate the sample point.</param>
        /// <param name="isWeighted">Is true when we want to solve weighted least squares.</param>
        public float ComputeIlluminationDifferenceBetweenSamplePointAndLight(SamplePoint samplePoint, Light light, bool isWeighted = true)
        {
            float totalDifference = 0.0f;
            Color difference = Color.black;
            Color totalIllumination = Color.black;

            totalIllumination += RenderingSystem.instance.ComputeLambertian(samplePoint.worldPosition,
                                                                    samplePoint.worldNormal,
                                                                    samplePoint.Kd,
                                                                    light,
                                                                    canCastShadow);
            totalIllumination += RenderingSystem.instance.ComputeAmbient(samplePoint.Kd);

#if COLOR_SPACE_LINEAR
            totalIllumination = totalIllumination.gamma;
#endif
            if(isWeighted)
                difference = (totalIllumination - samplePoint.GoalIllumination) * samplePoint.Weight;
            else
                difference = totalIllumination - samplePoint.GoalIllumination;
            totalDifference += difference.r * difference.r + difference.g * difference.g + difference.b * difference.b;

            return Mathf.Sqrt(totalDifference + Mathf.Epsilon);
        }
        */

        /// <summary>
        /// To update the LumenLight's lumens with current light intensity.
        /// </summary>
        /// <param name="lightToUpdate">The list of lights to be updated.</param>
        /*void UpdateLumenWithLightIntensity(List<LumenLight> lumenToUpdate)
        {
            for(int i = 0; i < lumenToUpdate.Count; i++)
                lumenToUpdate[i].SetLumensWithLightIntensity();
        }*/

        /// <summary>
        /// Is true when all light's lumen match it's range. Will update light's range after comparing.
        /// </summary>
        /// <param name="lightToCompare">The light's component.</param>
        /// <param name="lumenToCompare">The lumen script of the light.</param>
        /// <param name="matchProportionThreshold">Absolute difference lower than this proportion will count as matching.</param>
        /// <returns>Is true when all light's lumen match it's properties.</returns>
        /*bool DoesAllLumenMatchLightRange(List<Light> lightToCompare, List<LumenLight> lumenToCompare, float matchProportionThreshold = 0.1f)
        {
            bool doesAllLumenMatchLightRange = true;

            for (int i = 0; i < lightToCompare.Count; i++)
            {
                if (lumenToCompare[i].lumens < 0)
                    continue;

                float newLightRange = lumenToCompare[i].GetLightRangeFromLumens();

                if(Mathf.Abs(lightToCompare[i].range - newLightRange) > newLightRange * matchProportionThreshold) // still continue to update light's range for all lights.
                    doesAllLumenMatchLightRange = false;
                print("L" + i.ToString() + " Current: " + lightToCompare[i].range + " New: " + newLightRange + " Threshold: " + newLightRange * matchProportionThreshold);
                lightToCompare[i].range = newLightRange;
            }

            return doesAllLumenMatchLightRange;
        }*/
    }
}
