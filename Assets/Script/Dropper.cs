using UnityEngine;

public class Dropper : MonoBehaviour
{
    private const int RandomPrefabPoolSize = 5;
    private const float MouseResumeThresholdPixels = 1f;

    [Header("Movement")]
    [SerializeField] private float minX = -2.58f;
    [SerializeField] private float maxX = 2.58f;
    [SerializeField] private float holdY = 2f;
    [SerializeField] private bool mouseControlEnabled = true;
    [SerializeField] private float spawnDelay = 0.2f;

    private GameSetting game;
    public GameObject currentPrefabs;
    private int nextPrefabId;
    private bool keyboardControlActive;
    private Vector3 lastMouseScreenPosition;

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
        lastMouseScreenPosition = Input.mousePosition;
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

        Invoke(nameof(CreatePrefabs), spawnDelay);
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
        EnsureCurrentPrefabsExists();

        if (mouseControlEnabled && Camera.main != null && !game.gameOver)
        {
            UpdateMouseControl();
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

        GameObject releasedPrefabs = currentPrefabs;
        Rigidbody2D rb2d = releasedPrefabs.GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            return;
        }

        Prefabs prefabs = releasedPrefabs.GetComponent<Prefabs>();
        if (prefabs != null)
        {
            prefabs.Release();
        }

        rb2d.gravityScale = 1;
        currentPrefabs = null;
        SpawnNewPrefabs();
    }

    public void NotifyExternalControlInput()
    {
        keyboardControlActive = true;
        lastMouseScreenPosition = Input.mousePosition;
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

    private void EnsureCurrentPrefabsExists()
    {
        if (game == null || game.gameOver || currentPrefabs != null || IsInvoking(nameof(CreatePrefabs)))
        {
            return;
        }

        SpawnNewPrefabs();
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

    private void UpdateMouseControl()
    {
        Vector3 currentMouseScreenPosition = Input.mousePosition;
        bool hasMouseMoved = HasMouseMoved(currentMouseScreenPosition);

        if (!keyboardControlActive || hasMouseMoved)
        {
            keyboardControlActive = false;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(currentMouseScreenPosition);
            MoveToX(mousePos.x);
        }

        lastMouseScreenPosition = currentMouseScreenPosition;
    }

    private bool HasMouseMoved(Vector3 currentMouseScreenPosition)
    {
        return (currentMouseScreenPosition - lastMouseScreenPosition).sqrMagnitude >= MouseResumeThresholdPixels * MouseResumeThresholdPixels;
    }
}
