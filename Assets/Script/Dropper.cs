using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    private GameSetting game;
    public GameObject currentPrefabs;

    private void PrepareCurrentAnimal()
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
        game = GameObject.Find("GameSetting").GetComponent<GameSetting>();

        currentPrefabs = game.InstantiateAnimal((Vector2)this.transform.position, 0);
        if (currentPrefabs == null)
        {
            Debug.LogError("currentPrefabs is null in Start()");
        }
        else
        {
            PrepareCurrentAnimal();
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
        int randomAnimal = Random.Range(0, 5);

        currentPrefabs = game.InstantiateAnimal((Vector2)this.transform.position, randomAnimal);
        if (currentPrefabs == null)
        {
            Debug.LogError("currentPrefabs is null in CreatePrefabs()");
        }
        else
        {
            PrepareCurrentAnimal();
        }
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!game.gameOver && mousePos.x < 2.2f && mousePos.x > -2.2f)
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
}
