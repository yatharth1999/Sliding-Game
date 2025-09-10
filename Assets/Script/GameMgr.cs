using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    // Start is called before the first frame update

    public enum aiSolverType
    {
        BFS,
        DFS,
        AStar
    }
    public aiSolverType solverType = aiSolverType.BFS;
    public int gameLevel = 3; // 3x3 grid
    public int lives = 3;

    public static GameMgr inst;
    void Start()
    {
        if (inst == null)
            inst = this;
        else
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetGameLevel(int level)
    {
        gameLevel = level;
    }
    public void SetSolverType(int type)
    {
        solverType = (aiSolverType)type;
    }
    public void PressedApply()
    {

        SetGameLevel(PanelSwitcher.inst.LevelDropdown.value + 3);
        SetSolverType(PanelSwitcher.inst.SolverDropdown.value);
        if (ImageSlidingPuzzle.inst != null)
            ImageSlidingPuzzle.inst.SetN(gameLevel);
        PanelSwitcher.inst.ShowMainMenuPanel();
        ResponsiveGrid.inst.SetN(gameLevel);
    }
    public void PressedBack()
    {
        PanelSwitcher.inst.ShowMainMenuPanel();
    }

    public void ResetGame()
    {
        lives = 3;
        gameLevel = 3; // default
        solverType = aiSolverType.BFS; // default
        if (PanelSwitcher.inst != null)
            PanelSwitcher.inst.UpdateHeartsUI();
        if (TypewriterEffect.inst != null)
            TypewriterEffect.inst.textIndex = 0;
        // Reset puzzle size to default
        if (ImageSlidingPuzzle.inst != null)
            ImageSlidingPuzzle.inst.SetN(gameLevel);
    }
}
