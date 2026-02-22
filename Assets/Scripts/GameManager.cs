using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GameManager class will attach to the GameManager game object and will handle the overall state of the game
/// i.e. the current puzzle, player score etc
/// </summary>
public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject levelPrefab;

    private void Start()
    {
        GenerateLevel();    
        SpawnPlayer();
    }
    
    private void Update()
    {
        HandleQuitGame();
    }

    private static void GenerateLevel()
    {
        
    }

    private static void SpawnPlayer()
    {
        
    }

    private static void HandleQuitGame()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
