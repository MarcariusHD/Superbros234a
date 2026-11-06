using UnityEngine;
using MarioBros.UI;

namespace MarioBros.Player
{
    public class PlayerFire : MonoBehaviour
    {
        private float nextShot;
        private void Update()
        {
            var health=GetComponent<PlayerHealth>();var movement=GetComponent<PlayerMovement>();
            if(GameManager.CurrentState!=GameState.Playing||CoopSession.Transitioning||health.IsDead||
                health.CurrentPowerState!=PowerState.Fire||!movement.enabled||movement.IsCrouching||
                Time.time<nextShot||!PlayerIdentity.InputFor(this).FireDown)return;
            int active=0;
            foreach(var shot in FindObjectsByType<Fireball>(FindObjectsSortMode.None)) if(shot.Owner==gameObject)active++;
            if(active>=2)return;
            nextShot=Time.time+.2f;
            Fireball.Launch(gameObject,movement.IsFacingRight?1:-1);
            Sound.SfxPlayer.Play("fireball");
        }
    }
}
