using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;

namespace CliffLeeCL
{
    /// <summary>
    /// The singleton class manage all lights in the scene.
    /// </summary>
    public class LightingManager : MonoBehaviour
    {
        /// <summary>
        /// The variable is used to access this class.
        /// </summary>
        public static LightingManager instance = null;

        /// <summary>
        /// Is used to know what order is the current record.
        /// </summary>
        public int currentRecordIndex = 0;

        /// <summary>
        /// Is true when the lighting setup record can be saved.
        /// </summary>
        public bool canSaveLightingSetupRecord = true;

        /// <summary>
        /// The lighting setup holds all lights.
        /// </summary>
        LightingSetup lightingSetup = new LightingSetup();
        /// <summary>
        /// Store all lighting setups created in play mode.
        /// </summary>
        public List<LightingSetup> lightingSetupRecord;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            if(instance == null)
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
                lightingSetupRecord = new List<LightingSetup>();
                currentRecordIndex = 0;
                RefreshLightingSetup();
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                SaveLightingSetup();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                // This is useless for now. The function should be used to support undo and redo.
                //LoadLightingSetup();
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
        /// Sent to all game objects before the application is quit.
        /// In the editor this is called when the user stops playmode.
        /// </summary>
        void OnApplicationQuit()
        {
            SaveLightingSetupToJSON();
        }

        /// <summary>
        /// Save lighting's setup in the local record.
        /// </summary>
        public void SaveLightingSetup()
        {
            if (canSaveLightingSetupRecord)
                RefreshLightingSetup();
        }

        /// <summary>
        /// Save lighting's setup in the JSON format.
        /// </summary>
        public void SaveLightingSetupToJSON()
        {
            RefreshLightingSetup();

            string fileText;
            string fileName = DateTime.Now.ToString("MM-dd HH_mm_ss") + " LightingSetup";

            fileText = JsonUtility.ToJson(lightingSetup, true);
            // JSON.net doesn't support Vector3 and Color
            //fileText = JsonConvert.SerializeObject(lightingSetup, Formatting.Indented); 
            File.WriteAllText(Application.dataPath + "/JSON/" + fileName + ".json", fileText);
            Debug.Log("(" + fileName + ") The lighting's setup is saved!");
        }

        /// <summary>
        /// Load lighting's setup from a JSON file.
        /// </summary>
        public void LoadLightingSetup()
        {
            string fileText = File.ReadAllText(Application.dataPath + "/JSON/LightingSetup.json");
            Light[] lightInScene = FindObjectsOfType<Light>();

            lightingSetup = JsonConvert.DeserializeObject<LightingSetup>(fileText);
            // Inactive all lights in the scene.
            foreach(Light light in lightInScene)
            {
                light.gameObject.SetActive(false);
            }
            LightingDesign.instance.ClearSelection();

            // Apply loaded lighting setup.
            for (int i = 0; i < lightingSetup.lightPorpertyList.Count; i++)
            {
                if(i < lightInScene.Length) // Overwrite current lights.
                {
                    lightInScene[i].gameObject.SetActive(true);
                    lightInScene[i].name = lightingSetup.lightPorpertyList[i].name;
                    lightInScene[i].transform.position = lightingSetup.lightPorpertyList[i].position;
                    lightInScene[i].transform.rotation = lightingSetup.lightPorpertyList[i].rotation;
                    lightInScene[i].color = lightingSetup.lightPorpertyList[i].color;
                    lightInScene[i].intensity = lightingSetup.lightPorpertyList[i].intensity;
                    lightInScene[i].range = lightingSetup.lightPorpertyList[i].range;
                    lightInScene[i].spotAngle = lightingSetup.lightPorpertyList[i].spotAngle;
                    if (lightInScene[i].type != lightingSetup.lightPorpertyList[i].lightType)
                    {
                        foreach (Transform child in lightInScene[i].transform)
                            Destroy(child.gameObject);
                        lightInScene[i].type = lightingSetup.lightPorpertyList[i].lightType;
                        LightingDesign.instance.InitializeLight(lightInScene[i]);
                    }
                    lightInScene[i].shadows = lightingSetup.lightPorpertyList[i].shadowType;
                }
                else // Create new lights.
                {
                    GameObject lightObject = new GameObject("LightObject" + i);
                    Light light = lightObject.AddComponent<Light>();

                    light.name = lightingSetup.lightPorpertyList[i].name;
                    light.transform.position = lightingSetup.lightPorpertyList[i].position;
                    light.transform.rotation = lightingSetup.lightPorpertyList[i].rotation;
                    light.color = lightingSetup.lightPorpertyList[i].color;
                    light.intensity = lightingSetup.lightPorpertyList[i].intensity;
                    light.range = lightingSetup.lightPorpertyList[i].range;
                    light.spotAngle = lightingSetup.lightPorpertyList[i].spotAngle;
                    light.type = lightingSetup.lightPorpertyList[i].lightType;
                    light.shadows = lightingSetup.lightPorpertyList[i].shadowType;
                    LightingDesign.instance.InitializeLight(lightInScene[i]);
                }
            }
            Debug.Log("The lighting's setup is loaded!");
        }

        /// <summary>
        /// Load lighting's setup from a setup.
        /// </summary>
        /// <param name="loadedSetup">The setup to be loaded.</param>
        public void LoadLightingSetup(LightingSetup loadedSetup)
        {
            Light[] lightInScene = FindObjectsOfType<Light>();

            lightingSetup = new LightingSetup(loadedSetup);
            // Inactive all lights in the scene.
            foreach (Light light in lightInScene)
            {
                light.gameObject.SetActive(false);
            }
            LightingDesign.instance.ClearSelection();
            LightingDesign.instance.selectionRangeInScene.Clear();

            // Apply loaded lighting setup.
            for (int i = 0; i < lightingSetup.lightPorpertyList.Count; i++)
            {
                if (i < lightInScene.Length) // Overwrite current lights.
                {
                    lightInScene[i].gameObject.SetActive(true);
                    lightInScene[i].name = lightingSetup.lightPorpertyList[i].name;
                    lightInScene[i].transform.position = lightingSetup.lightPorpertyList[i].position;
                    lightInScene[i].transform.rotation = lightingSetup.lightPorpertyList[i].rotation;
                    lightInScene[i].color = lightingSetup.lightPorpertyList[i].color;
                    lightInScene[i].intensity = lightingSetup.lightPorpertyList[i].intensity;
                    lightInScene[i].range = lightingSetup.lightPorpertyList[i].range;
                    lightInScene[i].spotAngle = lightingSetup.lightPorpertyList[i].spotAngle;
                    lightInScene[i].type = lightingSetup.lightPorpertyList[i].lightType;
                    lightInScene[i].shadows = lightingSetup.lightPorpertyList[i].shadowType;
                    foreach (Transform child in lightInScene[i].transform) // Need to recreate range object for all lights.
                        Destroy(child.gameObject);
                    LightingDesign.instance.InitializeLight(lightInScene[i]);
                }
                else // Create new lights.
                {
                    GameObject lightObject = new GameObject("LightObject" + i);
                    Light light = lightObject.AddComponent<Light>();

                    light.name = lightingSetup.lightPorpertyList[i].name;
                    light.transform.position = lightingSetup.lightPorpertyList[i].position;
                    light.transform.rotation = lightingSetup.lightPorpertyList[i].rotation;
                    light.color = lightingSetup.lightPorpertyList[i].color;
                    light.intensity = lightingSetup.lightPorpertyList[i].intensity;
                    light.range = lightingSetup.lightPorpertyList[i].range;
                    light.spotAngle = lightingSetup.lightPorpertyList[i].spotAngle;
                    light.type = lightingSetup.lightPorpertyList[i].lightType;
                    light.shadows = lightingSetup.lightPorpertyList[i].shadowType;
                    LightingDesign.instance.InitializeLight(light);
                }
            }
            Debug.Log("The lighting's setup is loaded! currentRecordIndex: " + currentRecordIndex);
        } 

        /// <summary>
        /// Refresh lighting setup by current scene's setup.
        /// </summary>
        public void RefreshLightingSetup()
        {
            lightingSetup.lightPorpertyList.Clear();
            foreach (Light light in FindObjectsOfType<Light>())
            {
                LightProperty property = new LightProperty();

                property.name = light.name;
                property.position = light.transform.position;
                property.rotation = light.transform.rotation;
                property.color = light.color;
                property.intensity = light.intensity;
                property.range = light.range;
                property.spotAngle = light.spotAngle;
                property.lightType = light.type;
                property.shadowType = light.shadows;

                lightingSetup.lightPorpertyList.Add(property);
            }

            lightingSetupRecord.Insert(currentRecordIndex, new LightingSetup(lightingSetup));
            currentRecordIndex++;
            // Do someting after undo.
            if(currentRecordIndex != lightingSetupRecord.Count)
                lightingSetupRecord.RemoveRange(currentRecordIndex, lightingSetupRecord.Count - currentRecordIndex);
        }
    }

}
