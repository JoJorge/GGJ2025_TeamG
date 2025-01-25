using UnityEngine;
using System;
using System.Collections.Generic;

namespace CliffLeeCL
{
    /// <summary>
    /// The class define the lighting setup's data.
    /// </summary>
    [Serializable]
    public class LightingSetup
    {
        /// <summary>
        /// The list stores all lights' properties.
        /// </summary>
        public List<LightProperty> lightPorpertyList = new List<LightProperty>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LightingSetup(){
            lightPorpertyList.Clear();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="setup">The setup to be copied.</param>
        public LightingSetup(LightingSetup setup)
        {
            for(int i = 0; i < setup.lightPorpertyList.Count; i++)
            {
                LightProperty property = new LightProperty();

                property.name = setup.lightPorpertyList[i].name;
                property.position = setup.lightPorpertyList[i].position;
                property.rotation = setup.lightPorpertyList[i].rotation;
                property.color = setup.lightPorpertyList[i].color;
                property.intensity = setup.lightPorpertyList[i].intensity;
                property.range = setup.lightPorpertyList[i].range;
                property.spotAngle = setup.lightPorpertyList[i].spotAngle;
                property.lightType = setup.lightPorpertyList[i].lightType;
                property.shadowType = setup.lightPorpertyList[i].shadowType;

                lightPorpertyList.Add(property);
            }
        }
    }

    [Serializable]
    public class LightProperty
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Color color;
        public float intensity;
        public float range;
        public float spotAngle;
        public LightType lightType;
        public LightShadows shadowType;
    }
}

