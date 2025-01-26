using CliffLeeCL;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class AmmoSlider : MonoBehaviour
{
   public Team team;
   
   public Slider slider;

   private void OnEnable()
   {
      EventManager.Instance.onCurrentAmmoChanged += SetCurrentAmmo;  
      EventManager.Instance.onMaxAmmoChanged += SetMaxAmmo;
   }
   
   private void OnDisable()
   {
      EventManager.Instance.onCurrentAmmoChanged -= SetCurrentAmmo;
      EventManager.Instance.onMaxAmmoChanged -= SetMaxAmmo;
   }

   private void SetCurrentAmmo(object sender, int ammo)
   {
      var normalPlayer = sender as NormalPlayer;
      if (normalPlayer == null || normalPlayer.TeamType != team)
      {
         return;
      }
      slider.value = ammo;
   }

   private void SetMaxAmmo(object sender, int ammo)
   {
      var normalPlayer = sender as NormalPlayer;
      if (normalPlayer == null || normalPlayer.TeamType != team)
      {
         return;
      }
      slider.maxValue = ammo;
   }
   
   [Button]
   private void TestMaxAmmo()
   {
      EventManager.Instance.OnMaxAmmoChanged(this, 10);
   }
   
   [Button]
   private void TestCurrentAmmo()
   {
      EventManager.Instance.OnCurrentAmmoChanged(this, 5);
   }
}
