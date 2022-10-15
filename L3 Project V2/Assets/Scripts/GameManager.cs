using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState state;

    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(Instance);
    }

    private void Start()
    {
        UpdateGameState(GameState.Loading);
    }

    public void BootUp()
    {

    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.Loading:
                break;
            case GameState.Transition:
                break;
        }
        OnGameStateChanged?.Invoke(newState);
    }
}
public enum GameState
{
    Loading,
    Transition
}