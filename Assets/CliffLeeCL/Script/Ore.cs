using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CliffLeeCL
{
    /// <summary>
    /// The class define how ore works.
    /// </summary>
    public class Ore : MonoBehaviour
    {
        /// <summary>
        /// The text that shows the remaining count.
        /// </summary>
        public Text remainingCountText;
        /// <summary>
        /// Define how much damage can a ore take.
        /// </summary>
        public int health = 2;
        /// <summary>
        /// Game objects that aim to be dropped. 
        /// </summary>
        [Tooltip("Drop a random object if there are more than one prefab.")]
        public GameObject[] dropObjectPrefabs;
        /// <summary>
        /// The variable how many objects will be dropped.
        /// </summary>
        /// <para>The minimun is inclusive, and the maximum is exclusive.</para>
        [Tooltip("Min[inclusive], Max[exclusive]")]
        public IntervalInt dropNumber = new IntervalInt(1, 4);
        /// <summary>
        /// Is used to adjust the drop point.
        /// </summary>
        public Vector3 dropPositionOffset = Vector3.zero;
        /// <summary>
        /// Define how much the force will be applied to dropped objects.
        /// </summary>
        public Vector2 dropForce = Vector2.zero;

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            remainingCountText.enabled = false;
        }

        /// <summary>
        /// The function will be called by attacker to damage the owner of this function.
        /// </summary>
        /// <param name="damage">The amount of damage</param>
        public void TakeDamage(int damage)
        {
            health -= damage;
            remainingCountText.text = health.ToString();
            remainingCountText.enabled = true;
            if (health <= 0)
                OnOreDied();
        }

        /// <summary>
        /// Actions will be done after the ore is dead.
        /// </summary>
        void OnOreDied()
        {
            remainingCountText.enabled = false;
            DropObject();
            Destroy(gameObject);
        }

        /// <summary>
        /// The fucntion drop object with some randomness if required.
        /// </summary>
        void DropObject()
        {
            int dropNum = Random.Range(dropNumber.minimun, dropNumber.maximun);

            for (int i = 0; i < dropNum; ++i)
            {
                GameObject instance;
                int typeIndex = Random.Range(0, dropObjectPrefabs.Length);

                instance = (GameObject)Instantiate(dropObjectPrefabs[typeIndex], transform.position + dropPositionOffset, Quaternion.identity);
                instance.GetComponent<Rigidbody2D>().AddForce(dropForce, ForceMode2D.Impulse);
            }
        }
    }
}
