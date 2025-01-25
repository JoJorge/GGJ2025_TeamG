using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

namespace CliffLeeCL
{
    public class UndoAndRedoButton : MonoBehaviour
    {
        public Button[] undoButton, redoButton;

        // Update is called once per frame
        void Update()
        {
            if(LightingManager.instance.currentRecordIndex == LightingManager.instance.lightingSetupRecord.Count)
                for(int i = 0; i < redoButton.Length; i++)
                    redoButton[i].interactable = false;
            else
                for (int i = 0; i < redoButton.Length; i++)
                    redoButton[i].interactable = true;

            if (LightingManager.instance.currentRecordIndex == 1)
                for (int i = 0; i < redoButton.Length; i++)
                    undoButton[i].interactable = false;
            else
                for (int i = 0; i < redoButton.Length; i++)
                    undoButton[i].interactable = true;
        }

        public void Undo()
        {
            LightingManager.instance.LoadLightingSetup(LightingManager.instance.lightingSetupRecord[LightingManager.instance.currentRecordIndex - 2]);
            LightingManager.instance.currentRecordIndex--;
            // Update light fixture can't work here, so moved to LightingDesign.InitalizeLight
        }

        public void Redo()
        {
            LightingManager.instance.LoadLightingSetup(LightingManager.instance.lightingSetupRecord[LightingManager.instance.currentRecordIndex]);
            LightingManager.instance.currentRecordIndex++;
            // Update light fixture can't work here, so moved to LightingDesign.InitalizeLight
        }
    }
}
