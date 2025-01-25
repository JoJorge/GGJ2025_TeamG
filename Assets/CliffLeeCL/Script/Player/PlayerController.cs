using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace CliffLeeCL
{
    /// <summary>
    /// Control player's movement and rotation by user's input.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Define the viewpoint mode the class supports.
        /// </summary>
        public enum Viewpoint
        {
            FirstPerson,
            ThirdPersonFixed,
            ThirdPersonTracking
        };
        /// <summary>
        /// Define the name for input horizontal axis.
        /// </summary>
        public string horizontalAxisName = "Horizontal";
        /// <summary>
        /// Define the name for input vertical axis.
        /// </summary>
        public string verticalAxisName = "Vertical";
        /// <summary>
        /// Current viewpoint mode. The variable will affect controller's behaviour.
        /// </summary>
        public Viewpoint viewpointMode;
        /// <summary>
        /// Use this gameobject to contorl camera X axis rotation.
        /// </summary>
        public GameObject cameraHolder;
        /// <summary>
        /// Define how much can camera holder rotate.
        /// </summary>
        public IntervalFloat cameraRotationXAxis;
        /// <summary>
        /// Decide whether the player can sprint or not.
        /// </summary>
        public bool canSprint = true;
        /// <summary>
        /// Define the key that is used for sprint;
        /// </summary>
        public KeyCode sprintKey = KeyCode.LeftShift;
        /// <summary>
        /// Is used to emit sprint effect particle.
        /// </summary>
        public ParticleSystem sprintEffect;
        /// <summary>
        /// Decide whether the player can jump or not.
        /// </summary>
        public bool canJump = true;

        /// <summary>
        /// Define where the checker should be relatively to player.
        /// </summary>
        [Header("Ground check")]
        public Vector3 checkerPositionOffset = Vector3.zero;
        /// <summary>
        /// Define how large the checker is.
        /// </summary>
        public float checkerRadius = 0.1f;
        /// <summary>
        /// Define how many layers the checker will check.
        /// </summary>
        public LayerMask checkLayer;

        
        [Header("FOV transition")]
        /// <summary>
        /// Define normal FOV of camera.
        /// </summary>
        public float normalFOV = 60.0f;
        /// <summary>
        /// Define FOV of camera when the player is sprinting.
        /// </summary>
        public float sprintFOV = 65.0f;
        /// <summary>
        /// Define the transition time between FOVs.
        /// </summary>
        public float FOVTransitionTime = 0.5f;
        /// <summary>
        /// Time that transition between normalFOV to sprintFOV.
        /// </summary>
        float normalToSprintTime = 0.0f;
        /// <summary>
        /// Time that transition between sprintFOV to normalFOV.
        /// </summary>
        float sprintToNormalTime = 0.0f;

        Rigidbody rigid;
        /// <summary>
        /// Is used to get movement related data.
        /// </summary>
        PlayerStatus status;
        /// <summary>
        /// Is true when player is on the ground.
        /// </summary>
        bool isGrounded = true;
        /// <summary>
        /// Is true when player is sprinting.
        /// </summary>
        bool isSprinting = false;
        /// <summary>
        /// Is true when player drain his stamina.
        /// </summary>
        bool isDrained = false;

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            rigid = GetComponent<Rigidbody>();
            Assert.IsTrue(rigid, "Need \"Rigidbody\" component on this gameObject");
            status = GetComponent<PlayerStatus>();
            Assert.IsTrue(status, "Need \"PlayerStatus\" component on this gameObject");

            status.currentStamina = status.maxStamina;
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            UpdateSprintEffect();
            UpdateStamina();
            if (viewpointMode != Viewpoint.ThirdPersonFixed)
            {
                UpdateCameraFOV();
                UpdateCameraRotation();
            }
        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            UpdateMovement();
            UpdateIsGrounded();
            UpdateJump();
            UpdateRotation();
        }

        /// <summary>
        /// The function is used to emit sprint effect paticle when the player is sprinting.
        /// </summary>
        void UpdateSprintEffect()
        {
            if (isSprinting)
            {
                if (sprintEffect)
                    sprintEffect.Play();
            }
            else
            {
                if (sprintEffect)
                    sprintEffect.Stop();
            }
        }

        /// <summary>
        /// Recover or decrease player's stamina in conditions.
        /// </summary>
        void UpdateStamina()
        {
            if (isSprinting)
            {
                if (status.currentStamina > 0.0f)
                    status.currentStamina -= status.staminaLossPerSecond * Time.deltaTime;
                else
                {
                    status.currentStamina = 0.0f;
                    isDrained = true;
                    StartCoroutine("Rest", status.restTimeWhenDrained);
                }
            }
            else if(!isDrained) // Have to rest a while if the player is drained.
            {
                if (status.currentStamina < status.maxStamina)
                    status.currentStamina += status.staminaRecoveryPerSecond * Time.deltaTime;
                else
                    status.currentStamina = status.maxStamina;
            }
        }

        /// <summary>
        /// Coroutine that will makes the player to wait for restTime to recover its stamina.
        /// </summary>
        /// <param name="restTime">The time the player needs to rest.</param>
        /// <returns>Interface that all coroutines use.</returns>
        IEnumerator Rest(float restTime)
        {
            yield return new WaitForSeconds(restTime);
            isDrained = false;
        }

        /// <summary>
        /// Transition between different FOVs if needed.
        /// </summary>
        private void UpdateCameraFOV()
        {
            if (isSprinting && (Camera.main.fieldOfView < sprintFOV))
            {
                sprintToNormalTime = 0.0f;
                normalToSprintTime += Time.deltaTime;
                //Not good but enough.
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sprintFOV, normalToSprintTime / FOVTransitionTime);
            }
            else if(!isSprinting && (Camera.main.fieldOfView > normalFOV))
            {
                normalToSprintTime = 0.0f;
                sprintToNormalTime += Time.deltaTime;
                //Not good but enough.
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, normalFOV, sprintToNormalTime / FOVTransitionTime);
            }
        }

        /// <summary>
        /// Update camera's rotation via euler angle in X axis.
        /// </summary>
        private void UpdateCameraRotation()
        {
            float inputMouseY = Input.GetAxis("Mouse Y");
            float newAngle = cameraHolder.transform.localEulerAngles.x - inputMouseY * status.angularSpeed * Time.deltaTime;

            if (newAngle > 180.0f)
                newAngle -= 360.0f;

            if ((newAngle >= cameraRotationXAxis.minimun) && (newAngle <= cameraRotationXAxis.maximun))
                cameraHolder.transform.localEulerAngles = new Vector3(newAngle, 0.0f, 0.0f);
        }

        /// <summary>
        /// Update player's movement via assigning new velocity.
        /// </summary>
        private void UpdateMovement()
        {
            float inputHorizontal = Input.GetAxis(horizontalAxisName);
            float inputVertical = Input.GetAxis(verticalAxisName);

            if (!Mathf.Approximately(inputHorizontal, 0.0f) || !Mathf.Approximately(inputVertical, 0.0f))
            {
                Vector3 direction = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;
                Vector3 moveVelocity;
                Vector3 sprintVelocity;
                bool isSprintKeyPressed = Input.GetKey(sprintKey);

                if (viewpointMode != Viewpoint.ThirdPersonFixed)
                {
                    moveVelocity = (transform.TransformDirection(direction) * status.movingSpeed * Time.fixedDeltaTime);
                    sprintVelocity = (transform.TransformDirection(direction) * status.sprintSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    moveVelocity = direction * status.movingSpeed * Time.fixedDeltaTime;
                    sprintVelocity = direction * status.sprintSpeed * Time.fixedDeltaTime;
                }

                if (canSprint && isSprintKeyPressed && (status.currentStamina > 0))
                {
                    Vector3 newPosition = rigid.position + sprintVelocity;

                    if (viewpointMode == Viewpoint.ThirdPersonFixed)
                    {
                        Quaternion newRotation = Quaternion.LookRotation((newPosition - rigid.position).normalized);
                        rigid.MoveRotation(newRotation);
                    }
                    rigid.MovePosition(newPosition);
                    if (sprintVelocity.sqrMagnitude > Mathf.Epsilon)// Is really moving.
                        isSprinting = true;
                    else
                        isSprinting = false;
                }
                else
                {
                    Vector3 newPosition = rigid.position + moveVelocity;

                    if (viewpointMode == Viewpoint.ThirdPersonFixed)
                    {
                        Quaternion finalRotation = Quaternion.LookRotation((newPosition - rigid.position).normalized);
                        Quaternion newRotation = Quaternion.RotateTowards(rigid.rotation, finalRotation, status.angularSpeed);
                        rigid.MoveRotation(newRotation);
                    }
                    rigid.MovePosition(newPosition);                 
                    isSprinting = false;
                }
            }
        }

        /// <summary>
        /// Check whether the player is grounded.
        /// </summary>
        private void UpdateIsGrounded()
        {
            if (Physics.CheckSphere(transform.position + checkerPositionOffset, checkerRadius, checkLayer, QueryTriggerInteraction.Ignore))
                isGrounded = true;
            else
                isGrounded = false;
        }

        /// <summary>
        /// Update player's jump via adding impulse force.
        /// </summary>
        private void UpdateJump()
        {
            bool isJumpping = Input.GetButtonDown("Jump"); 

            if(canJump && isGrounded && isJumpping)
            {
                rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, 0f, rigid.linearVelocity.z);
                rigid.AddForce(new Vector3(0f, status.jumpForce, 0f), ForceMode.Impulse);
                isGrounded = false;
            }
        }

        /// <summary>
        /// Update player's rotation via quaternion in Y axis.
        /// </summary>
        private void UpdateRotation()
        {
            if (viewpointMode != Viewpoint.ThirdPersonFixed)
            {
                float inputMouseX = Input.GetAxis("Mouse X");

                rigid.MoveRotation(rigid.rotation * Quaternion.AngleAxis(inputMouseX * status.angularSpeed * Time.fixedDeltaTime, transform.up));
            }
        }

        /// <summary>
        /// Implement this OnDrawGizmosSelected if you want to draw gizmos only if the object is selected.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + checkerPositionOffset, checkerRadius);
        }
    }
}
