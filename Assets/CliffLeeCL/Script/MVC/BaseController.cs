using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// Basic class for all controllers.
    /// </summary>
    /// <typeparam name="M">According model.</typeparam>
    public class BaseController<M> : MonoBehaviour 
        where M: BaseModel
    {
        protected M model;

        public virtual void SetupModel(M inputModel)
        {
            model = inputModel;
        }
    }
}
