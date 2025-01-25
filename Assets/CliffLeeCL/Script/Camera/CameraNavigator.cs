using UnityEngine;
using System.Collections;

namespace CliffLeeCL
{
    /// <summary>
    /// The class is used to move a ojbect like the scene camera in the Unity editor.
    /// </summary>
    public class CameraNavigator : MonoBehaviour
    {
        /// <summary>
        /// Define the speed of rotation.
        /// </summary>
        public float angularSpeed = 10.0f;
        /// <summary>
        /// Define the speed of default.
        /// </summary>
        public float moveSpeed = 1.0f;
        /// <summary>
        /// Define a speed is used when the user want to move faster.
        /// </summary>
        public float sprintSpeed = 2.0f;
        /// <summary>
        /// Define the key that is used for sprint;
        /// </summary>
        public KeyCode sprintKey = KeyCode.LeftShift;
        /// <summary>
        /// Define the key to move vertically up. 
        /// </summary>
        public KeyCode PedestalUpKey = KeyCode.E;
        /// <summary>
        /// Define the key to move vertically down. 
        /// </summary>
        public KeyCode PedestalDownKey = KeyCode.Q;

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
            Move();
            Rotate();
        }

        /// <summary>
        /// Move the camera with the user input.
        /// </summary>
        void Move()
        {
            float inputHorizontal = Input.GetAxis("Horizontal");
            float inputVertical = Input.GetAxis("Vertical");
            float inputMouseX = Input.GetAxis("Mouse X");
            float inputMouseY = Input.GetAxis("Mouse Y");
            Vector3 moveDirection = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;
            Vector3 truckDirection = new Vector3(inputMouseX, inputMouseY, 0.0f).normalized;
            bool isSprinting = Input.GetKey(sprintKey);
            bool isMiddleMouseButtonPressed = Input.GetMouseButton(2);
            bool isPedestalUp = Input.GetKey(PedestalUpKey);
            bool isPedestalDown = Input.GetKey(PedestalDownKey);

            transform.Translate(moveDirection * (isSprinting ? sprintSpeed : moveSpeed) * Time.deltaTime, Space.Self);

            if (isPedestalUp)
                transform.Translate(Vector3.up * (isSprinting ? sprintSpeed : moveSpeed) * Time.deltaTime, Space.Self);

            if (isPedestalDown)
                transform.Translate(Vector3.down * (isSprinting ? sprintSpeed : moveSpeed) * Time.deltaTime, Space.Self);

            if (isMiddleMouseButtonPressed)
                transform.Translate(truckDirection * (isSprinting ? sprintSpeed : moveSpeed) * Time.deltaTime, Space.Self);
        }

        /// <summary>
        /// Rotate the camera with the user input.
        /// </summary>
        void Rotate()
        {
            float inputMouseX = Input.GetAxis("Mouse X");
            float inputMouseY = Input.GetAxis("Mouse Y");
            bool isRightMouseButtonPressed = Input.GetMouseButton(1);

            if (isRightMouseButtonPressed)
            {
                transform.Rotate(-inputMouseY * angularSpeed * Time.deltaTime, inputMouseX * angularSpeed * Time.deltaTime, 0, Space.Self);
                transform.localEulerAngles = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0.0f);
            }
        }
    }
}
