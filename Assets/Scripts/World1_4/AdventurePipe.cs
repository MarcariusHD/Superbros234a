using System.Collections;
using MarioBros.Level;
using MarioBros.Player;
using MarioBros.UI;
using UnityEngine;

namespace MarioBros.World1_4
{
    /// <summary>Tubería dentro de la misma escena, con la animación PipeTransit existente.</summary>
    public sealed class AdventurePipe : MonoBehaviour
    {
        public Transform destination;
        public AdventureZone destinationZone;
        public bool underwaterDestination;
        public bool requireDown = true;
        private float cooldown;
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!AdventureLevel.Live || Time.unscaledTime < cooldown) return;
            var health = other.GetComponent<PlayerHealth>();
            if (health == null || health.IsDead) return;
            var movement = health.GetComponent<PlayerMovement>();
            if (requireDown && (!MarioInput.Down || Mathf.Abs(other.bounds.center.x - transform.position.x) > .5f || Mathf.Abs(other.bounds.min.y - transform.position.y) > .4f)) return;
            if (requireDown && !movement.IsUnderwater && !movement.IsGrounded) return;
            if (!requireDown && !MarioInput.Right) return;
            cooldown = Time.unscaledTime + 3;
            StartCoroutine(Travel(other.gameObject));
        }
        private IEnumerator Travel(GameObject player)
        {
            var level = AdventureLevel.Instance; level.Traveling = true;
            GameManager.Instance.SetState(GameState.Paused);
            var travel = new PipeTransit(player);
            level.sound.Play(level.sound.pipe, .35f);
            yield return travel.Enter(transform.position, requireDown);
            yield return level.presentation.Fade(true);
            player.transform.position = destination.position;
            level.ResetWater(underwaterDestination);
            level.EnterZone(destinationZone, true);
            player.transform.position = destination.position - Vector3.up * (player.GetComponent<SpriteRenderer>().bounds.size.y + .25f);
            yield return level.presentation.Fade(false);
            yield return travel.Emerge(destination.position);
            travel.Restore(); level.Traveling = false;
            GameManager.Instance.SetState(GameState.Playing);
            cooldown = Time.unscaledTime + .7f;
        }
    }
}
