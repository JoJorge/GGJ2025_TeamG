using System.Collections;
using UnityEngine.Assertions;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// Control player's movement and rotation by user's input.
    /// </summary>
    public class PlayerControllerMVC : BaseController<PlayerModel>
    {   
        /// <summary>
        /// Use this gameobject to contorl camera X axis rotation.
        /// </summary>
        public GameObject cameraHolder;

        public Transform playerTransform;
        public Rigidbody playerRigid;

        /// <summary>
        /// Is true when the sprint key is pressed.
        /// </summary>
        [HideInInspector]
        public bool isSprintKeyPressed = false;

        /// <summary>
        /// Is true when player is on the ground.
        /// </summary>
        bool isGrounded = true;
        /// <summary>
        /// Is true when player drain his stamina.
        /// </summary>
        bool isDrained = false;

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            model.playerStatus.currentStamina = model.playerStatus.maxStamina;
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            UpdateStamina();
        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            UpdateIsGrounded();
        }

        /// <summary>
        /// Update camera's rotation via euler angle in X axis.
        /// </summary>
        public void UpdateCameraRotation(float inputMouseY)
        {
            float newAngle = cameraHolder.transform.localEulerAngles.x - inputMouseY * model.playerStatus.angularSpeed * Time.deltaTime;

            if (newAngle > 180.0f)
                newAngle -= 360.0f;

            if ((newAngle >= model.cameraRotationXAxis.minimun) && (newAngle <= model.cameraRotationXAxis.maximun))
                cameraHolder.transform.localEulerAngles = new Vector3(newAngle, 0.0f, 0.0f);
        }

        /// <summary>
        /// Update player's movement via assigning new velocity.
        /// </summary>
        public void UpdateMovement(float inputHorizontal, float inputVertical)
        {

            if (!Mathf.Approximately(inputHorizontal, 0.0f) || !Mathf.Approximately(inputVertical, 0.0f))
            {
                Vector3 direction = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;
                Vector3 moveVelocity;
                Vector3 sprintVelocity;

                if (model.viewpointMode != PlayerModel.Viewpoint.ThirdPersonFixed)
                {
                    moveVelocity = (playerTransform.TransformDirection(direction) * model.playerStatus.movingSpeed * Time.fixedDeltaTime);
                    sprintVelocity = (playerTransform.TransformDirection(direction) * model.playerStatus.sprintSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    moveVelocity = direction * model.playerStatus.movingSpeed * Time.fixedDeltaTime;
                    sprintVelocity = direction * model.playerStatus.sprintSpeed * Time.fixedDeltaTime;
                }

                if (model.canSprint && isSprintKeyPressed && (model.playerStatus.currentStamina > 0))
                {
                    Vector3 newPosition = playerRigid.position + sprintVelocity;

                    if (model.viewpointMode == PlayerModel.Viewpoint.ThirdPersonFixed)
                    {
                        Quaternion newRotation = Quaternion.LookRotation((newPosition - playerRigid.position).normalized);
                        playerRigid.MoveRotation(newRotation);
                    }
                    playerRigid.MovePosition(newPosition);
                    if (sprintVelocity.sqrMagnitude > Mathf.Epsilon)// Is really moving.
                        model.isSprinting = true;
                    else
                        model.isSprinting = false;
                }
                else
                {
                    Vector3 newPosition = playerRigid.position + moveVelocity;

                    if (model.viewpointMode == PlayerModel.Viewpoint.ThirdPersonFixed)
                    {
                        Quaternion newRotation = Quaternion.LookRotation((newPosition - playerRigid.position).normalized);
                        playerRigid.MoveRotation(newRotation);
                    }
                    playerRigid.MovePosition(newPosition);
                    model.isSprinting = false;
                }
            }
        }

        /// <summary>
        /// Update player's rotation via quaternion in Y axis.
        /// </summary>
        public void UpdateRotation(float inputMouseX)
        {
            if (model.viewpointMode != PlayerModel.Viewpoint.ThirdPersonFixed)
                playerRigid.MoveRotation(playerRigid.rotation * Quaternion.AngleAxis(inputMouseX * model.playerStatus.angularSpeed * Time.fixedDeltaTime, playerTransform.up));
        }

        /// <summary>
        /// Update player's jump via adding impulse force.
        /// </summary>
        public void UpdateJump()
        {
            if (model.canJump && isGrounded)
            {
                playerRigid.linearVelocity = new Vector3(playerRigid.linearVelocity.x, 0f, playerRigid.linearVelocity.z);
                playerRigid.AddForce(new Vector3(0f, model.playerStatus.jumpForce, 0f), ForceMode.Impulse);
                isGrounded = false;
            }
        }

        /// <summary>
        /// Recover or decrease player's stamina in conditions.
        /// </summary>
        void UpdateStamina()
        {
            if (model.isSprinting)
            {
                if (model.playerStatus.currentStamina > 0.0f)
                    model.playerStatus.currentStamina -= model.playerStatus.staminaLossPerSecond * Time.deltaTime;
                else
                {
                    model.playerStatus.currentStamina = 0.0f;
                    isDrained = true;
                    StartCoroutine("Rest", model.playerStatus.restTimeWhenDrained);
                }
            }
            else if (!isDrained) // Have to rest a while if the player is drained.
            {
                if (model.playerStatus.currentStamina < model.playerStatus.maxStamina)
                    model.playerStatus.currentStamina += model.playerStatus.staminaRecoveryPerSecond * Time.deltaTime;
                else
                    model.playerStatus.currentStamina = model.playerStatus.maxStamina;
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
        /// Check whether the player is grounded.
        /// </summary>
        void UpdateIsGrounded()
        {
            if (Physics.CheckSphere(playerTransform.position + model.checkerPositionOffset, model.checkerRadius, model.checkLayer, QueryTriggerInteraction.Ignore))
                isGrounded = true;
            else
                isGrounded = false;
        }
    }
}
