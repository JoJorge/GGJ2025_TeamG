using UnityEngine;
using UnityEngine.Assertions;

namespace CliffLeeCL
{
    public class VRManager : MonoBehaviour
    {
        /// <summary>
        /// The variable is used to access this class.
        /// </summary>
        public static VRManager instance;

        /// <summary>
        /// Camera on headset
        /// </summary>
        public Camera headsetCamera;
        /// <summary>
        /// Vive headset
        /// </summary>
        public GameObject headset;
        /// <summary>
        /// Vive left controller
        /// </summary>
        public GameObject controllerLeft;
        /// <summary>
        /// Vive right controller
        /// </summary>
        public GameObject controllerRight;
        /// <summary>
        /// Vive left pointer
        /// </summary>
        public GameObject pointerLeft;
        /// <summary>
        /// Vive right pointer
        /// </summary>
        public GameObject pointerRight;

        /// <summary>
        /// Is used to know status of vive controller (trigger).
        /// </summary>
        public enum ViveTrigger {
            None,
            Left,
            Right,
            BothLeftFirst,
            BothRightFirst
        };

        /// <summary>
        /// Is used to know status of vive controller (trigger).
        /// </summary>
        public ViveTrigger triggerStatus = ViveTrigger.None;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);

            headset = GameObject.FindGameObjectWithTag("ViveHeadset");
            controllerLeft = GameObject.FindGameObjectWithTag("ViveControllerLeft");
            controllerRight = GameObject.FindGameObjectWithTag("ViveControllerRight");
            pointerLeft = GameObject.FindGameObjectWithTag("VivePointerLeft");
            pointerRight = GameObject.FindGameObjectWithTag("VivePointerRight");
        }

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            if (pointerLeft)
                pointerLeft.SetActive(false);
            if (pointerRight)
                pointerRight.SetActive(false);
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if(!headset)
                headset = GameObject.FindGameObjectWithTag("ViveHeadset");
            if (!headsetCamera && headset)
                headsetCamera = headset.GetComponent<Camera>();
            if (!controllerLeft)
                controllerLeft = GameObject.FindGameObjectWithTag("ViveControllerLeft");
            if (!controllerRight)
                controllerRight = GameObject.FindGameObjectWithTag("ViveControllerRight");
            if (!pointerLeft)
            {
                pointerLeft = GameObject.FindGameObjectWithTag("VivePointerLeft");
                if(pointerLeft)
                    pointerLeft.SetActive(false);
            }
            if (!pointerRight)
            {
                pointerRight = GameObject.FindGameObjectWithTag("VivePointerRight");
                if (pointerRight)
                    pointerRight.SetActive(false);
            }
            /*
            if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Grip))
                pointerRight.SetActive(!pointerRight.activeInHierarchy);

            if(ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger))
            {
                if(triggerStatus == ViveTrigger.None)
                    triggerStatus = ViveTrigger.Left;
                if(triggerStatus == ViveTrigger.Right)
                    triggerStatus = ViveTrigger.BothRightFirst;
            }

            if (ViveInput.GetPressUp(HandRole.LeftHand, ControllerButton.Trigger))
            {
                if (triggerStatus == ViveTrigger.Left)
                    triggerStatus = ViveTrigger.None;
                if ((triggerStatus == ViveTrigger.BothLeftFirst) || (triggerStatus == ViveTrigger.BothRightFirst))
                    triggerStatus = ViveTrigger.Right;
            }

            if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger))
            {
                if (triggerStatus == ViveTrigger.None)
                    triggerStatus = ViveTrigger.Right;
                if (triggerStatus == ViveTrigger.Left)
                    triggerStatus = ViveTrigger.BothLeftFirst;
            }

            if (ViveInput.GetPressUp(HandRole.RightHand, ControllerButton.Trigger))
            {
                if (triggerStatus == ViveTrigger.Right)
                    triggerStatus = ViveTrigger.None;
                if ((triggerStatus == ViveTrigger.BothLeftFirst) || (triggerStatus == ViveTrigger.BothRightFirst))
                    triggerStatus = ViveTrigger.Left;
            }*/
        }
    }
}

