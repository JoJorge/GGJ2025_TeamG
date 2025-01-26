using UnityEngine;
using UnityEngine.UI;
using CliffLeeCL;

public class FireCd : MonoBehaviour
{
    [SerializeField]
    private Image image;

    [SerializeField]
    private Timer cdTimer;
    
    private bool isCd = false;
    
    private float cdTime = 0;
    
    private void OnEnable()
    {
        EventManager.Instance.onAttackCDStart += StartAttackCd;  
    }
   
    private void OnDisable()
    {
        EventManager.Instance.onAttackCDStart -= StartAttackCd;
    }
    
    private void Update()
    {
        if (isCd)
        {
            image.fillAmount = cdTimer.CurrentTime / cdTime;
        }
    }
    
    private void StartAttackCd(object sender, float cd)
    {
        cdTime = cd;
        cdTimer.StartCountDownTimer(cd, false, () => {
            isCd = false;
            image.fillAmount = 0;
        });
    }
}
