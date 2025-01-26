using System;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public PlayerConfig playerConfig;
    public InputCtrlConfig inputCtrlConfig;
    public ItemConfig itemConfig;
    public MainMenuConfig mainMenuConfig;
    
    // singleton
    private static GameConfig instance = null;
    public static GameConfig Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        // assgin singleton
        instance = this;
    }
}
