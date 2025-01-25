using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "InputCtrlConfig", menuName = "Scriptable Objects/InputCtrlConfig")]
public class InputCtrlConfig : ScriptableObject
{
    public enum InputCtrlType
    {
        Local,
        Net,
        Debug
    }
    
    // (InputCtrl, prefab) pair
    [System.Serializable]
    public class InputCtrlPrefabPair
    {
        public InputCtrlType inputCtrlType;
        public BaseInputController inputCtrl;
    }
    
    // pair list
    [SerializeField]
    private List<InputCtrlPrefabPair> inputCtrlPrefabList = new System.Collections.Generic.List<InputCtrlPrefabPair>();
    
    // pair dictionary
    private Dictionary<InputCtrlType, BaseInputController> inputCtrlPrefabDict = new Dictionary<InputCtrlType, BaseInputController>();
    
    private void OnEnable()
    {
        // init dictionary
        inputCtrlPrefabDict.Clear();
        foreach (var pair in inputCtrlPrefabList)
        {
            if (inputCtrlPrefabDict.ContainsKey(pair.inputCtrlType))
            {
                continue;
            }
            inputCtrlPrefabDict.Add(pair.inputCtrlType, pair.inputCtrl);
        }
    }
    
    public BaseInputController GetInputCtrlPrefab(InputCtrlType inputCtrlType)
    {
        if (inputCtrlPrefabDict.ContainsKey(inputCtrlType))
        {
            return inputCtrlPrefabDict[inputCtrlType];
        }
        return null;
    }
}
