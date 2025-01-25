using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// The class define the lighting goal information fpr inverse lighting computation.
    /// </summary>
    public class LightingGoal
    {
        public List<SamplePoint> goalSamplePoint;

        public LightingGoal()
        {
            goalSamplePoint = new List<SamplePoint>();
        }

        public LightingGoal(LightingGoal instanceToCopy)
        {
            goalSamplePoint = instanceToCopy.goalSamplePoint;
        }

        ~LightingGoal()
        {
            
        }

        public void FilterInvisibleSamplePoint(Camera renderCamera)
        {
            List<SamplePoint> filteredSamplePoint = new List<SamplePoint>();
            Rect viewport = new Rect(0, 0, 1, 1);

            for (int i = 0; i < goalSamplePoint.Count; i++) {
                Vector3 viewportSpacePoint = renderCamera.WorldToViewportPoint(goalSamplePoint[i].worldPosition);

                if (viewport.Contains(viewportSpacePoint) && viewportSpacePoint.z > 0.0f) // Is visible to renderCamera.
                    filteredSamplePoint.Add(goalSamplePoint[i]);
            }

            goalSamplePoint = filteredSamplePoint;
        }

    }

    /// <summary>
    /// Store the attribute of certain vertex on mesh.
    /// </summary>
    public class MeshAttribute
    {
        public Color vertexColor;           // current vertex color of a mesh's vertex.
        public Vector3 worldVertexPosition;
        public Vector3 worldVertexNormal;
        public Vector2 texCoord;
        public Material material;

        public MeshAttribute()
        {

        }

        public MeshAttribute(MeshAttribute instanceToCopy)
        {
            vertexColor = instanceToCopy.vertexColor;
            worldVertexPosition = instanceToCopy.worldVertexPosition;
            worldVertexNormal = instanceToCopy.worldVertexNormal;
            texCoord = instanceToCopy.texCoord;
            material = instanceToCopy.material;
        }
    }
}