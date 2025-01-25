using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// Define each player's basic status. (The same as PlayerStatus)
    /// </summary>
    public class PlayerModel : BaseModel
    {
        public PlayerStatus playerStatus;

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
        /// Is true when player is sprinting.
        /// </summary>
        public bool isSprinting = false;
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
    }
}
