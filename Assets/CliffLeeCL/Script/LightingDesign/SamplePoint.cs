using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// The class store mesh's surface attribute for inverse lighting.
    /// </summary>
    public class SamplePoint
    {
        /// <summary>
        /// Property to access goalIllumination. will set pointMaterial in setter.
        /// </summary>
        public Color GoalIllumination
        {
            get
            {
                return goalIllumination;
            }
            set
            {
                goalIllumination = value;
                if (pointRenderer)
                {
                    propBlock.SetColor("_Color", value);
                    pointRenderer.SetPropertyBlock(propBlock);
                }
            }
        }
        /// <summary>
        /// Property to access point's weight, will get value from InverseLightingSolver.
        /// </summary>
        public float Weight
        {
            get
            {
                 return isPainted() ? InverseLightingSolver.instance.paintedWeight : InverseLightingSolver.instance.unpaintedWeight;
            }
        }
        /// <summary>
        /// World position of the sample point.
        /// </summary>
        public Vector3 worldPosition;
        /// <summary>
        /// World normal of the sample point.
        /// </summary>
        public Vector3 worldNormal;
        /// <summary>
        /// Texture coordinate of the sample point.
        /// </summary>
        public Vector2 texCoord;
        /// <summary>
        /// The material of the mesh which the sample point was generated on.
        /// </summary>
        public Material meshMaterial;
        /// <summary>
        /// The diffuse coefficient of the sample point. (needs texCoord and meshMeaterial)
        /// </summary>
        public Color Kd;
        /// <summary>
        /// Current illumination of this point.
        /// </summary>
        public Color currentIllumination;
        /// <summary>
        /// paintingStatus >0 -> isPainted. (isPainted == 1 -> editable)
        /// </summary>
        public int paintingStatus;
        /// <summary>
        /// Is true when the sample point's token is always visible. (for debugging)
        /// </summary>
        public bool isAlwaysVisible;

        /// <summary>
        /// The token for visualizing sample point.
        /// </summary>
        private GameObject token;
        /// <summary>
        /// The renderer of the sample point. (for visualizing sample point)
        /// </summary>
        private Renderer pointRenderer;
        /// <summary>
        /// Is used for changing color in GPU instancing.
        /// </summary>
        private MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        
        /// <summary>
        /// Goal illumination color of this point.
        /// </summary>
        private Color goalIllumination;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SamplePoint()
        {
            worldPosition = Vector3.zero;
            worldNormal = Vector3.zero;
            texCoord = Vector2.zero;
            meshMaterial = null;
            Kd = Color.black;
            currentIllumination = Color.white;
            paintingStatus = 0;
            isAlwaysVisible = false;

            token = null;
            pointRenderer = null;
            goalIllumination = Color.white;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="instanceToCopy">The instace to copy with.</param>
        public SamplePoint(SamplePoint instanceToCopy)
        {
            
            worldPosition = instanceToCopy.worldPosition;
            worldNormal = instanceToCopy.worldNormal;
            texCoord = instanceToCopy.texCoord;
            meshMaterial = instanceToCopy.meshMaterial;
            Kd = instanceToCopy.Kd;
            currentIllumination = instanceToCopy.currentIllumination;
            paintingStatus = instanceToCopy.paintingStatus;

            isAlwaysVisible = instanceToCopy.isAlwaysVisible;

            token = instanceToCopy.token;
            pointRenderer = instanceToCopy.pointRenderer;
            goalIllumination = instanceToCopy.goalIllumination;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~SamplePoint()
        {
            meshMaterial = null;
            pointRenderer = null;
            token = null;
        }

        /// <summary>
        /// Set token object's visibility.
        /// </summary>
        public void SetTokenVisibility(bool isVisible)
        {
            if(token)
                if(isAlwaysVisible && !token.activeInHierarchy)
                    token.SetActive(true);
                else if (!isAlwaysVisible && token.activeInHierarchy != isVisible)
                    token.SetActive(isVisible);
        }

        /// <summary>
        /// Activate / Deactivate token object.
        /// </summary>
        public void ToggleTokenVisibility()
        {
            if (token)
                token.SetActive(!token.activeInHierarchy);
        }

        /// <summary>
        /// Set token.
        /// </summary>
        /// <param name="obj"></param>
        public void SetToken(GameObject obj)
        {
            token = obj;
        }

        /// <summary>
        /// Set point material.
        /// </summary>
        /// <param name="mat"></param>
        public void SetPointRenderer(Renderer render)
        {
            pointRenderer = render;
        }

        /// <summary>
        /// Compute Kd for the sample point.
        /// </summary>
        public void ComputeKd()
        {
            Kd = meshMaterial.GetColor("_Color");

            if (meshMaterial.mainTexture)
            {
                Texture2D mainTexure = (Texture2D)meshMaterial.mainTexture;
                Color texelColor = mainTexure.GetPixelBilinear(texCoord.x, texCoord.y);
                Kd *= texelColor;
            }
        }

        public bool isPainted()
        {
            return paintingStatus > 0;
        }

        public bool isEditable()
        {
            return paintingStatus == 1;
        }

    }
}
