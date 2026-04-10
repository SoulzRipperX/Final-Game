using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameSetting : MonoBehaviour
{
    private const string SfxVolumeKey = "SFX_VOLUME";

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
    [SerializeField] private TMP_Text nextPrefabText;

    [Header("Audio")]
    [SerializeField] private AudioClip mergeSfx;

    [Header("Game Over")]
    [SerializeField] private float topBarLoseDelay = 5f;

    private float elapsedTime = 0f;
    private float topBarTouchTimer = 0f;
    private AudioSource mergeAudioSource;
    private readonly HashSet<Prefabs> prefabsTouchingTopBar = new HashSet<Prefabs>();

    private void Start()
    {
        UpdateHud();
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
        UpdateResultText();
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

        if (nextPrefabText != null)
        {
            nextPrefabText.text = previewName;
        }
    }

    private void UpdateHud()
    {
        UpdateScoreText();
        UpdateTimeText();
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
