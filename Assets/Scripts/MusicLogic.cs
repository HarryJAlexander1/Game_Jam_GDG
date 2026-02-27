using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicMemoryGame : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip noteQ;
    [SerializeField] private AudioClip noteW;
    [SerializeField] private AudioClip noteE;
    [SerializeField] private AudioClip noteR;
    [SerializeField] private AudioClip noteT;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    [Header("Notes Display")]
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Transform staffParent;
    [SerializeField] private float spacing = 80f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Settings")]
    [SerializeField] private float notePlayDuration = 0.4f;
    [SerializeField] private float pauseBetweenNotes = 0.2f;
    [SerializeField] private float delayBeforeInput = 0.5f;
    [SerializeField] private float delayAfterWrongAnswer = 1.5f;
    [SerializeField] private float delayAfterCorrectLevel = 1.0f;
    [SerializeField] private int startingSequenceLength = 4;
    [SerializeField] private int lengthIncreasePerLevel = 2;
    [SerializeField] private int totalLevels = 5;

    [Header("Note Colors")]
    [SerializeField] private Color defaultNoteColor = Color.white;
    [SerializeField] private Color highlightNoteColor = Color.yellow;
    [SerializeField] private Color correctNoteColor = Color.green;
    [SerializeField] private Color wrongNoteColor = Color.red;

    private static readonly Dictionary<KeyCode, float> NoteYOffsets = new Dictionary<KeyCode, float>
    {
        { KeyCode.Q, -40f },
        { KeyCode.W, -20f },
        { KeyCode.E, 0f },
        { KeyCode.R, 20f },
        { KeyCode.T, 40f }
    };

    private static readonly KeyCode[] InputKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T };

    private enum GameState
    {
        Idle,
        PlayingSequence,
        WaitingForInput,
        ShowingResult
    }

    private Dictionary<KeyCode, AudioClip> keyToClip;
    private List<List<KeyCode>> sequences = new List<List<KeyCode>>();
    private List<KeyCode> playerInput = new List<KeyCode>();
    private List<Image> spawnedNotes = new List<Image>();

    private int currentLevel;
    private int score;
    private GameState state = GameState.Idle;
    private Coroutine activeRoutine;

    void Start()
    {
        keyToClip = new Dictionary<KeyCode, AudioClip>
        {
            { KeyCode.Q, noteQ },
            { KeyCode.W, noteW },
            { KeyCode.E, noteE },
            { KeyCode.R, noteR },
            { KeyCode.T, noteT }
        };

        ValidateSetup();
        StartNewGame();
    }

    void ValidateSetup()
    {
        foreach (var pair in keyToClip)
        {
            if (pair.Value == null)
                Debug.LogWarning($"MusicMemoryGame: AudioClip for key {pair.Key} is not assigned!");
        }

        if (audioSource == null)
            Debug.LogError("MusicMemoryGame: AudioSource is not assigned!");

        if (notePrefab == null)
            Debug.LogError("MusicMemoryGame: Note prefab is not assigned!");

        if (staffParent == null)
            Debug.LogError("MusicMemoryGame: Staff parent is not assigned!");
    }

    public void StartNewGame()
    {
        currentLevel = 0;
        score = 0;
        GenerateSequences();
        UpdateUI();
        StartLevel();
    }

    void GenerateSequences()
    {
        sequences.Clear();
        int length = startingSequenceLength;

        for (int i = 0; i < totalLevels; i++)
        {
            List<KeyCode> seq = new List<KeyCode>();

            for (int j = 0; j < length; j++)
                seq.Add(InputKeys[Random.Range(0, InputKeys.Length)]);

            sequences.Add(seq);
            length += lengthIncreasePerLevel;
        }
    }

    void StartLevel()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(PlaySequenceRoutine());
    }

    IEnumerator PlaySequenceRoutine()
    {
        state = GameState.PlayingSequence;
        playerInput.Clear();
        ClearNotes();
        UpdateUI();
        SetStatus($"Watch & Listen... (Level {currentLevel + 1})");

        yield return new WaitForSeconds(delayBeforeInput);

        List<KeyCode> sequence = sequences[currentLevel];

        for (int i = 0; i < sequence.Count; i++)
            SpawnNote(sequence[i], i);

        for (int i = 0; i < sequence.Count; i++)
        {
            spawnedNotes[i].color = highlightNoteColor;
            PlaySound(sequence[i]);

            yield return new WaitForSeconds(notePlayDuration);

            spawnedNotes[i].color = defaultNoteColor;

            yield return new WaitForSeconds(pauseBetweenNotes);
        }

        yield return new WaitForSeconds(delayBeforeInput);

        state = GameState.WaitingForInput;
        SetStatus("Your turn! Repeat the sequence (Q W E R T)");
    }

    void SpawnNote(KeyCode key, int index)
    {
        GameObject obj = Instantiate(notePrefab, staffParent);
        RectTransform rt = obj.GetComponent<RectTransform>();

        float yOffset = NoteYOffsets.ContainsKey(key) ? NoteYOffsets[key] : 0f;
        rt.anchoredPosition = new Vector2(index * spacing, yOffset);

        Image img = obj.GetComponent<Image>();
        img.color = defaultNoteColor;
        spawnedNotes.Add(img);
    }

    void ClearNotes()
    {
        foreach (Image img in spawnedNotes)
        {
            if (img != null)
                Destroy(img.gameObject);
        }

        spawnedNotes.Clear();
    }

    void Update()
    {
        if (state != GameState.WaitingForInput)
            return;

        foreach (KeyCode key in InputKeys)
        {
            if (Input.GetKeyDown(key))
            {
                HandlePlayerInput(key);
                break;
            }
        }
    }

    void HandlePlayerInput(KeyCode key)
    {
        PlaySound(key);
        playerInput.Add(key);

        int index = playerInput.Count - 1;
        List<KeyCode> currentSequence = sequences[currentLevel];

        if (key == currentSequence[index])
        {
            spawnedNotes[index].color = correctNoteColor;

            if (playerInput.Count == currentSequence.Count)
            {
                score += currentLevel + 1;
                currentLevel++;
                UpdateUI();

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
            spawnedNotes[index].color = wrongNoteColor;

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(OnWrongAnswer());
        }
    }

    IEnumerator OnLevelComplete()
    {
        state = GameState.ShowingResult;
        SetStatus("Correct! Get ready for the next level...");

        if (correctSound != null)
            audioSource.PlayOneShot(correctSound);

        yield return new WaitForSeconds(delayAfterCorrectLevel);

        StartLevel();
    }

    IEnumerator OnWrongAnswer()
    {
        state = GameState.ShowingResult;
        SetStatus("Wrong! Restarting this level...");

        if (wrongSound != null)
            audioSource.PlayOneShot(wrongSound);

        for (int i = playerInput.Count; i < spawnedNotes.Count; i++)
            spawnedNotes[i].color = wrongNoteColor;

        yield return new WaitForSeconds(delayAfterWrongAnswer);

        StartLevel();
    }

    void OnGameWon()
    {
        state = GameState.ShowingResult;
        SetStatus($"You Win! Final score: {score}. Press Space to play again.");

        if (correctSound != null)
            audioSource.PlayOneShot(correctSound);

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(WaitForRestart());
    }

    IEnumerator WaitForRestart()
    {
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        StartNewGame();
    }

    void PlaySound(KeyCode key)
    {
        if (keyToClip.TryGetValue(key, out AudioClip clip) && clip != null)
            audioSource.PlayOneShot(clip);
        else
            Debug.LogWarning($"MusicMemoryGame: No clip assigned for key {key}");
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Level: {currentLevel + 1} / {totalLevels}";

        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void OnDestroy()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
    }
}
