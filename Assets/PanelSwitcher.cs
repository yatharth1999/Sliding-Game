using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject settingsPanel;
    public GameObject resultPanel;

    private List<GameObject> allPanels;

    void Awake()
    {
        allPanels = new List<GameObject> { mainMenuPanel, gamePanel, settingsPanel, resultPanel };
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

    public void ShowResultPanel()
    {
        SwitchToPanel(resultPanel);
    }
}
