using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace CliffLeeCL
{
    /// <summary>
    /// The class is to implement the ladder mechanism in 2D games.
    /// </summary>
    public class Ladder : MonoBehaviour
    {
        Rigidbody2D playerRigid;
        /// <summary>
        /// Is used to get movement related data.
        /// </summary>
        PlayerStatus status;
        /// <summary>
        /// Is used to store the player's gravity before changing.
        /// </summary>
        float startPlayerGravity;
        /// <summary>
        /// Is true when the player is in the range.
        /// </summary>
        bool isPlayerInRange = false;

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            if (isPlayerInRange)
            {
                float inputVertical = Input.GetAxis("Vertical");
                Vector2 direction = new Vector2(0.0f, inputVertical).normalized;
                Vector2 climbVelocity = (direction * status.climbingSpeed * Time.fixedDeltaTime);

                playerRigid.linearVelocity = new Vector2(playerRigid.linearVelocity.x, 0.0f);
                if (inputVertical != 0.0f) {  
                   
                    playerRigid.linearVelocity = new Vector2(playerRigid.linearVelocity.x, climbVelocity.y);
                }
            }
        }

        /// <summary>
        /// Sent each frame where another object is within a trigger collider attached to this object (2D physics only).
        /// </summary>
        /// <param name="col">The Collision data associated with this collision.</param>
        void OnTriggerStay2D(Collider2D col)
        {
            if (col.tag == "Player") {
                if (!playerRigid)
                {
                    playerRigid = col.GetComponent<Rigidbody2D>();
                    startPlayerGravity = playerRigid.gravityScale;
                    Assert.IsTrue(playerRigid, "Need \"Rigidbody2D\" component on this gameObject");
                }
                if (!status)
                {
                    status = col.GetComponent<PlayerStatus>();
                    Assert.IsTrue(status, "Need \"PlayerStatus\" component on this gameObject");
                }
                playerRigid.gravityScale = 0.0f;
                isPlayerInRange = true;
            }
        }

        /// <summary>
        /// Sent when another object leaves a trigger collider attached to this object (2D physics only).
        /// </summary>
        /// <param name="col">The Collision data associated with this collision.</param>
        void OnTriggerExit2D(Collider2D col)
        {
            if (col.tag == "Player")
            {
                isPlayerInRange = false;
                playerRigid.gravityScale = startPlayerGravity;
            }
        }
    }
}
