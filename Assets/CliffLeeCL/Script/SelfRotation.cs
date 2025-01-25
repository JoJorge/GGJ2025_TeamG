using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace CliffLeeCL
{
    /// <summary>
    /// The class will make object keep rotating.
    /// </summary>
    public class SelfRotation : MonoBehaviour
    {
        /// <summary>
        /// Define how fast the object rotates.
        /// </summary>
        [Tooltip("(Degree/Second)")]
        public float angularSpeed = 30.0f;

        Rigidbody rigid;

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            rigid = GetComponent<Rigidbody>();
            Assert.IsTrue(rigid, "Need \"Rigidbody\" component on this gameObject");
        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            rigid.MoveRotation(transform.rotation * Quaternion.AngleAxis(angularSpeed * Time.deltaTime, transform.up));
        }
    }

}
