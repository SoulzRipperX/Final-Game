using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameSetting : MonoBehaviour
{
    private const string SfxVolumeKey = "SFX_VOLUME";
    private const string LeaderboardSaveKey = "LEADERBOARD_DATA";

    [Header("Gameplay")]
    public GameObject[] Prefabs;
    public int score = 0;
    public bool gameOver = false;

    [Header("UI")]
    public TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text resultText;
    [FormerlySerializedAs("playAgainButton")]
    [SerializeField] private GameObject overCanvas;

    [Header("Next Preview")]
    [SerializeField] private Image nextPrefabImage;
    [SerializeField] private SpriteRenderer nextPrefabSpriteRenderer;

    [Header("Leaderboard")]
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private int leaderboardSize = 5;

    [Header("Audio")]
    [SerializeField] private AudioClip mergeSfx;

    [Header("Game Over")]
    [SerializeField] private float topBarLoseDelay = 5f;

    private float elapsedTime = 0f;
    private float topBarTouchTimer = 0f;
    private AudioSource mergeAudioSource;
    private readonly HashSet<Prefabs> prefabsTouchingTopBar = new HashSet<Prefabs>();
    private LeaderboardData leaderboardData = new LeaderboardData();

    [System.Serializable]
    private class LeaderboardEntry
    {
        public int score;
        public float playTime;
    }

    [System.Serializable]
    private class LeaderboardData
    {
        public int bestScore;
        public float bestPlayTime;
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    private void Start()
    {
        LoadLeaderboard();
        UpdateHud();
        RefreshLeaderboardUi();
        SetOverCanvasVisibility(false);
        SetResultVisibility(false);
    }

    private void Update()
    {
        if (gameOver)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        UpdateTimeText();
        UpdateBestScoreText();
        UpdateTopBarLoseState();
    }

    public bool CanInstantiatePrefabs(int id)
    {
        return id >= 0 && id < Prefabs.Length;
    }

    public GameObject InstantiatePrefabs(Vector2 position, int id)
    {
        if (!CanInstantiatePrefabs(id))
        {
            return null;
        }

        Vector3 spawnPosition = new Vector3(position.x, position.y, Prefabs[id].transform.position.z);
        GameObject combinedPrefabs = Instantiate(Prefabs[id], spawnPosition, Quaternion.identity);
        combinedPrefabs.GetComponent<Prefabs>().id = id;
        return combinedPrefabs;

    }

    public void addScore(int id)
    {
        switch (id)
        {
            case 0:
                score += 1;
                break;
            case 1:
                score += 3;
                break;
            case 2:
                score += 6;
                break;
            case 3:
                score += 10;
                break;
            case 4:
                score += 15;
                break;
            case 5:
                score += 21;
                break;
            case 6:
                score += 28;
                break;
            case 7:
                score += 36;
                break;
            case 8:
                score += 45;
                break;
            case 9:
                score += 55;
                break;
            case 10:
                score += 66;
                break;
        }

        UpdateScoreText();
    }

    public void endGame()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;
        prefabsTouchingTopBar.Clear();
        topBarTouchTimer = 0f;
        SaveCurrentRunToLeaderboard();
        UpdateResultText();
        RefreshLeaderboardUi();
        SetOverCanvasVisibility(true);
        SetResultVisibility(true);
    }

    public void PlayMergeSfx()
    {
        if (mergeSfx == null)
        {
            return;
        }

        EnsureMergeAudioSource();
        mergeAudioSource.volume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        mergeAudioSource.PlayOneShot(mergeSfx);
    }

    public void playAgain()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void SetTopBarContact(Prefabs prefab, bool isTouching)
    {
        if (prefab == null)
        {
            return;
        }

        if (isTouching)
        {
            prefabsTouchingTopBar.Add(prefab);
        }
        else
        {
            prefabsTouchingTopBar.Remove(prefab);
        }
    }

    public void UpdateNextPrefabPreview(int prefabId)
    {
        Sprite previewSprite = GetPrefabSprite(prefabId);
        string previewName = GetPrefabDisplayName(prefabId);

        if (nextPrefabImage != null)
        {
            nextPrefabImage.sprite = previewSprite;
            nextPrefabImage.enabled = previewSprite != null;
            nextPrefabImage.preserveAspect = true;
        }

        if (nextPrefabSpriteRenderer != null)
        {
            nextPrefabSpriteRenderer.sprite = previewSprite;
            nextPrefabSpriteRenderer.enabled = previewSprite != null;
        }
    }

    private void UpdateHud()
    {
        UpdateScoreText();
        UpdateTimeText();
        UpdateBestScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score;
        }
    }

    private void UpdateTimeText()
    {
        if (timeText != null)
        {
            timeText.text = "TIME: " + FormatTime(elapsedTime);
        }
    }

    private void UpdateResultText()
    {
        if (resultText != null)
        {
            resultText.text = "GAME OVER\nSCORE: " + score + "\nTIME: " + FormatTime(elapsedTime);
        }
    }

    private void UpdateBestScoreText()
    {
        if (bestScoreText != null)
        {
            int displayedBestScore = Mathf.Max(leaderboardData.bestScore, score);
            bestScoreText.text = "BEST SCORE: " + displayedBestScore;
        }
    }

    private void SetResultVisibility(bool isVisible)
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(isVisible);
        }
    }

    private void SetOverCanvasVisibility(bool isVisible)
    {
        if (overCanvas != null)
        {
            overCanvas.SetActive(isVisible);
        }
    }

    private void UpdateTopBarLoseState()
    {
        if (prefabsTouchingTopBar.Count == 0)
        {
            topBarTouchTimer = 0f;
            return;
        }

        topBarTouchTimer += Time.deltaTime;

        if (topBarTouchTimer >= topBarLoseDelay)
        {
            endGame();
        }
    }

    private void RefreshLeaderboardUi()
    {
        UpdateBestScoreText();
        UpdateLeaderboardText();
    }

    private void UpdateLeaderboardText()
    {
        if (leaderboardText == null)
        {
            return;
        }

        if (leaderboardData.entries == null || leaderboardData.entries.Count == 0)
        {
            leaderboardText.text = "\nNO RECORD";
            return;
        }

        StringBuilder builder = new StringBuilder();


        int rank = 1;
        foreach (LeaderboardEntry entry in leaderboardData.entries)
        {
            builder.Append(rank);
            builder.Append(". SCORE ");
            builder.Append(entry.score);
            builder.Append(" | PLAY TIME ");
            builder.Append(FormatTime(entry.playTime));
            builder.AppendLine();
            rank++;
        }

        leaderboardText.text = builder.ToString().TrimEnd();
    }

    private void SaveCurrentRunToLeaderboard()
    {
        if (leaderboardData.entries == null)
        {
            leaderboardData.entries = new List<LeaderboardEntry>();
        }

        leaderboardData.entries.Add(new LeaderboardEntry
        {
            score = score,
            playTime = elapsedTime
        });

        leaderboardData.entries.Sort((left, right) => right.score.CompareTo(left.score));

        int maxEntries = Mathf.Max(1, leaderboardSize);
        if (leaderboardData.entries.Count > maxEntries)
        {
            leaderboardData.entries.RemoveRange(maxEntries, leaderboardData.entries.Count - maxEntries);
        }

        leaderboardData.bestScore = Mathf.Max(leaderboardData.bestScore, score);
        leaderboardData.bestPlayTime = Mathf.Max(leaderboardData.bestPlayTime, elapsedTime);

        SaveLeaderboard();
    }

    private void LoadLeaderboard()
    {
        if (!PlayerPrefs.HasKey(LeaderboardSaveKey))
        {
            leaderboardData = new LeaderboardData();
            return;
        }

        string savedJson = PlayerPrefs.GetString(LeaderboardSaveKey, string.Empty);
        if (string.IsNullOrEmpty(savedJson))
        {
            leaderboardData = new LeaderboardData();
            return;
        }

        leaderboardData = JsonUtility.FromJson<LeaderboardData>(savedJson);
        if (leaderboardData == null)
        {
            leaderboardData = new LeaderboardData();
        }

        if (leaderboardData.entries == null)
        {
            leaderboardData.entries = new List<LeaderboardEntry>();
        }
    }

    private void SaveLeaderboard()
    {
        string leaderboardJson = JsonUtility.ToJson(leaderboardData);
        PlayerPrefs.SetString(LeaderboardSaveKey, leaderboardJson);
        PlayerPrefs.Save();
    }

    private Sprite GetPrefabSprite(int prefabId)
    {
        if (!CanInstantiatePrefabs(prefabId))
        {
            return null;
        }

        SpriteRenderer spriteRenderer = Prefabs[prefabId].GetComponent<SpriteRenderer>();
        return spriteRenderer != null ? spriteRenderer.sprite : null;
    }

    private string GetPrefabDisplayName(int prefabId)
    {
        if (!CanInstantiatePrefabs(prefabId))
        {
            return string.Empty;
        }

        return "NEXT: " + Prefabs[prefabId].name;
    }

    private void EnsureMergeAudioSource()
    {
        if (mergeAudioSource != null)
        {
            return;
        }

        mergeAudioSource = GetComponent<AudioSource>();

        if (mergeAudioSource == null)
        {
            mergeAudioSource = gameObject.AddComponent<AudioSource>();
        }

        mergeAudioSource.playOnAwake = false;
        mergeAudioSource.loop = false;
        mergeAudioSource.spatialBlend = 0f;
    }

    private static string FormatTime(float timeInSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeInSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
