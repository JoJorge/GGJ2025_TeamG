using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

public enum GizmoTypes { Position, Rotation, Scale }
public enum GizmoAxis { Center, X, Y, Z }
public enum SpotLightAdjust { Angle, Range, Orientation }
public enum PointLightAdjust {  Range, Orientation }
public enum IndirectIdleStateEnter { Position, ShadowDrag }
public enum InverseIdleStateEnter { Position, Painting }
public enum LightingDesignState { Direct, Indirect, Inverse }
public enum PaintingStatus { Color, EyeDropper, Brighten, Darken, SpotlightColor }

namespace CliffLeeCL
{
    /// <summary>
    /// The class includes lighting design operations.
    /// </summary>
    public partial class LightingDesign : MonoBehaviour, IContext
    {
        /// <summary>
        /// The variable is used to access this class.
        /// </summary>
        public static LightingDesign instance;

        [Header("Main")]
        /// <summary>
        /// The object represents for the selection range of point lights.
        /// </summary>
        public GameObject selectionRangePointLightPrefab;
        /// <summary>
        /// The object represents for the selection range of spotlights.
        /// </summary>
        public GameObject selectionRangeSpotlightPrefab;
        /// <summary>
        /// The object represents for the selection range of directional lights.
        /// </summary>
        public GameObject selectionRangeDirectionalLightPrefab;
        /// <summary>
        /// The added light should be addLightDistance away from main camera.
        /// </summary>
        public float addLightDistance = 5.0f;
        /// <summary>
        /// The threshold to used when deleting lighting (dot value between light direction and headset's forward).
        /// </summary>
        public float deleteLightThreshold = -0.3f;
        public float spotAngleAdjustSpeed = 10.0f;

        ///<summary>
        /// Define the boundary of resizing.
        ///</summary>
        public float minimumOfResize = 0.3f;
        public float maximumOfResize = 6.0f;
        public float defaultScale = 1.3f;
        public float resizeToDefaultTime = 2.0f;

        /// <summary>
        /// The state machine controls the agnet's behaviour.
        /// </summary>
        private StateMachine<State<LightingDesign>, LightingDesign> stateMachine;

        [HideInInspector]
        /// <summary>
        /// Is true when the triggered controller is left controller, and is false when the triggered controller is right controller.
        /// </summary>
        public bool isLeftViveController = true;
        [HideInInspector]
        /// <summary>
        /// Is true when using pointer to move light's position and is false when the using controller to move light's position.
        /// </summary>
        public bool isPositionedByPointer = true;
        [HideInInspector]
        public bool isControllerInSelectionRange;

        [HideInInspector]
        /// <summary>
        /// Store all selectionRange in the scene.
        /// </summary>
        public List<GameObject> selectionRangeInScene = new List<GameObject>();
        [HideInInspector]
        /// <summary>
        /// The order of items in the container must be exactly the same .
        /// </summary>
        public List<Light> selectedLights;
        [HideInInspector]
        public List<Transform> SelectedObjects;

        /// <summary>
        /// To count the number of each kind of lights.
        /// 0 : directional light, 1 : point light, 2 : spot light
        /// </summary>
        private int[] lightTypeCount = new int[3];

        public bool enableLightGizmo = true;

        public SpotLightAdjust spotLightCurrentAdjustTarget = SpotLightAdjust.Angle;
        public PointLightAdjust pointLightCurrentAdjustTarget = PointLightAdjust.Range;

        ///<summary>
        ///The manager of the light source's properties.
        ///</summary>
        public GameObject PropertyManager;

        [HideInInspector]
        ///<summary>
        /// Stealth Mode : The status that would be entered by pressing the menu button.
        /// The contour of the selected object & the auxiliary line would be gone.
        ///</summary>
        public bool stealthMode = false;

        /// <summary>
        /// Position : Enter Common State
        /// ShadowDrag : Enter Shadow Position State
        /// </summary>
        public IndirectIdleStateEnter currentIndirectTransferTo;

        /// <summary>
        /// Position : Enter Common State
        /// Painting : Enter Painting State
        /// </summary>
        public InverseIdleStateEnter currentInverseTransferTo;

        /// <summary>
        /// Represent the current lighting design state. Initial to "Direct".
        /// </summary>
        public LightingDesignState currentLightingDesignState;

        /// <summary>
        /// Draw Contour on the scene to emphasize the range of shadow. (Indirect Lighting Design)
        /// </summary>
        private IEnumerator DrawContourCoroutine;
        private bool coroutineEnds;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        /// 
        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += ResetOnSceneLoaded;

            currentIndirectTransferTo = IndirectIdleStateEnter.ShadowDrag;
            currentInverseTransferTo = InverseIdleStateEnter.Painting;

            currentLightingDesignState = LightingDesignState.Direct;

            InverseInterfaceAwake();
        }

        public void SetupStateMachine()
        {
            stateMachine = new StateMachine<State<LightingDesign>, LightingDesign>(this);
            // The rest is deleted.
        }

        public void UpdateStateMachine()
        {
            stateMachine.UpdateStateMachine();
        }

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            InverseInterfaceStart();
        }

        /// <summary>
        /// This function is called after a new level was loaded.
        /// </summary>
        /// <param name="level">The index of the level that was loaded.</param>
        public void ResetOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (instance == this)
            {
                stateMachine.SetInitialState("DirectIdleState");

                currentIndirectTransferTo = IndirectIdleStateEnter.ShadowDrag;
                currentInverseTransferTo = InverseIdleStateEnter.Painting;

                currentLightingDesignState = LightingDesignState.Direct;

                foreach (Light light in FindObjectsOfType<Light>())
                    InitializeLight(light);

                // Gizmo Part.
                SelectedObjects = new List<Transform>();
                selectedLights = new List<Light>();

                for (int i = 0; i < 3; i++) lightTypeCount[i] = 0;
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            UpdateStateMachine();
            /*
            if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Menu) ||
                ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Menu))
            {
                stealthMode = !stealthMode;

                ToggleSelectionRange();
                ToggleLightingGizmo();
            }*/

            //  For Direct Lighting Design
            //Would only pass if certain light source is selected.
            if (SelectedObjects.Count > 0)
            {
                // Only pick the FIRST light in the container for processing.
                Light currentLight = selectedLights[0];

            }
            /*
            if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Pad))
            {
                // Change mode for spotlight
                switch (spotLightCurrentAdjustTarget)
                {
                    case SpotLightAdjust.Angle:
                        spotLightCurrentAdjustTarget = SpotLightAdjust.Range;
                        break;
                    case SpotLightAdjust.Range:
                        spotLightCurrentAdjustTarget = SpotLightAdjust.Orientation;
                        break;
                    case SpotLightAdjust.Orientation:
                        spotLightCurrentAdjustTarget = SpotLightAdjust.Angle;
                        break;
                }
                //  Change mode for pointlight
                switch(pointLightCurrentAdjustTarget)
                {
                    case PointLightAdjust.Range:
                        pointLightCurrentAdjustTarget = PointLightAdjust.Orientation;
                        break;
                    case PointLightAdjust.Orientation:
                        pointLightCurrentAdjustTarget = PointLightAdjust.Range;
                        break;
                }
            }
            */
            InverseInterfaceUpdate();
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        void OnDisable()
        {
            SceneManager.sceneLoaded -= ResetOnSceneLoaded;
        }

        public static void SetLayerRecursively(GameObject go, int layerMask)
        {
            go.layer = layerMask;
            foreach (Transform transform in go.GetComponentsInChildren<Transform>(true))
            {
                transform.gameObject.layer = layerMask;
            }
        }

        /// <summary>
        /// Transit to specific state.
        /// </summary>
        /// /// <param name="stateName">The name of the state which is about to transit to.</param>
        public void TransitToState(string stateName)
        {
            stateMachine.TransitToState(stateName);
        }

        /// <summary>
        /// Check whether the current state matches the stateName.
        /// </summary>
        /// <param name="stateName">The name of the state which is about to compare.</param>
        /// <returns>Is true when the current state matches the stateName.</returns>
        public bool IsCurrentState(string stateName)
        {
            return stateMachine.IsCurrentState(stateName);
        }

        /// <summary>
        /// Can only be called if there exists a selected light. (the size of the selectedObject should be examine first.)
        /// </summary>
        /// <returns></returns>
        public LightType GizmoCurrentLightType()
        {
            return selectedLights[0].type;
        }

        /// <summary>
        /// Can only be called if there exists a selected spotlight. (should make sure the GizmoCurrentLightType() has been called.)
        /// </summary>
        /// <returns></returns>
        public SpotLightAdjust SpotLightGizmoCurrentAdjust()
        {
            return spotLightCurrentAdjustTarget;
        }

        /// <summary>
        /// Can only be called if there exists a selected pointlight. (should make sure the GizmoCurrentLightType() has been called.)
        /// </summary>
        /// <returns></returns>
        public PointLightAdjust PointLightGizmoCurrentAdjust()
        {
            return pointLightCurrentAdjustTarget;
        }

        public void UpdateCenter()
        {
            if (SelectedObjects[0])
            {
                transform.position = SelectedObjects[0].parent.position;
                transform.rotation = SelectedObjects[0].parent.rotation;
            }
        }

        public void ClearSelection()
        {

            SelectedObjects.Clear();
            selectedLights.Clear();

            for (int i = 0; i < 3; i++) lightTypeCount[i] = 0;
        }

        public void SelectObject(Transform _parent)
        {

            if (!SelectedObjects.Contains(_parent))
            {
                SelectedObjects.Add(_parent);
                selectedLights.Add(_parent.parent.GetComponent<Light>());
            }

            UpdateCenter();

            Light _light = _parent.parent.GetComponent<Light>();

            //  Remove when the multiple selection function is construct.
            for (int i = 0; i < 3; i++) lightTypeCount[i] = 0;

            if (enableLightGizmo)
            {
                if (_light.type == LightType.Point)
                {
                    lightTypeCount[1] = 1;
                }
                else if (_light.type == LightType.Spot)
                {
                    lightTypeCount[2] = 1;
                }
                else { lightTypeCount[0] = 1; }
            }

        }

        /// <summary>
        /// Initialize the light for lighting design.
        /// </summary>
        public void InitializeLight(Light light)
        {
            GameObject rangeObject, rangePrefab = null;

            switch (light.type)
            {
                case LightType.Spot:
                    rangePrefab = selectionRangeSpotlightPrefab;
                    break;
                case LightType.Directional:
                    rangePrefab = selectionRangeDirectionalLightPrefab;
                    break;
                case LightType.Point:
                    rangePrefab = selectionRangePointLightPrefab;
                    break;
                default:
                    break;
            }

            rangeObject = (GameObject)Instantiate(rangePrefab, light.transform, false);
            selectionRangeInScene.Add(rangeObject);

            // Update light fixture material (write this way because can't change material when undo and redo)
            MeshRenderer lightRenderer;
            lightRenderer = rangeObject.GetComponent<MeshRenderer>();
            if (lightRenderer)
                lightRenderer.material.SetColor("_EmissionColor", light.color);

            //Unify all selection ranges' scale.
            if (light.transform.parent != null)
            {
                rangeObject.transform.localScale = new Vector3(rangeObject.transform.localScale.x / light.transform.parent.localScale.x,
                                                        rangeObject.transform.localScale.y / light.transform.parent.localScale.y,
                                                        rangeObject.transform.localScale.z / light.transform.parent.localScale.z);
            }
            //SetLayerRecursively(light.gameObject, LayerMask.NameToLayer("Player"));
        }

        /// <summary>
        /// Add a light in front of the main camera.
        /// </summary>
        /// <param name="type">The light type.</param>
        public void AddLight(int type)
        {
            GameObject lightObject = new GameObject("AddedLightObject");
            Light light = lightObject.AddComponent<Light>();

            light.range = 10.0f;
            light.type = (LightType)type;
            light.shadows = LightShadows.Soft;
            if ((LightType)type == LightType.Point)
            {
                light.shadowNearPlane = 0.1f; // fix object's shadow that is too near.
                light.bounceIntensity = 1.0f;
            }
            else if ((LightType)type == LightType.Spot)
            {
                light.shadowNearPlane = 0.5f; // fix object's shadow that is too near.
                light.bounceIntensity = 1.0f;
            }
            lightObject.transform.position = VRManager.instance.headset.transform.position + VRManager.instance.headset.transform.forward * addLightDistance;
            lightObject.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            //lightObject.AddComponent<LumenLight>();
            InitializeLight(light);
            LightingManager.instance.SaveLightingSetup();
        }

        /// <summary>
        /// Add a light in copy way.
        /// </summary>
        /// <param name="light">The light to copy.</param>
        public void AddLight(Light light)
        {
            GameObject lightObject = new GameObject("AddedLightObject");
            Light copiedLight = lightObject.AddComponent<Light>();

            copiedLight.name = light.name;
            copiedLight.color = light.color;
            copiedLight.intensity = light.intensity;
            copiedLight.range = light.range;
            copiedLight.spotAngle = light.spotAngle;
            copiedLight.type = light.type;
            copiedLight.shadows = light.shadows;
            if (light.type == LightType.Point)
            {
                copiedLight.shadowNearPlane = 0.1f; // fix object's shadow that is too near.
                copiedLight.bounceIntensity = 0.5f;
            }
            else if (light.type == LightType.Spot)
            {
                copiedLight.shadowNearPlane = 0.5f; // fix object's shadow that is too near.
                copiedLight.bounceIntensity = 1.0f;
            }
            lightObject.transform.position = light.transform.position;
            lightObject.transform.rotation = light.transform.rotation;
            //lightObject.AddComponent<LumenLight>();
            InitializeLight(copiedLight);
            LightingManager.instance.SaveLightingSetup();
        }

        /// <summary>
        /// Delete a light object.
        /// </summary>
        /// <param name="lightObject">The light's object to delete.</param>
        public void DeleteLight(GameObject lightObject)
        {
            ClearSelection();

            selectionRangeInScene.Remove( lightObject.transform.GetChild(0).gameObject );
            selectedLights.Remove(lightObject.GetComponent<Light>());
            SelectedObjects.Remove(lightObject.transform);

            DestroyImmediate(lightObject); // Destroy immediately to prevent wrong behaviour.
        }


        /// <summary>
        /// Update a light fixture's material according to its light color.
        /// </summary>
        /// <param name="lightToUpdate">A light to update.</param>
        public void UpdateLightFixtureMaterial(Light lightToUpdate)
        {
            MeshRenderer lightRenderer;

            lightRenderer = lightToUpdate.GetComponentInChildren<MeshRenderer>();
            if (lightRenderer)
                lightRenderer.material.SetColor("_EmissionColor", lightToUpdate.color);
        }

        /// <summary>
        /// Update a light fixture's material according to its light color.
        /// </summary>
        /// <param name="lightToUpdate">A list of lights to update.</param>
        public void UpdateLightFixtureMaterial(List<Light> lightToUpdate)
        {
            MeshRenderer lightRenderer;

            for (int i = 0; i < lightToUpdate.Count; i++)
            {
                lightRenderer = lightToUpdate[i].GetComponentInChildren<MeshRenderer>();
                if (lightRenderer)
                    lightRenderer.material.SetColor("_EmissionColor", lightToUpdate[i].color);
            }
        }

        /// <summary>
        /// To toggle all selecitonRanges.
        /// </summary>
        void ToggleSelectionRange()
        {
            //  Only for the directional light. ( spotlight & point light source would always stay active. )
           
            for (int i = 0; i < selectionRangeInScene.Count; ++i)
            {
                if( selectionRangeInScene[i].transform.parent.GetComponent<Light>().type == LightType.Directional )
                {
                    if( selectionRangeInScene[i].activeInHierarchy )
                        selectionRangeInScene[i].SetActive(false);
                    else
                        selectionRangeInScene[i].SetActive(true);
                }
                    
            }
                             
        }

        void ToggleLightingGizmo()
        {
            foreach(Transform child in gameObject.transform)
            {
                if (child.gameObject.activeInHierarchy) child.gameObject.SetActive(false);
                else child.gameObject.SetActive(true);
            }
        }

        public bool ExistSelectedDirectionalLight() { return lightTypeCount[0] > 0;  }
        public bool ExistSelectedPointLight() { return lightTypeCount[1] > 0; }
        public bool ExistSelectedSpotLight() { return lightTypeCount[2] > 0; }

        public void DrawContour(Light _light)
        {
            DrawContourCoroutine = Sparkle(_light);
            coroutineEnds = false;
            StartCoroutine( DrawContourCoroutine );
        }

        public void ClearContour(Light _light, Color c)
        {
            coroutineEnds = true;

            StopCoroutine(DrawContourCoroutine);
            _light.color = c;            
        }

        public bool CoroutineRunning() { return coroutineEnds; }

        private IEnumerator Sparkle(Light _light)
        {
            Color originColor = _light.color;
            Color currentColor = _light.color;

            float[] ratio = { 10.0f, 9.5f, 9.0f, 8.5f, 8.0f, 7.5f, 7.0f, 6.5f, 7.0f, 7.5f, 8.0f, 8.5f, 9.0f, 9.5f, 10.0f };
            while(true)
            {
                for(int j=0;j<ratio.Length;j++)
                {
                    yield return new WaitForSeconds(0.1f);
                    float[] rgb = { originColor.r / 10 * ratio[j], originColor.g / 10 * ratio[j], originColor.b / 10 * ratio[j] };
                    currentColor.r = rgb[0];
                    currentColor.g = rgb[1];
                    currentColor.b = rgb[2];

                    _light.color = currentColor;
                }
            }
        }

        
    }
}

