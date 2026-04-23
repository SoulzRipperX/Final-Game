using UnityEngine;

public class Dropper : MonoBehaviour
{
    private const int RandomPrefabPoolSize = 5;

    [Header("Movement")]
    [SerializeField] private float minX = -2.58f;
    [SerializeField] private float maxX = 2.58f;
    [SerializeField] private float holdY = 2f;
    [SerializeField] private bool mouseControlEnabled = true;

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
        if (mouseControlEnabled && Camera.main != null && !game.gameOver)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MoveToX(mousePos.x);
        }

        UpdateHeldPrefabPosition();

        if (mouseControlEnabled && !game.gameOver && Input.GetMouseButtonDown(0))
        {
            DropCurrentPrefabs();
        }
    }

    public void MoveBy(float deltaX)
    {
        MoveToX(transform.position.x + deltaX);
    }

    public void MoveToX(float targetX)
    {
        if (game == null || game.gameOver)
        {
            return;
        }

        Vector3 targetPosition = transform.position;
        targetPosition.x = Mathf.Clamp(targetX, minX, maxX);
        targetPosition.y = holdY;
        transform.position = targetPosition;
    }

    public void DropCurrentPrefabs()
    {
        if (game == null || game.gameOver || currentPrefabs == null)
        {
            return;
        }

        Rigidbody2D rb2d = currentPrefabs.GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            return;
        }

        Prefabs prefabs = currentPrefabs.GetComponent<Prefabs>();
        if (prefabs != null)
        {
            prefabs.Release();
        }

        rb2d.gravityScale = 1;
    }

    private void UpdateHeldPrefabPosition()
    {
        if (currentPrefabs == null)
        {
            return;
        }

        Rigidbody2D rb2d = currentPrefabs.GetComponent<Rigidbody2D>();
        if (rb2d != null && rb2d.gravityScale == 0)
        {
            currentPrefabs.transform.position = transform.position;
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
