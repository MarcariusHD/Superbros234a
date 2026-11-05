using MarioBros.CameraControl;
using UnityEngine;

namespace MarioBros.World1_4
{
    /// <summary>Reutiliza CameraFollow; añade únicamente el encuadre del jefe y temblores breves.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class AdventureCamera : MonoBehaviour
    {
        public CameraFollow follow;
        public Transform player;
        public Camera view;
        private bool arena;
        private Vector3 velocity;
        private float shakeUntil, shakeStrength;
        public void SetRegion(float left, float right, bool snap)
        {
            float half = view.orthographicSize * view.aspect;
            float min = right - left < half * 2 ? (left + right) / 2 : left + half;
            float max = Mathf.Max(min, right - half);
            float x = snap ? Mathf.Clamp(player.position.x + 2, min, max) : Mathf.Clamp(transform.position.x, min, max);
            follow.SetRegion(min, max, x);
        }
        public void FrameArena() { arena = true; follow.enabled = false; }
        public void Shake(float amount = .08f) { shakeStrength = amount; shakeUntil = Time.time + .24f; }
        private void LateUpdate()
        {
            if (arena)
            {
                float size = Mathf.Max(7.5f, 15 / Mathf.Max(.7f, view.aspect));
                view.orthographicSize = Mathf.Lerp(view.orthographicSize, size, 1 - Mathf.Exp(-Time.deltaTime * 3));
                transform.position = Vector3.SmoothDamp(transform.position, new Vector3(331, 7.5f, -10), ref velocity, .6f);
            }
            Vector3 p = transform.position;
            if (Time.time < shakeUntil && AdventureLevel.Live)
                p += new Vector3(Mathf.Sin(Time.time * 73), Mathf.Cos(Time.time * 89), 0) * shakeStrength * Mathf.Clamp01((shakeUntil - Time.time) / .24f);
            transform.position = new Vector3(Mathf.Round(p.x * 16) / 16, Mathf.Round(p.y * 16) / 16, -10);
        }
    }
}
