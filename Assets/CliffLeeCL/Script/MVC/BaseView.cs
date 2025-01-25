using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// Basic class for all views.
    /// </summary>
    /// <typeparam name="M">According model.</typeparam>
    /// <typeparam name="C">According controller.</typeparam>
    public class BaseView<M, C> : MonoBehaviour
        where M: BaseModel
        where C: BaseController<M>
    {
        public M model;
        [SerializeField]
        protected C controller;

        public virtual void Awake()
        {
            controller.SetupModel(model);
        }
    }
}
