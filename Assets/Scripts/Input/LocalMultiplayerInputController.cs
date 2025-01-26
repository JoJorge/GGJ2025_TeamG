using CliffLeeCL;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiplayerInputController : BaseInputController
{
    public PlayerInput playerInput;
    public bool useMicrophone = true;
    [ShowIf("useMicrophone")]
    public int microphoneIndex = 0;
    [ShowIf("useMicrophone")]
    public float loudnessScalar = 10;
    [ShowIf("useMicrophone")]
    public float loudnessThreshold = 0.5f;
    public float moveScalar = 1;
    public float turnScalar = 1;

    private AudioDetector audioDetector = new();
 
    private void Start()
    {
        playerInput = player.GetComponent<PlayerInput>();
        playerInput.onActionTriggered += OnActionTriggered;
        microphoneIndex = playerInput.playerIndex;
    }

    private void OnEnable()
    {
        EventManager.Instance.onMatchStart += OnMatchStart;
    }
    
    private void OnDisable()
    {
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
        if (useMicrophone)
        {
            var loudness = audioDetector.GetMicrophoneLoudness(microphoneIndex) * loudnessScalar;
            if (loudness > loudnessThreshold)
            {
                player.ShootBubble(loudness);
            }
        }
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log($"Action started: {context.action.name}");
        }
        if (context.performed)
        {
            Debug.Log($"Action performed: {context.action.name}");
        }
        if (context.canceled)
        {
            Debug.Log($"Action canceled: {context.action.name}");
        }
        if (context.action.name == "Move")
        {
            player.StartMove(context.ReadValue<Vector2>() * moveScalar);
        }
        if (context.action.name == "Look")
        {
            player.StartTurn(context.ReadValue<Vector2>() * turnScalar);
        }
        
        if (context.action.name == "Jump")
        {
            player.Jump();
        }
        if (context.action.name == "Attack")
        {
            player.ShootAttack();
        }
        if (!useMicrophone && context.action.name == "SecondaryAttack")
        {
            player.ShootBubble(1);
        }
    }
}
