using System.Collections;
using MarioBros.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarioBros.World1_4
{
    /// <summary>Carteles y final construidos con el mismo NesText y recursos del menú de Marco.</summary>
    public sealed class AdventurePresentation : MonoBehaviour
    {
        public NesText announcement, hint, zone, bossTitle, victoryScore;
        public CanvasGroup announcementGroup, hintGroup, fadeGroup;
        public GameObject bossPanel, victoryPanel;
        public Button menuButton;
        public Image[] healthCoins;
        public ParticleSystem celebration;
        private float announcementUntil, hintUntil;
        private bool leaving;
        private void Update()
        {
            announcementGroup.alpha = Mathf.Clamp01((announcementUntil - Time.unscaledTime) * 3);
            hintGroup.alpha = Mathf.Clamp01((hintUntil - Time.unscaledTime) * 3);
        }
        public void Announce(string text, float seconds = 3) { announcement.Text = text; announcementUntil = Time.unscaledTime + seconds; }
        public void Hint(string text, float seconds = 3) { hint.Text = text; hintUntil = Time.unscaledTime + seconds; }
        public void SetZone(string title) { zone.Text = title; }
        public void Boss(int health, int phase, string message)
        {
            bossPanel.SetActive(true); bossTitle.Text = "BOWSER — FASE " + phase + "/3";
            for (int i = 0; i < healthCoins.Length; i++) healthCoins[i].color = i < health ? Color.white : new Color(.25f, .25f, .25f, .8f);
            Hint(message, 30);
        }
        public IEnumerator Fade(bool dark)
        {
            float target = dark ? 1 : 0;
            while (Mathf.Abs(fadeGroup.alpha - target) > .01f)
            {
                fadeGroup.alpha = Mathf.MoveTowards(fadeGroup.alpha, target, Time.unscaledDeltaTime * 4);
                yield return null;
            }
            fadeGroup.alpha = target;
        }
        public void Victory()
        {
            StartCoroutine(CelebrateThenScreen());
        }
        private IEnumerator CelebrateThenScreen()
        {
            bossPanel.SetActive(false); hintGroup.gameObject.SetActive(false); announcementGroup.gameObject.SetActive(false);
            victoryScore.Text = GameManager.Score.ToString("D6") + " PUNTOS";
            if (celebration != null) celebration.Play();
            yield return new WaitForSecondsRealtime(1.8f);
            victoryPanel.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(menuButton.gameObject);
        }
        public void ReturnToMenu()
        {
            if (leaving) return; leaving = true; AdventureCheckpoint.Clear(); Time.timeScale = 1;
            SceneManager.LoadScene("TitleScreen");
        }
        public void Replay()
        {
            if (leaving) return; leaving = true; AdventureCheckpoint.Clear(); Time.timeScale = 1;
            GameManager.Instance.SetState(GameState.Playing); SceneManager.LoadScene("World1_4");
        }
    }
}
