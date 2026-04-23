using UnityEngine;

public class DropperKeyboardController : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private string dropperObjectName = "Dropper";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;

    [Header("String Patterns")]
    [SerializeField] private string[] moveLeftPatterns = { "left", "a" };
    [SerializeField] private string[] moveRightPatterns = { "right", "d" };
    [SerializeField] private string[] dropPatterns = { "space", "down" };

    private Dropper dropper;

    private void Start()
    {
        ConnectDropper();
    }

    private void Update()
    {
        if (dropper == null)
        {
            ConnectDropper();
            return;
        }

        float direction = GetHorizontalDirection();
        if (!Mathf.Approximately(direction, 0f))
        {
            dropper.MoveBy(direction * moveSpeed * Time.deltaTime);
        }

        if (IsAnyPatternDown(dropPatterns))
        {
            dropper.DropCurrentPrefabs();
        }
    }

    private void ConnectDropper()
    {
        GameObject dropperObject = GameObject.Find(dropperObjectName);
        if (dropperObject != null)
        {
            dropper = dropperObject.GetComponent<Dropper>();
        }
    }

    private float GetHorizontalDirection()
    {
        float direction = 0f;

        if (IsAnyPatternHeld(moveLeftPatterns))
        {
            direction -= 1f;
        }

        if (IsAnyPatternHeld(moveRightPatterns))
        {
            direction += 1f;
        }

        return direction;
    }

    private static bool IsAnyPatternHeld(string[] patterns)
    {
        if (patterns == null)
        {
            return false;
        }

        foreach (string pattern in patterns)
        {
            if (!string.IsNullOrWhiteSpace(pattern) && Input.GetKey(pattern))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAnyPatternDown(string[] patterns)
    {
        if (patterns == null)
        {
            return false;
        }

        foreach (string pattern in patterns)
        {
            if (!string.IsNullOrWhiteSpace(pattern) && Input.GetKeyDown(pattern))
            {
                return true;
            }
        }

        return false;
    }
}
