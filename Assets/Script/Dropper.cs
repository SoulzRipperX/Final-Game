using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    private const int RandomPrefabPoolSize = 5;

    private GameSetting game;
    public GameObject currentPrefabs;
    private int nextPrefabId;

    private void PrepareCurrentPrefabs()
    {
        if (currentPrefabs == null)
        {
            return;
        }

        Prefabs prefabs = currentPrefabs.GetComponent<Prefabs>();
        if (prefabs != null)
        {
            prefabs.PrepareForDrop(this);
        }

        Rigidbody2D rb2d = currentPrefabs.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.gravityScale = 0;
        }
    }

    void Start()
    {
        game = GameObject.Find("GameController").GetComponent<GameSetting>();
        nextPrefabId = GetRandomPrefabId();
        game.UpdateNextPrefabPreview(nextPrefabId);

        currentPrefabs = game.InstantiatePrefabs((Vector2)this.transform.position, 0);
        if (currentPrefabs == null)
        {
            Debug.LogError("currentPrefabs is null in Start()");
        }
        else
        {
            PrepareCurrentPrefabs();
        }
    }

    public void SpawnNewPrefabs()
    {
        if (game.gameOver || currentPrefabs != null || IsInvoking(nameof(CreatePrefabs)))
        {
            return;
        }

        Invoke(nameof(CreatePrefabs), 0.1f);
    }

    private void CreatePrefabs()
    {
        currentPrefabs = game.InstantiatePrefabs((Vector2)this.transform.position, nextPrefabId);
        if (currentPrefabs == null)
        {
            Debug.LogError("currentPrefabs is null in CreatePrefabs()");
        }
        else
        {
            PrepareCurrentPrefabs();
        }

        nextPrefabId = GetRandomPrefabId();
        game.UpdateNextPrefabPreview(nextPrefabId);
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!game.gameOver && mousePos.x < 2.58f && mousePos.x > -2.58f)
        {
            mousePos.y = 3f;
            transform.position = mousePos;
        }

        if (currentPrefabs != null)
        {
            var rb2d = currentPrefabs.GetComponent<Rigidbody2D>();
            if (rb2d != null && rb2d.gravityScale == 0)
            {
                currentPrefabs.transform.position = this.transform.position;
            }
        }

        if (!game.gameOver && Input.GetMouseButtonDown(0))
        {
            if (currentPrefabs != null)
            {
                var rb2d = currentPrefabs.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    Prefabs prefabs = currentPrefabs.GetComponent<Prefabs>();
                    if (prefabs != null)
                    {
                        prefabs.Release();
                    }

                    rb2d.gravityScale = 1;
                }
            }
        }
    }

    private int GetRandomPrefabId()
    {
        int maxExclusive = Mathf.Min(RandomPrefabPoolSize, game.Prefabs.Length);

        if (maxExclusive <= 0)
        {
            return 0;
        }

        return Random.Range(0, maxExclusive);
    }
}
