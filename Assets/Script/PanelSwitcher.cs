using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PanelSwitcher : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject puzzleGamePanel;
    public GameObject settingsPanel;
    public GameObject resultPanel;
    public AudioClip BackgroundMusic;
    public AudioSource audioSource;

    [SerializeField] public TMP_Dropdown LevelDropdown;
    [SerializeField] public TMP_Dropdown SolverDropdown;

    public TextMeshProUGUI timerText;
    public float timeRemaining = 120f; // 2 minutes
    private Coroutine timerCoroutine;

    [Header("Lives / Hearts UI")]
    [Tooltip("Assign 3 heart Image components in order (full set for lives). Will enable based on remaining lives.")]
    public List<Image> heartImages = new List<Image>();
    // Internal flag: timer-triggered auto-solve pending life loss once solution completes.
    private bool pendingLifeLossOnAutoSolve = false;
    public bool IsPendingLifeLoss => pendingLifeLossOnAutoSolve;

    private List<GameObject> allPanels;
    public static PanelSwitcher inst;
    void Awake()
    {
        if (inst == null)
            inst = this;
        else
            Destroy(gameObject);
        allPanels = new List<GameObject> { mainMenuPanel, gamePanel, settingsPanel, resultPanel, puzzleGamePanel };
    }

    void Start()
    {
        ShowMainMenuPanel();
    UpdateHeartsUI();
    }

    private void SwitchToPanel(GameObject panelToShow)
    {
        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
            {
                panel.SetActive(panel == panelToShow);
            }
        }
        // Stop timer if not puzzle game panel
        if (panelToShow != puzzleGamePanel && timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    public void ShowMainMenuPanel()
    {
        SwitchToPanel(mainMenuPanel);
    }

    public void ShowGamePanel()
    {
        SwitchToPanel(gamePanel);
    }

    public void ShowSettingsPanel()
    {
        SwitchToPanel(settingsPanel);
    }
    public void ShowPuzzleGamePanel()
    {
        SwitchToPanel(puzzleGamePanel);
        // Start 2-minute timer
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timeRemaining = 120f;
        timerCoroutine = StartCoroutine(TimerCountdown());
    }
    public void ShowResultPanel()
    {
        SwitchToPanel(resultPanel);
    }
    public void PlayBackgroundMusic()
    {
        if (audioSource != null && BackgroundMusic != null)
        {
            audioSource.clip = BackgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StartGame()
    {
        ShowGamePanel();
        StartCoroutine(TypewriterEffect.inst.ShowText());
    }

    private System.Collections.IEnumerator TimerCountdown()
    {
        while (timeRemaining > 0)
        {
            UpdateTimerDisplay();
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }
        UpdateTimerDisplay();
        // Time's up: trigger AI auto-solve first; life will be deducted AFTER solve completes.
        if (puzzleGamePanel != null && ImageSlidingPuzzle.inst != null)
        {
            pendingLifeLossOnAutoSolve = true;
            ImageSlidingPuzzle.inst.SolvePuzzle(); // parameterless; handler will check pending flag
            yield break;
        }
        // Fallback if no puzzle available: just deduct life immediately.
        DeductLifeAfterAutoSolve();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        if (timerText != null)
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    // Update heart images enabled state based on GameMgr lives count.
    public void UpdateHeartsUI()
    {
        if (heartImages == null || heartImages.Count == 0 || GameMgr.inst == null) return;
        int lives = Mathf.Clamp(GameMgr.inst.lives, 0, heartImages.Count);
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] != null)
                heartImages[i].enabled = i < lives;
        }
    }

    // Called by puzzle after an auto-solve completes (timer forced).
    public void HandleAutoSolved()
    {
        if (!pendingLifeLossOnAutoSolve) return;
        pendingLifeLossOnAutoSolve = false;
        DeductLifeAfterAutoSolve();
    }

    private void DeductLifeAfterAutoSolve()
    {
        if (GameMgr.inst == null) return;
        GameMgr.inst.lives = Mathf.Max(0, GameMgr.inst.lives - 1);
        UpdateHeartsUI();
        if (GameMgr.inst.lives <= 0)
        {
            ShowGamePanel();
            if (TypewriterEffect.inst != null)
            {
                if (TypewriterEffect.inst.headingText != null)
                    TypewriterEffect.inst.headingText.text = "YOU LOST THE CASE";
                if (TypewriterEffect.inst.bodyText != null)
                    TypewriterEffect.inst.bodyText.text = "AI won again solving this case faster than us.\nI think it's time to say goodbye.";
                // Enable restart mode so player can restart after losing all lives
                TypewriterEffect.inst.EnableRestart();
            }
        }
    }
}
