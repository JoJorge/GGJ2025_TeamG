using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CliffLeeCL
{
    public class MainMenu : State<GameCore>
    {
        private MainMenuUI mainMenuUI;
        private bool isFinished;

        public override void OnStateEnter()
        {
            isFinished = false;
            mainMenuUI = GameObject.Instantiate(GameConfig.Instance.uiConfig.MainMenuUI);
            mainMenuUI.OnStart += () => isFinished = true;
            mainMenuUI.OnExit += Exit;
        }

        private void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public override void UpdateState()
        {
            if (isFinished)
            {
                stateContext.SwitchState("PreparePlayer");
            }
        }

        public override void OnStateExit()
        {
            GameObject.Destroy(mainMenuUI.gameObject);
        }
    }
}
