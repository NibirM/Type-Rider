using UnityEngine;

public class RestartButton : MonoBehaviour
{
    public CarMovement car;
    public LapManager lapManager;
    public TypingSystem typingSystem;
    public LiveryManager liveryManager;

    private void Awake()
    {
        Debug.Log("RestartButton.Awake running on '" + gameObject.name + "'.", this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Restart();
        }
    }

    public void Restart()
    {
        Debug.Log("RestartButton.Restart() called.", this);

        if (car != null)
        {
            car.RestartRace();
        }
        else
        {
            Debug.LogWarning("RestartButton: Car reference not assigned.", this);
        }

        if (lapManager != null)
        {
            lapManager.ResetRace();
        }
        else
        {
            Debug.LogWarning("RestartButton: LapManager reference not assigned.", this);
        }

        if (typingSystem != null)
        {
            typingSystem.ResetSession();
        }
        else
        {
            Debug.LogWarning("RestartButton: TypingSystem reference not assigned.", this);
        }

        if (liveryManager != null)
        {
            liveryManager.ResetLivery();
        }
    }
}