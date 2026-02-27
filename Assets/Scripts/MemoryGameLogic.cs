using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryGameLogic : MonoBehaviour
{
    public event System.Action<int> LevelStarted;
    public event System.Action<int> LevelCompleted;
    public event System.Action<List<KeyCode>> SequencePrepared;
    public event System.Action<int, KeyCode> NotePlaybackStart;
    public event System.Action<int, KeyCode> NotePlaybackEnd;
    public event System.Action<int, bool> InputFeedback;
    public event System.Action<string> StatusChanged;
    public event System.Action<int> ScoreChanged;
    public event System.Action GameWon;
    public event System.Action ClearRequested;

    [Header("Settings")]
    [SerializeField] private float notePlayDuration = 0.4f;
    [SerializeField] private float pauseBetweenNotes = 0.2f;
    [SerializeField] private float delayBeforeInput = 0.5f;
    [SerializeField] private float delayAfterWrongAnswer = 1.5f;
    [SerializeField] private float delayAfterCorrectLevel = 1.0f;
    [SerializeField] private int startingSequenceLength = 4;
    [SerializeField] private int lengthIncreasePerLevel = 2;
    [SerializeField] private int totalLevels = 5;

    [Header("Symbols")]
    [SerializeField] private KeyCode[] inputKeys = new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T };

    [Header("Time Limit")]
    [SerializeField] private bool enableTimeLimit = false;
    [SerializeField] private float totalTimeLimitSeconds = 60f;

    private enum GameState
    {
        Idle,
        PlayingSequence,
        WaitingForInput,
        ShowingResult
    }

    private List<List<KeyCode>> sequences = new List<List<KeyCode>>();
    private List<KeyCode> playerInput = new List<KeyCode>();
    private int currentLevel;
    private int score;
    private GameState state = GameState.Idle;
    private Coroutine activeRoutine;
    private float timeRemaining;
    private bool gameEnded;

    void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        currentLevel = 0;
        score = 0;
        gameEnded = false;
        if (enableTimeLimit)
            timeRemaining = totalTimeLimitSeconds;
        GenerateSequences();
        ScoreChanged?.Invoke(score);
        StartLevel();
    }

    void Update()
    {
        if (enableTimeLimit && !gameEnded)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                gameEnded = true;
                state = GameState.ShowingResult;
                StatusChanged?.Invoke("Time's up! Press Space to restart.");
            }
        }

        if (gameEnded && Input.GetKeyDown(KeyCode.Space))
        {
            StartNewGame();
            return;
        }

        if (state != GameState.WaitingForInput)
            return;

        for (int i = 0; i < inputKeys.Length; i++)
        {
            var key = inputKeys[i];
            if (Input.GetKeyDown(key))
            {
                HandlePlayerInput(key);
                break;
            }
        }
    }

    void GenerateSequences()
    {
        sequences.Clear();
        int length = startingSequenceLength;
        for (int i = 0; i < totalLevels; i++)
        {
            List<KeyCode> seq = new List<KeyCode>();
            for (int j = 0; j < length; j++)
                seq.Add(inputKeys[Random.Range(0, inputKeys.Length)]);
            sequences.Add(seq);
            length += lengthIncreasePerLevel;
        }
    }

    void StartLevel()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
        LevelStarted?.Invoke(currentLevel);
        activeRoutine = StartCoroutine(PlaySequenceRoutine());
    }

    IEnumerator PlaySequenceRoutine()
    {
        state = GameState.PlayingSequence;
        playerInput.Clear();
        ClearRequested?.Invoke();
        StatusChanged?.Invoke($"Watch & Listen... (Level {currentLevel + 1})");
        yield return new WaitForSeconds(delayBeforeInput);
        var sequence = sequences[currentLevel];
        SequencePrepared?.Invoke(sequence);
        for (int i = 0; i < sequence.Count; i++)
        {
            NotePlaybackStart?.Invoke(i, sequence[i]);
            yield return new WaitForSeconds(notePlayDuration);
            NotePlaybackEnd?.Invoke(i, sequence[i]);
            yield return new WaitForSeconds(pauseBetweenNotes);
        }
        yield return new WaitForSeconds(delayBeforeInput);
        state = GameState.WaitingForInput;
        StatusChanged?.Invoke("Your turn! Repeat the sequence (Q W E R T)");
    }

    void HandlePlayerInput(KeyCode key)
    {
        playerInput.Add(key);
        int index = playerInput.Count - 1;
        var currentSequence = sequences[currentLevel];
        bool correct = key == currentSequence[index];
        InputFeedback?.Invoke(index, correct);
        if (correct)
        {
            if (playerInput.Count == currentSequence.Count)
            {
                score += currentLevel + 1;
                ScoreChanged?.Invoke(score);
                currentLevel++;
                if (currentLevel >= sequences.Count)
                {
                    OnGameWon();
                }
                else
                {
                    if (activeRoutine != null)
                        StopCoroutine(activeRoutine);
                    activeRoutine = StartCoroutine(OnLevelComplete());
                }
            }
        }
        else
        {
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(OnWrongAnswer());
        }
    }

    IEnumerator OnLevelComplete()
    {
        state = GameState.ShowingResult;
        LevelCompleted?.Invoke(currentLevel - 1);
        StatusChanged?.Invoke("Correct! Get ready for the next level...");
        yield return new WaitForSeconds(delayAfterCorrectLevel);
        StartLevel();
    }

    IEnumerator OnWrongAnswer()
    {
        state = GameState.ShowingResult;
        StatusChanged?.Invoke("Wrong! Restarting this level...");
        yield return new WaitForSeconds(delayAfterWrongAnswer);
        StartLevel();
    }

    void OnGameWon()
    {
        state = GameState.ShowingResult;
        gameEnded = true;
        GameWon?.Invoke();
        StatusChanged?.Invoke($"You Win! Final score: {score}. Press Space to play again.");
    }

    void OnDestroy()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
    }
}
