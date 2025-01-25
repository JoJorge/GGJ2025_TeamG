using CliffLeeCL;
using UnityEngine;

public class LocalInputController : BaseInputController 
{
    public int microphoneIndex = 0;
    public float loudnessScalar = 10;
    public float loudnessThreshold = 0.5f;
    public float turnScale = 1;
    
    private InputSystem_Actions inputActions;
    private AudioDetector audioDetector = new();
 
    private void Awake()
    {
        inputActions = new InputSystem_Actions();     
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        EventManager.Instance.onMatchStart += OnMatchStart;
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
        EventManager.Instance.onMatchStart -= OnMatchStart;
    }
    
    private void OnMatchStart()
    {
        audioDetector.StartRecording(microphoneIndex);
    }

    void Update()
    {
        player.StartMove(inputActions.Player.Move.ReadValue<Vector2>());
        player.StartTurn(inputActions.Player.Look.ReadValue<Vector2>() * turnScale);
        
        if (inputActions.Player.Jump.triggered)
        {
            player.Jump();
        }
        
        if (inputActions.Player.Attack.triggered)
        {
            player.ShootAttack();
        } 
        
        var loudness = audioDetector.GetMicrophoneLoudness(microphoneIndex) * loudnessScalar;
        if (loudness > loudnessThreshold)
        {
            player.ShootBubble(loudness);
        }
    }
}
