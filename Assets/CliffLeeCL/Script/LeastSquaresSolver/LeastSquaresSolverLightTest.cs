#define COLOR_SPACE_LINEAR

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CliffLeeCL
{
    public class LeastSquaresSolverLightTest : MonoBehaviour
    {
        public Text lightTestText;
        public bool canAdjustLightColor = false;
        public bool canSolveSpotlightReplacement = false;

        LightingGoal lightingGoal = new LightingGoal();

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (Input.GetMouseButtonUp(2))
                StartCoroutine(SolveInverseLighting());
        }

        IEnumerator SolveInverseLighting()
        {
            Stopwatch totalWatch = new Stopwatch();
            List<Light> existingLight = new List<Light>(FindObjectsOfType<Light>());
            List<Light> existingLightWithoutDirectionalLight= InverseLightingSolver.instance.FilterDirectionalLight(existingLight);
            float difference = 0.0f;

            InverseLightingSolver.instance.isSolvingInverseLighting = true;
            totalWatch.Start();
            lightingGoal.goalSamplePoint = SamplePointManager.instance.generatedSamplePoint;
                
            if (existingLightWithoutDirectionalLight.Count == 0)
            {
                yield return StartCoroutine(InverseLightingSolver.instance.CoarseToFine(InverseLightingSolver.instance.pointLightPrefab, lightingGoal, false, AddLightStage.FilteredILSA));

                InverseLightingSolver.instance.addLightStageParentObj[(int)AddLightStage.FilteredILSA].SetActive(true);
                for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Coarse].Count; i++)
                    LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Coarse][i]);
                for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredCoarse].Count; i++)
                    LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredCoarse][i]);
                for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Spread].Count; i++)
                    LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Spread][i]);
                for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredSpread].Count; i++)
                    LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredSpread][i]);
                for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.ILSA].Count; i++)
                    LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.ILSA][i]);
                for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA].Count; i++)
                    LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA][i]);
            }
            else
            {
                if (canSolveSpotlightReplacement)
                {
                    //InverseLightingSolver.instance.SolveSpotlightReplacement(existingLightWithoutDirectionalLight, lightingGoal, 3, true);
                }
                else
                {
                    /*yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, true,
                                                InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
                    yield return StartCoroutine(InverseLightingSolver.instance.SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, true));
                    print("Approximate difference: " + InverseLightingSolver.instance.ComputeIlluminationDifferenceOfLightingGoal(lightingGoal));*/
                    //InverseLightingSolver.instance.SolveColorTemperatureOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal);
                    //InverseLightingSolver.instance.SolveLightLumenOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, false);
                    yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, true,
                            InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
                    yield return StartCoroutine(InverseLightingSolver.instance.SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, 
                            lightingGoal, true, InverseLightingSolver.instance.canApproximateGlobalLighting));
                    yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, false,
                            InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
                    
                    if (!InverseLightingSolver.instance.isLightingGoalReached(InverseLightingSolver.instance.differenceThreshold, 
                        InverseLightingSolver.instance.reachedRatioThreshold, lightingGoal))
                    {
                        yield return StartCoroutine(InverseLightingSolver.instance.CoarseToFine(InverseLightingSolver.instance.pointLightPrefab, lightingGoal, true, AddLightStage.FilteredILSA));
                        InverseLightingSolver.instance.addLightStageParentObj[(int)AddLightStage.FilteredILSA].SetActive(true);
                        for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Coarse].Count; i++)
                            LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Coarse][i]);
                        for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredCoarse].Count; i++)
                            LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredCoarse][i]);
                        for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Spread].Count; i++)
                            LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.Spread][i]);
                        for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredSpread].Count; i++)
                            LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredSpread][i]);
                        for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.ILSA].Count; i++)
                            LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.ILSA][i]);
                        for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA].Count; i++)
                            LightingDesign.instance.InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA][i]);
                    }
                }
            }
            existingLight = new List<Light>(FindObjectsOfType<Light>());
            existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);
            yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, false,
                        InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
            difference = InverseLightingSolver.instance.ComputeNormalizedIlluminationDifferenceOfLightingGoal(lightingGoal);
            print("Final existing lights diff: " + difference);
            LightingDesign.instance.UpdateLightFixtureMaterial(existingLightWithoutDirectionalLight);
            SamplePointManager.instance.ResetPaintedSamplePoint(lightingGoal.goalSamplePoint);
            yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, true,
                                                InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
            totalWatch.Stop();
            InverseLightingSolver.instance.isSolvingInverseLighting = false;
            print("Solve inverse lighting process time: " + totalWatch.ElapsedMilliseconds + "ms");
        }

        string GetGroundTruthDifference(List<Light> lightToApply, List<List<double>> solvedLightIntensities)
        {
            string differenceLog = "Ground truth difference: \n(solved - ground truth)\n";

            if (lightToApply.Count != solvedLightIntensities[0].Count)
            {
                print("There is wrong input for ApplyInverseLightingResult!");
                return "Error!";
            }

            for (int i = 0; i < lightToApply.Count; i++)
            {
                Color groundTruthIntensity = lightToApply[i].intensity * lightToApply[i].color;
                Color solvedIntensity = Color.white;
                Color originSolvedIntensity;

                solvedIntensity.r = (float)solvedLightIntensities[0][i];
                solvedIntensity.g = (float)solvedLightIntensities[1][i];
                solvedIntensity.b = (float)solvedLightIntensities[2][i];
#if COLOR_SPACE_LINEAR
                solvedIntensity = solvedIntensity.gamma;
#endif
                originSolvedIntensity = solvedIntensity;
                if (solvedIntensity.maxColorComponent > 0)
                    solvedIntensity = solvedIntensity / solvedIntensity.maxColorComponent;
                else
                    solvedIntensity = Color.black;

                differenceLog += "<color=#" + ColorUtility.ToHtmlStringRGB(solvedIntensity) + ">Light " + i.ToString() + ": ("
                    + (originSolvedIntensity.r - groundTruthIntensity.r).ToString("0.000") + ", "
                    + (originSolvedIntensity.g - groundTruthIntensity.g).ToString("0.000") + ", "
                    + (originSolvedIntensity.b - groundTruthIntensity.b).ToString("0.000") + ")</color>\n";
            }

            return differenceLog;
        }
    }
}