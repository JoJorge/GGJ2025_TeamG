using UnityEngine;

namespace CliffLeeCL
{
    public class MatchOver : State<GameCore>
    {

        public override void UpdateState()
        {
        }

        public override void OnStateEnter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void OnStateExit()
        {
        }
    }
}
