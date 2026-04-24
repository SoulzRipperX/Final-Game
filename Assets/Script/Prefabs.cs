using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    private const string TopBarTag = "TopBar";

    public int id;
    private GameSetting game;
    private bool hasCollided = false;
    private bool hasBeenReleased = false;
    private bool hasSpawnedNextPrefabs = false;
    private int topBarOverlapCount = 0;
    public Dropper dropper;
    public bool isTouchingTopBar = false;


    void Start()
    {
        game = GameObject.Find("GameController").GetComponent<GameSetting>();
        if (dropper == null)
        {
            dropper = GameObject.Find("Dropper").GetComponent<Dropper>();
        }
    }

    public void PrepareForDrop(Dropper owningDropper)
    {
        dropper = owningDropper;
        hasCollided = false;
        hasBeenReleased = false;
        hasSpawnedNextPrefabs = false;
        topBarOverlapCount = 0;
        isTouchingTopBar = false;
        SetPhysicalCollisionsEnabled(false);
    }

    public void Release()
    {
        hasBeenReleased = true;
        SetPhysicalCollisionsEnabled(true);

        if (isTouchingTopBar)
        {
            game.SetTopBarContact(this, true);
        }
    }

    private void SpawnNextPrefabsIfNeeded()
    {
        if (!hasBeenReleased || hasSpawnedNextPrefabs || dropper == null || game.gameOver)
        {
            return;
        }

        hasSpawnedNextPrefabs = true;

        if (dropper.currentPrefabs == this.gameObject)
        {
            dropper.currentPrefabs = null;
            dropper.SpawnNewPrefabs();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(TopBarTag))
        {
            return;
        }

        topBarOverlapCount++;
        RefreshTopBarState();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag(TopBarTag))
        {
            return;
        }

        if (topBarOverlapCount == 0)
        {
            topBarOverlapCount = 1;
        }

        RefreshTopBarState();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(TopBarTag))
        {
            return;
        }

        topBarOverlapCount = Mathf.Max(0, topBarOverlapCount - 1);
        RefreshTopBarState();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnDisable()
    {
        topBarOverlapCount = 0;
        isTouchingTopBar = false;

        if (game != null)
        {
            game.SetTopBarContact(this, false);
        }
    }

    private IEnumerator DestroyPrefabs(GameObject prefabs1, GameObject prefabs2)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(prefabs1);
        Destroy(prefabs2);
    }

    private void RefreshTopBarState()
    {
        isTouchingTopBar = topBarOverlapCount > 0;

        if (game == null)
        {
            return;
        }

        if (hasBeenReleased && isTouchingTopBar)
        {
            game.SetTopBarContact(this, true);
        }
        else
        {
            game.SetTopBarContact(this, false);
        }
    }

    private void SetPhysicalCollisionsEnabled(bool isEnabled)
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            if (!collider.isTrigger)
            {
                collider.enabled = isEnabled;
            }
        }
    }

    private void HandleCollision(Collision2D collision)
    {
        SpawnNextPrefabsIfNeeded();

        Prefabs otherPrefabs = collision.gameObject.GetComponent<Prefabs>();

        if (hasCollided || otherPrefabs == null || otherPrefabs.hasCollided || otherPrefabs.id != id || game == null)
        {
            return;
        }

        if (gameObject.GetInstanceID() > collision.gameObject.GetInstanceID())
        {
            return;
        }

        hasCollided = true;
        otherPrefabs.hasCollided = true;

        if (game.CanInstantiatePrefabs(id + 1))
        {
            game.InstantiatePrefabs((Vector2)transform.position, id + 1);
        }

        game.addScore(id);
        game.PlayMergeSfx();
        StartCoroutine(DestroyPrefabs(gameObject, collision.gameObject));
    }
}
