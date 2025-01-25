using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

namespace CliffLeeCL {
    public class PlayerView : BaseView<PlayerModel, PlayerControllerMVC>
    {
        /// <summary>
        /// Is used to emit sprint effect particle.
        /// </summary>
        public ParticleSystem sprintEffect;

        /// <summary>
        /// Time that transition between normalFOV to sprintFOV.
        /// </summary>
        float normalToSprintTime = 0.0f;
        /// <summary>
        /// Time that transition between sprintFOV to normalFOV.
        /// </summary>
        float sprintToNormalTime = 0.0f;

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
            UpdateSprintEffect();
            if (model.viewpointMode != PlayerModel.Viewpoint.ThirdPersonFixed)
            {
                UpdateCameraFOV();
                if(Input.GetAxis("Mouse Y") != 0.0f)
                    controller.UpdateCameraRotation(Input.GetAxis("Mouse Y"));
            }
        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            if ((Input.GetAxis(model.horizontalAxisName) != 0.0f) || (Input.GetAxis(model.verticalAxisName) != 0.0f))
                controller.UpdateMovement(Input.GetAxis(model.horizontalAxisName), Input.GetAxis(model.verticalAxisName));

            if (Input.GetAxis("Mouse X") != 0.0f)
                controller.UpdateRotation(Input.GetAxis("Mouse X"));

            if (Input.GetButtonDown("Jump"))
                controller.UpdateJump();

            controller.isSprintKeyPressed = Input.GetKey(model.sprintKey);
        }

        /// <summary>
        /// The function is used to emit sprint effect paticle when the player is sprinting.
        /// </summary>
        void UpdateSprintEffect()
        {
            if (model.isSprinting)
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
        /// Transition between different FOVs if needed.
        /// </summary>
        void UpdateCameraFOV()
        {
            if (model.isSprinting && (Camera.main.fieldOfView < model.sprintFOV))
            {
                sprintToNormalTime = 0.0f;
                normalToSprintTime += Time.deltaTime;
                //Not good but enough.
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, model.sprintFOV, normalToSprintTime / model.FOVTransitionTime);
            }
            else if (!model.isSprinting && (Camera.main.fieldOfView > model.normalFOV))
            {
                normalToSprintTime = 0.0f;
                sprintToNormalTime += Time.deltaTime;
                //Not good but enough.
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, model.normalFOV, sprintToNormalTime / model.FOVTransitionTime);
            }
        }

        /// <summary>
        /// Implement this OnDrawGizmosSelected if you want to draw gizmos only if the object is selected.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + model.checkerPositionOffset, model.checkerRadius);
        }
    }
}
