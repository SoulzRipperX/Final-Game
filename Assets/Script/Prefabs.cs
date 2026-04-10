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
    private bool hasSpawnedNextAnimal = false;
    private Coroutine endGameCoroutine;
    public Dropper dropper;
    public bool isTouchingTopBar = false;


    void Start()
    {
        game = GameObject.Find("GameSetting").GetComponent<GameSetting>();
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
        hasSpawnedNextAnimal = false;
        isTouchingTopBar = false;

        if (endGameCoroutine != null)
        {
            StopCoroutine(endGameCoroutine);
            endGameCoroutine = null;
        }
    }

    public void Release()
    {
        hasBeenReleased = true;
    }

    IEnumerator DestroyAnimals(GameObject Animal1, GameObject Animal2)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(Animal1);
        Destroy(Animal2);
    }

    IEnumerator WaitForEndGame()
    {
        yield return new WaitForSeconds(5f);
        endGameCoroutine = null;

        if (isTouchingTopBar == true)
        {
            game.endGame();
        }
    }

    private void SpawnNextAnimalIfNeeded()
    {
        if (!hasBeenReleased || hasSpawnedNextAnimal || dropper == null || game.gameOver)
        {
            return;
        }

        hasSpawnedNextAnimal = true;

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

        isTouchingTopBar = true;

        if (endGameCoroutine == null)
        {
            endGameCoroutine = StartCoroutine(WaitForEndGame());
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(TopBarTag))
        {
            return;
        }

        isTouchingTopBar = false;

        if (endGameCoroutine != null)
        {
            StopCoroutine(endGameCoroutine);
            endGameCoroutine = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        SpawnNextAnimalIfNeeded();

        Prefabs otherAnimal = collision.gameObject.GetComponent<Prefabs>();

        if (!hasCollided && otherAnimal != null && !otherAnimal.hasCollided && otherAnimal.id == this.id && this.gameObject.GetInstanceID() < collision.gameObject.GetInstanceID())
        {
            hasCollided = true;
            otherAnimal.hasCollided = true;

            if (game.CanInstantiateAnimal(this.id + 1))
            {
                game.InstantiateAnimal((Vector2)this.transform.position, this.id + 1);
            }

            game.addScore(this.id);
            StartCoroutine(DestroyAnimals(this.gameObject, collision.gameObject));
        }
    }
}
