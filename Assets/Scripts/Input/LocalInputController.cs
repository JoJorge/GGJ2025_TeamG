using CliffLeeCL;
using Sirenix.OdinInspector;
using UnityEngine;

public class LocalInputController : BaseInputController
{
    public bool useMicrophone = true;
    [ShowIf("useMicrophone")]
    public int microphoneIndex = 0;
    [ShowIf("useMicrophone")]
    public float loudnessScalar = 10;
    [ShowIf("useMicrophone")]
    public float loudnessThreshold = 0.5f;
    public float moveScalar = 1;
    public float turnScalar = 1;
    
    private InputSystem_Actions inputActions;
    private AudioDetector audioDetector = new();
 
    [SerializeField]
    private float shootBubbleCd = 0.3f;
    
    [SerializeField]
    private Timer shootBubbleCdTimer;
    
    private bool isShootBubbleCd = false;
    
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
        if (useMicrophone)
        {
            audioDetector.StartRecording(microphoneIndex);
        }
    }

    void Update()
    {
        player.StartMove(inputActions.Player.Move.ReadValue<Vector2>() * moveScalar);
        player.StartTurn(inputActions.Player.Look.ReadValue<Vector2>() * turnScalar);
        
        if (inputActions.Player.Jump.triggered)
        {
            player.Jump();
        }
        
        if (inputActions.Player.Attack.triggered)
        {
            player.ShootAttack();
        } 
        
        if (isShootBubbleCd)
        {
            return;
        }
        if (useMicrophone)
        {
            var loudness = audioDetector.GetMicrophoneLoudness(microphoneIndex) * loudnessScalar;
            if (loudness > loudnessThreshold)
            {
                player.ShootBubble(loudness);
                isShootBubbleCd = true;
                shootBubbleCdTimer.StartCountDownTimer(shootBubbleCd, false, () => { 
                    isShootBubbleCd = false; 
                });
            }
        }
        else if (inputActions.Player.SecondaryAttack.triggered)
        {
            player.ShootBubble(0.5f);  
            isShootBubbleCd = true;
            shootBubbleCdTimer.StartCountDownTimer(shootBubbleCd, false, () => { 
                isShootBubbleCd = false; 
            });
        }
    }
}
