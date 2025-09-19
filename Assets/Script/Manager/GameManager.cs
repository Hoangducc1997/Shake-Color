using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
    }

    public void GameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Game Over!");
    }

    public void Victory()
    {
        if (victoryPanel) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Victory!");
    }

    /// <summary>
    /// Restart level hiện tại
    /// </summary>
    public void RestartLevel()
    {
        AudioManager.Instance.PlayVFX("Choose");
        Time.timeScale = 1f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (BoardManager.ActiveBoard != null)
        {
            BoardManager.ActiveBoard.ResetBoard();
        }
        else if (BoardManager.Instances.Count > 0)
        {
            // Fallback: dùng board đầu tiên nếu không có active board
            BoardManager.Instances[0].ResetBoard();
        }

        // Restart tất cả Spawner
        foreach (var spawner in SpawnerManager.Instances)
        {
            if (spawner != null)
            {
                spawner.RestartSpawner();
            }
        }
    }

    /// <summary>
    /// Load level tiếp theo sau khi thắng
    /// </summary>
    public void NextLevel()
    {
        AudioManager.Instance.PlayVFX("Choose");
        Time.timeScale = 1f;
        if (victoryPanel) victoryPanel.SetActive(false);
        LevelManager.Instance.CompleteLevel();
    }

    /// <summary>
    /// Exit game
    /// <summary>
    public void ExitGame()
    {
        AudioManager.Instance.PlayVFX("Choose");
        Debug.Log("Exit Game");
        Application.Quit();
    }
}