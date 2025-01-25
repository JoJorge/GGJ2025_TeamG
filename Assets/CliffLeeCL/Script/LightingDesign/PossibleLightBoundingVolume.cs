using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// A class to define a bounding volume for possible lights.
    /// </summary>
    public class PossibleLightBoundingVolume : MonoBehaviour
    {
        /// <summary>
        /// Is true when we want to preview light positions on scene window.
        /// </summary>
        public bool canShowLightPosition = true;
        /// <summary>
        /// Is true when we want to preview coarse light's positions.
        /// </summary>
        public bool canShowCoarseLightPosition = true;
        /// <summary>
        /// Is true when we want to preview spread light's positions.
        /// </summary>
        public bool canShowSpreadLightPositoin = true;
        /// <summary>
        /// How many lights on X, Y, Z direction in the area.(use int only!)
        /// </summary>
        public Vector3 lightNumberInVolume = Vector3.one * 2;
        /// <summary>
        /// The size of constraint cube area.
        /// </summary>
        public Vector3 volumeSize = Vector3.one;
        /// <summary>
        /// For previewing possible light's position.
        /// </summary>
        public float previewSphereRadius = 0.1f;

        /// <summary>
        /// A point where the light position computation start from.
        /// </summary>
        public Vector3 StartPosition
        {
            get
            {
                return new Vector3(transform.position.x - volumeSize.x / 2,
                                   transform.position.y - volumeSize.y / 2,
                                   transform.position.z - volumeSize.z / 2);
            }
        }

        /// <summary>
        /// A offset that is used to compute interval between lights.
        /// </summary>
        public Vector3 Offset
        {
            get
            {
                return new Vector3(volumeSize.x / ((int)lightNumberInVolume.x),
                                   volumeSize.y / ((int)lightNumberInVolume.y),
                                   volumeSize.z / ((int)lightNumberInVolume.z));
            }
        }

        [HideInInspector]
        /// <summary>
        /// All possible coarse light's poistions.
        /// </summary>
        public List<Vector3> possibleCoarseLightPosition;
        [HideInInspector]
        /// <summary>
        /// All possible coarse light's poistions.
        /// </summary>
        public List<Vector3> possibleSpreadLightPosition;

        private void Awake()
        {
            possibleCoarseLightPosition = GetPossibleCoarseLightPosition();
            possibleSpreadLightPosition = GetPossibleSpreadLightPosition();
        }

        /// <summary>
        /// Compute and get coarse light positions in the bounding volume.
        /// </summary>
        /// <returns>Constraint light poistion list.</returns>
        public List<Vector3> GetPossibleCoarseLightPosition()
        {
            List<Vector3> coarseLightPosition = new List<Vector3>();
            Vector3 offset = Offset;

            for (int i = 0; i < lightNumberInVolume.x; i++)
                for (int j = 0; j < lightNumberInVolume.y; j++)
                    for (int k = 0; k < lightNumberInVolume.z; k++)
                        coarseLightPosition.Add(StartPosition + new Vector3(offset.x * i + (offset.x / 2) , offset.y * j + (offset.y / 2), offset.z * k + (offset.z / 2)));

            return coarseLightPosition;
        }

        /// <summary>
        /// Compute and get spread light positions in the bounding volume.
        /// </summary>
        /// <returns>Constraint light poistion list.</returns>
        public List<Vector3> GetPossibleSpreadLightPosition()
        {
            List<Vector3> coarseLightPosition = GetPossibleCoarseLightPosition();
            List<Vector3> spreadLightPosition = new List<Vector3>();
            Vector3 offsetBase = new Vector3(Offset.x / 4.0f, Offset.y / 4.0f, Offset.z / 4.0f);
            Vector3 offset = Vector3.zero;

            for (int i = 0; i < coarseLightPosition.Count; i++)
            {
                // XYZ, -X-Y-Z
                offset.Set(offsetBase.x, offsetBase.y, offsetBase.z);
                spreadLightPosition.Add(coarseLightPosition[i] + offset);
                spreadLightPosition.Add(coarseLightPosition[i] - offset);

                // -XYZ, X-Y-Z
                offset.Set(-offsetBase.x, offsetBase.y, offsetBase.z);
                spreadLightPosition.Add(coarseLightPosition[i] + offset);
                spreadLightPosition.Add(coarseLightPosition[i] - offset);

                // X-YZ, -XY-Z
                offset.Set(offsetBase.x, -offsetBase.y, offsetBase.z);
                spreadLightPosition.Add(coarseLightPosition[i] + offset);
                spreadLightPosition.Add(coarseLightPosition[i] - offset);

                // XY-Z, -X-YZ
                offset.Set(offsetBase.x, offsetBase.y, -offsetBase.z);
                spreadLightPosition.Add(coarseLightPosition[i] + offset);
                spreadLightPosition.Add(coarseLightPosition[i] - offset);
            }

            return spreadLightPosition;
        }

        void OnDrawGizmos()
        {
            if (canShowLightPosition)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position, volumeSize);
                if(canShowCoarseLightPosition)
                    DrawCoarseLightPosition();
                if (canShowSpreadLightPositoin)
                    DrawSpreadLightPosition();
            }
        }

        void DrawCoarseLightPosition()
        {
            List<Vector3> coarseLightPosition = GetPossibleCoarseLightPosition();

            Gizmos.color = Color.white;
            for (int i = 0; i < coarseLightPosition.Count; i++)
                Gizmos.DrawWireSphere(coarseLightPosition[i], previewSphereRadius);
        }

        void DrawSpreadLightPosition()
        {
            List<Vector3> spreadLightPosition = GetPossibleSpreadLightPosition();

            Gizmos.color = Color.black;
            for (int i = 0; i < spreadLightPosition.Count; i++)
                Gizmos.DrawWireSphere(spreadLightPosition[i], previewSphereRadius / 2);
        }

        void DrawLightPositionWithColor()
        {
            Color drawColor = Color.black;
            Vector3 startPosition = new Vector3(transform.position.x - volumeSize.x / 2,
                                                transform.position.y - volumeSize.y / 2,
                                                transform.position.z - volumeSize.z / 2);
            Vector3 offset = new Vector3(volumeSize.x / ((int)lightNumberInVolume.x - 1), 
                                        volumeSize.y / ((int)lightNumberInVolume.y - 1), 
                                        volumeSize.z / ((int)lightNumberInVolume.z- 1));

            for (int i = 0; i < lightNumberInVolume.x; i++)
            {
                drawColor.r = i / lightNumberInVolume.x;
                for (int j = 0; j < lightNumberInVolume.y; j++)
                {
                    drawColor.g = j / lightNumberInVolume.y;
                    for (int k = 0; k < lightNumberInVolume.z; k++)
                    {
                        drawColor.b = k / lightNumberInVolume.z;
                        Gizmos.color = drawColor;
                        Gizmos.DrawWireSphere(startPosition + new Vector3(offset.x * i, offset.y * j, offset.z * k), 0.1f);
                    }
                }
            }
        }
    }
}
