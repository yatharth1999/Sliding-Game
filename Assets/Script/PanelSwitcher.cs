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
    private float timeRemaining = 300f; // 5 minutes
    private Coroutine timerCoroutine;

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
        // Start 5-minute timer
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timeRemaining = 300f;
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
        // Time's up, reduce life
        if (GameMgr.inst != null)
        {
            GameMgr.inst.lives--;
            if (GameMgr.inst.lives <= 0)
            {
                // Show game panel with wrong heading
                ShowGamePanel();
                if (TypewriterEffect.inst != null && TypewriterEffect.inst.headingText != null)
                    TypewriterEffect.inst.headingText.text = "CASE FAILED";
            }
            else
            {
                // Show result panel or handle as needed
                ShowResultPanel();
            }
        }
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
}
