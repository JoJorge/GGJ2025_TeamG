#define COLOR_SPACE_LINEAR

using UnityEngine;
using UnityEngine.Assertions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace CliffLeeCL
{
    /// <summary>
    /// For the Inverse Lighting Design part of the LightingDesign.cs
    /// </summary>
    public partial class LightingDesign : MonoBehaviour
    {
        [Header("Inverse Interface")]
        /// <summary>
        /// Draw paint brush gizmo.
        /// </summary>
        public GameObject PaintBrush;

        /// <summary>
        /// The peak value of the brushSize.
        /// </summary>
        public float brushSizeMinimum, brushSizeMaximum;

        /// <summary>
        /// The Color of the raycasting pixel.
        /// </summary>
        public Color raycastHitPixelColor;

        /// <summary>
        /// The Image that show the current color of the eyeDropper.
        /// </summary>
        public Image showEyeDropperColor;

        private Texture2D screenCopy;
        public PaintingStatus currentPaintingStatus;

        /// <summary>
        /// Brushstroke's brightness.
        /// </summary>
        [SerializeField]
        private Slider brightnessSlider;
        private float currentBrightness = 0.0f;

        /// <summary>
        /// 筆刷速度。
        /// </summary>
        [HideInInspector]
        public float intensityBrushSpeed = 0.3f;

        LightingGoal lightingGoal = new LightingGoal();

        public int currentType;

        /// <summary>
        /// For the raycasting results of vive.
        /// </summary>
        private GameObject vivePointerRight;

        [HideInInspector]
        public bool isSamplePointPaintingUpdated;

        //  Show the color of the brush.
        [SerializeField]
        private Image eyeDrop;

        /// <summary>
        /// Is call by LightingDesign.cs's Awake.
        /// </summary>
        public void InverseInterfaceAwake()
        {
            if(GameObject.FindGameObjectWithTag("VivePointerRight"))
                vivePointerRight = GameObject.FindGameObjectWithTag("VivePointerRight").transform.Find("EventRaycaster").gameObject;
            if(GameObject.FindGameObjectWithTag("EyeDropCamera"))
                screenCopy = new Texture2D(GameObject.FindGameObjectWithTag("EyeDropCamera").gameObject.GetComponent<Camera>().pixelWidth, GameObject.FindGameObjectWithTag("ViveHeadset").gameObject.GetComponent<Camera>().pixelHeight);

        }

        /// <summary>
        /// Is call by LightingDesign.cs's Start.
        /// </summary>
        public void InverseInterfaceStart()
        {
        }

        /// <summary>
        /// Is call by LightingDesign.cs's Update.
        /// </summary>
        public void InverseInterfaceUpdate()
        {
        }

        public void GetPixelColor(Vector2 pixelPosition, Camera viveHeadSet)
        {
            StartCoroutine(GetScreenPixelColor(pixelPosition, viveHeadSet));
        }

        //  The ReadPixel function can only be called while the frame is end.
        IEnumerator GetScreenPixelColor(Vector2 raycastHitPixel,Camera eyeDropCamera)
        {
            yield return new WaitForEndOfFrame();

            //Get the screen shot from the viveHeadSet instead of the computer screen.
            eyeDropCamera.Render();
            RenderTexture.active = eyeDropCamera.targetTexture;

            screenCopy.ReadPixels( new Rect(0, 0, eyeDropCamera.pixelWidth, eyeDropCamera.pixelHeight), 0,  0);
            raycastHitPixelColor = screenCopy.GetPixelBilinear(raycastHitPixel.x, raycastHitPixel.y);
            screenCopy.Apply();

            RenderTexture.active = null;

        }

        /// <summary>
        /// Switch the painting status.
        /// </summary>
        public void SwitchPaintingStatus()
        {
            switch(currentPaintingStatus)
            {
                //  some error occurs when switch to eyedropper. Abandon Temporarily.
                case PaintingStatus.Color:
                    currentPaintingStatus = PaintingStatus.Brighten;
                    currentType = 2;
                    break;
                //case PaintingStatus.SpotlightColor:
                //    currentPaintingStatus = PaintingStatus.Brighten;
                //    currentType = 2;
                    break;
                case PaintingStatus.Brighten:
                    currentPaintingStatus = PaintingStatus.Darken;
                    currentType = 3;
                    break;
                case PaintingStatus.Darken:
                    currentPaintingStatus = PaintingStatus.Color;
                    currentType = 0;
                    break;
            }
        }

        /// <summary>
        /// For update the goal for entering inverseIdleState.
        /// </summary>
        public void UpdateGoalIlluminationWtihExistingLightWithoutDirectionalLight()
        {
            List<Light> existingLight = new List<Light>(FindObjectsOfType<Light>());
            List<Light> existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);

            StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, SamplePointManager.instance.generatedSamplePoint, true,
                                                InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
        }

        /// <summary>
        /// For solve inverse lighting button to trigger coroutine.
        /// </summary>
        public void StartSolveInverseLighting()
        {
            StartCoroutine(SolveInverseLighting());
        }

        /// <summary>
        /// Solve light intensities with current lighting goal and existing lights.
        /// </summary>
        IEnumerator SolveInverseLighting()
        {
            Stopwatch totalWatch = new Stopwatch();
            List<Light> existingLight = new List<Light>(FindObjectsOfType<Light>());
            List<Light> existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);
            float difference = 0.0f;

            InverseLightingSolver.instance.isSolvingInverseLighting = true;
            totalWatch.Start();
            lightingGoal.goalSamplePoint = SamplePointManager.instance.generatedSamplePoint;
            for (int i = 0; i < lightingGoal.goalSamplePoint.Count; i++)
                lightingGoal.goalSamplePoint[i].SetTokenVisibility(false);

            if (existingLightWithoutDirectionalLight.Count == 0)
            {
                yield return StartCoroutine(InverseLightingSolver.instance.CoarseToFine(InverseLightingSolver.instance.pointLightPrefab, lightingGoal, false));
                InverseLightingSolver.instance.addLightStageParentObj[(int)AddLightStage.FilteredILSA].SetActive(true);
                for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA].Count; i++)
                    InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA][i]);
            }
            else
            {
                /*yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, true,
                                                InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
                yield return StartCoroutine(InverseLightingSolver.instance.SolveLightIntensityOfLightingSetup(existingLightWithoutDirectionalLight, lightingGoal, true));
                print("Approximate difference: " + InverseLightingSolver.instance.ComputeIlluminationDifferenceOfLightingGoal(lightingGoal));*/
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
                    for (int i = 0; i < InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA].Count; i++)
                        InitializeLight(InverseLightingSolver.instance.addLightStageLight[(int)AddLightStage.FilteredILSA][i]);
                }
            }

            existingLight = new List<Light>(FindObjectsOfType<Light>());
            existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);
            yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, false,
                                   InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
            difference = InverseLightingSolver.instance.ComputeNormalizedIlluminationDifferenceOfLightingGoal(lightingGoal);
            print("Final existing lights diff: " + difference);
            UpdateLightFixtureMaterial(existingLightWithoutDirectionalLight);
            SamplePointManager.instance.ResetPaintedSamplePoint(lightingGoal.goalSamplePoint);
            yield return StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, true,
                                                InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
            LightingManager.instance.SaveLightingSetup();
            totalWatch.Stop();
            InverseLightingSolver.instance.isSolvingInverseLighting = false;
            print("Solve inverse lighting process time: " + totalWatch.ElapsedMilliseconds + "ms");
        }

        /// <summary>
        /// Solve least squares with current lighting goal and existing lights.
        /// </summary>
        public void SolveSpotlightReplacement()
        {
            List<Light> existingLight = new List<Light>(FindObjectsOfType<Light>());
            List<Light> existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);

            lightingGoal.goalSamplePoint = SamplePointManager.instance.generatedSamplePoint;

            //InverseLightingSolver.instance.SolveSpotlightReplacement(existingLight, lightingGoal, 3, true);

            existingLight = new List<Light>(FindObjectsOfType<Light>());
            existingLightWithoutDirectionalLight = InverseLightingSolver.instance.FilterDirectionalLight(existingLight);
            UpdateLightFixtureMaterial(existingLight);
            SamplePointManager.instance.ResetPaintedSamplePoint(lightingGoal.goalSamplePoint);
            StartCoroutine(SamplePointManager.instance.ComputeCurrentIlluminationOfSamplePoint(existingLightWithoutDirectionalLight, lightingGoal.goalSamplePoint, true,
                                                InverseLightingSolver.instance.canCastShadow, InverseLightingSolver.instance.canUseGlobalIllumination));
            LightingManager.instance.SaveLightingSetup();
        }

        public void UpdateSamplePointPaintingStatus()
        {
            for (int i = 0; i < SamplePointManager.instance.generatedSamplePoint.Count; i++)
            {
                if(SamplePointManager.instance.generatedSamplePoint[i].paintingStatus > 0)
                    SamplePointManager.instance.generatedSamplePoint[i].paintingStatus += 1;
            }                
        }
    }
}
