using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryGameView : MonoBehaviour
{
    [Header("Logic")]
    [SerializeField] private MemoryGameLogic logic;

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

    [Header("Note Colors")]
    [SerializeField] private Color defaultNoteColor = Color.white;
    [SerializeField] private Color highlightNoteColor = Color.yellow;
    [SerializeField] private Color correctNoteColor = Color.green;
    [SerializeField] private Color wrongNoteColor = Color.red;

    private Dictionary<KeyCode, AudioClip> keyToClip;
    private static readonly Dictionary<KeyCode, float> NoteYOffsets = new Dictionary<KeyCode, float>
    {
        { KeyCode.Q, -40f },
        { KeyCode.W, -20f },
        { KeyCode.E, 0f },
        { KeyCode.R, 20f },
        { KeyCode.T, 40f }
    };

    private List<Image> spawnedNotes = new List<Image>();
    private List<KeyCode> currentSequence = new List<KeyCode>();

    void Awake()
    {
        keyToClip = new Dictionary<KeyCode, AudioClip>
        {
            { KeyCode.Q, noteQ },
            { KeyCode.W, noteW },
            { KeyCode.E, noteE },
            { KeyCode.R, noteR },
            { KeyCode.T, noteT }
        };
    }

    void OnEnable()
    {
        if (logic == null) return;
        logic.LevelStarted += OnLevelStarted;
        logic.LevelCompleted += OnLevelCompleted;
        logic.SequencePrepared += OnSequencePrepared;
        logic.NotePlaybackStart += OnNotePlaybackStart;
        logic.NotePlaybackEnd += OnNotePlaybackEnd;
        logic.InputFeedback += OnInputFeedback;
        logic.StatusChanged += OnStatusChanged;
        logic.ScoreChanged += OnScoreChanged;
        logic.GameWon += OnGameWon;
        logic.ClearRequested += OnClearRequested;
    }

    void OnDisable()
    {
        if (logic == null) return;
        logic.LevelStarted -= OnLevelStarted;
        logic.LevelCompleted -= OnLevelCompleted;
        logic.SequencePrepared -= OnSequencePrepared;
        logic.NotePlaybackStart -= OnNotePlaybackStart;
        logic.NotePlaybackEnd -= OnNotePlaybackEnd;
        logic.InputFeedback -= OnInputFeedback;
        logic.StatusChanged -= OnStatusChanged;
        logic.ScoreChanged -= OnScoreChanged;
        logic.GameWon -= OnGameWon;
        logic.ClearRequested -= OnClearRequested;
    }

    void OnLevelStarted(int levelIndex)
    {
        if (levelText != null)
            levelText.text = $"Level: {levelIndex + 1}";
    }

    void OnLevelCompleted(int levelIndex)
    {
        if (correctSound != null && audioSource != null)
            audioSource.PlayOneShot(correctSound);
    }

    void OnSequencePrepared(List<KeyCode> sequence)
    {
        currentSequence = sequence;
        ClearNotes();
        for (int i = 0; i < sequence.Count; i++)
            SpawnNote(sequence[i], i);
    }

    void OnNotePlaybackStart(int index, KeyCode key)
    {
        if (index >= 0 && index < spawnedNotes.Count)
            spawnedNotes[index].color = highlightNoteColor;
        PlaySound(key);
    }

    void OnNotePlaybackEnd(int index, KeyCode key)
    {
        if (index >= 0 && index < spawnedNotes.Count)
            spawnedNotes[index].color = defaultNoteColor;
    }

    void OnInputFeedback(int index, bool correct)
    {
        if (index >= 0 && index < spawnedNotes.Count)
            spawnedNotes[index].color = correct ? correctNoteColor : wrongNoteColor;
        if (!correct && wrongSound != null && audioSource != null)
            audioSource.PlayOneShot(wrongSound);
    }

    void OnStatusChanged(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void OnScoreChanged(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    void OnGameWon()
    {
        if (correctSound != null && audioSource != null)
            audioSource.PlayOneShot(correctSound);
    }

    void OnClearRequested()
    {
        ClearNotes();
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

    void PlaySound(KeyCode key)
    {
        if (audioSource == null) return;
        if (keyToClip.TryGetValue(key, out AudioClip clip) && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
