using UnityEngine;
using UnityEngine.Assertions;

namespace CliffLeeCL
{
    /// <summary>
    /// The class control how the player interact with other objects in the game.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        /// <summary>
        /// Define how much force will be applied when the player drop the item.
        /// </summary>
        public Vector2 dropForce = new Vector2(50.0f, 10.0f);
        /// <summary>
        /// Is used to translate the item when the item is hold by the player.
        /// </summary>
        public Vector2 holdItemOffset = new Vector2(0.0f, 1.0f);

        /// <summary>
        /// Is used to retrieve current collision situation.
        /// </summary>
        PlayerCollision collision;
        /// <summary>
        /// The item that the player is holding.
        /// </summary>
        GameObject holdItem = null;
        /// <summary>
        /// Is true when the player is holding a item.
        /// </summary>
        bool isHoldingItem = false;
        /// <summary>
        /// Is true when the game is over.
        /// </summary>
        bool isGameOver = false;

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            collision = GetComponent<PlayerCollision>();
            Assert.IsTrue(collision, "Need \"PlayerCollision\" component on this gameObject");
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            // Execute actions
            if (Input.GetButtonDown("Fire1"))
            {
            }

            // Pick up / drop items
            if (Input.GetButtonDown("Fire2"))
            {
                if (isHoldingItem)
                {
                    Rigidbody2D rigid = holdItem.GetComponent<Rigidbody2D>();

                    isHoldingItem = false;
                    holdItem.transform.parent = null;
                    rigid.linearVelocity = Vector2.zero;
                    rigid.AddForce(new Vector2(dropForce.x * Mathf.Sign(transform.localScale.x), dropForce.y), ForceMode2D.Impulse);
                    holdItem = null;
                }
                else if (collision.isInItemRange)
                    if (!isHoldingItem)
                    {
                        isHoldingItem = true;
                        holdItem = collision.itemColliderData.gameObject;
                        holdItem.transform.parent = transform;
                        holdItem.transform.localPosition = holdItemOffset;
                    }
            }

            //Updage hold item.
            if (isHoldingItem)
            {
                // Item may be destroy on hand.
                if (holdItem && holdItem.transform.parent == transform)
                    holdItem.transform.localPosition = holdItemOffset;
                else {
                    isHoldingItem = false;
                }
            }

            // Crafting action
            if (Input.GetButtonDown("Jump"))
            {
            }

            if (!isGameOver && collision.isCollidedWithEnemy)
            {
                isGameOver = true;
                EventManager.Instance.OnGameOver();
            }
        }
    }
}
