using System.Collections;
using UnityEngine;

namespace MarioBros.World1_4
{
    public enum FinalBowserState { Waiting, Introduction, Attack, Recovery, Hurt, PhaseChange, Defeat }

    /// <summary>Máquina de estados del jefe. Los ataques y el puente permanecen en componentes separados.</summary>
    public sealed class FinalBowser : MonoBehaviour
    {
        public SpriteRenderer image;
        public Sprite[] walkFrames, fireFrames;
        public FinalBowserAttacks attacks;
        public AdventureHazard contact;
        public int Health { get; private set; } = 6;
        public int Phase { get; private set; } = 1;
        public FinalBowserState State { get; private set; } = FinalBowserState.Waiting;
        public bool Vulnerable => State == FinalBowserState.Recovery;
        public bool BreathingFire { get; set; }
        private Coroutine stateRoutine;
        private int turn;
        private AdventureLevel Level => AdventureLevel.Instance;
        public void Begin() { if (State == FinalBowserState.Waiting) SetState(FinalBowserState.Introduction); }
        public void Face(float direction) { image.flipX = direction < 0; }
        private void Update()
        {
            if (State == FinalBowserState.Waiting || State == FinalBowserState.Defeat) return;
            if (Level.player.IsDead) { Abort(); return; }
            var frames = BreathingFire ? fireFrames : walkFrames;
            if (frames.Length > 0) image.sprite = frames[(int)(Time.time * (BreathingFire ? 8 : 5)) % frames.Length];
            image.color = State == FinalBowserState.Hurt && Mathf.Sin(Time.time * 40) > 0 ? new Color(1, .35f, .3f) : Color.white;
        }
        private void SetState(FinalBowserState next)
        {
            if (stateRoutine != null) StopCoroutine(stateRoutine);
            State = next;
            contact.armed = next == FinalBowserState.Attack || next == FinalBowserState.Recovery;
            stateRoutine = StartCoroutine(StateRoutine(next));
        }
        private IEnumerator StateRoutine(FinalBowserState state)
        {
            switch (state)
            {
                case FinalBowserState.Introduction: yield return Introduce(); break;
                case FinalBowserState.Attack: yield return Attack(); break;
                case FinalBowserState.Recovery: yield return Recover(); break;
                case FinalBowserState.Hurt: yield return Hurt(); break;
                case FinalBowserState.PhaseChange: yield return ChangePhase(); break;
                case FinalBowserState.Defeat: yield return Defeat(); break;
            }
        }
        private IEnumerator Introduce()
        {
            Level.presentation.Announce("BOWSER\nEL ÚLTIMO DESAFÍO", 3);
            Level.presentation.Boss(Health, Phase, "ESQUIVA. CUANDO SE ILUMINE UN BLOQUE\nGOLPÉALO DESDE ABAJO");
            Level.sound.Play(Level.sound.mechanism, .3f);
            BreathingFire = true; Face(-1);
            Vector3 end = transform.position, start = end + Vector3.up * 2;
            float t = 0;
            while (t < 1) { t += Time.deltaTime; transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t)); yield return null; }
            Level.view.Shake(.06f); yield return new WaitForSeconds(2);
            SetState(FinalBowserState.Attack);
        }
        private IEnumerator Attack()
        {
            Level.arena.SetSwitches(false);
            yield return attacks.Pattern(this, turn++);
            if (AdventureLevel.Live) SetState(FinalBowserState.Recovery);
        }
        private IEnumerator Recover()
        {
            attacks.Clear(); BreathingFire = false;
            Level.arena.SetSwitches(true);
            Level.presentation.Boss(Health, Phase, "¡AHORA! GOLPEA UN BLOQUE ILUMINADO\nDESDE ABAJO PARA SOLTAR EL PUENTE");
            yield return new WaitForSeconds(Phase == 1 ? 6 : Phase == 2 ? 5.5f : 5);
            Level.arena.SetSwitches(false); SetState(FinalBowserState.Attack);
        }
        public bool ReceiveBridgeHit()
        {
            if (!Vulnerable || !AdventureLevel.Live) return false;
            Health--; attacks.Clear(); Level.arena.SetSwitches(false); SetState(FinalBowserState.Hurt); return true;
        }
        private IEnumerator Hurt()
        {
            BreathingFire = false;
            Level.presentation.Boss(Health, Phase, "¡EL PUENTE CEDE!\nPREPÁRATE PARA SU SIGUIENTE ATAQUE");
            Level.sound.Play(Level.sound.impact, .5f); Level.view.Shake(.1f);
            yield return new WaitForSeconds(1.2f);
            if (Health == 0) { SetState(FinalBowserState.Defeat); yield break; }
            int next = 3 - (Health - 1) / 2;
            if (next != Phase) { Phase = next; SetState(FinalBowserState.PhaseChange); }
            else SetState(FinalBowserState.Attack);
        }
        private IEnumerator ChangePhase()
        {
            Level.presentation.Announce(Phase == 2 ? "FASE 2\n¡CUIDADO CON SUS SALTOS!" : "FASE 3\nLA ÚLTIMA LLAMA", 2.5f);
            BreathingFire = true; Level.sound.Play(Level.sound.fire, .35f);
            yield return new WaitForSeconds(2.5f);
            SetState(FinalBowserState.Attack);
        }
        private IEnumerator Defeat()
        {
            attacks.Clear(); contact.armed = false;
            Level.presentation.Boss(0, 3, "¡EL PUENTE SE DERRUMBA!");
            Level.sound.Play(Level.sound.bowserFall, .6f);
            Level.arena.CollapseBridge();
            image.flipY = true;
            Vector3 start = transform.position; float t = 0;
            while (t < 2.3f)
            {
                t += Time.deltaTime; transform.position = start + new Vector3(t, 3 * t - 3.5f * t * t, 0); yield return null;
            }
            image.enabled = false; yield return new WaitForSeconds(.6f); Level.Complete();
        }
        public void Abort()
        {
            if (stateRoutine != null) StopCoroutine(stateRoutine);
            attacks.Clear(); contact.armed = false; Level.arena.SetSwitches(false); enabled = false;
        }
    }
}
