using MarioBros.Player;
using UnityEngine;

namespace MarioBros.World1_4
{
    /// <summary>Activa el nado original solo dentro del agua; admite volúmenes contiguos.</summary>
    public sealed class AdventureWater : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other) { Enter(other); }
        private void OnTriggerStay2D(Collider2D other) { Enter(other); }
        private void Enter(Collider2D other)
        {
            if (other.GetComponent<PlayerHealth>() != null && AdventureLevel.Instance != null && !AdventureLevel.Instance.Traveling)
                AdventureLevel.Instance.SetWater(this, true);
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerHealth>() != null && AdventureLevel.Instance != null && !AdventureLevel.Instance.Traveling)
                AdventureLevel.Instance.SetWater(this, false);
        }
    }
}
