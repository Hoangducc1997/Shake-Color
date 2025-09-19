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

        // Ẩn panel khi bắt đầu
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
    }

    /// <summary>
    /// Gọi khi thua game
    /// </summary>
    public void GameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // dừng game
        Debug.Log("Game Over!");
    }

    /// <summary>
    /// Gọi khi thắng level
    /// </summary>
    public void Victory()
    {
        if (victoryPanel) victoryPanel.SetActive(true);
        Time.timeScale = 0f; // dừng game
        Debug.Log("Victory!");
    }

    /// <summary>
    /// Restart level hiện tại
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);

        // Reset lại board mới
        if (BoardManager.Instance != null)
            BoardManager.Instance.ResetBoard();

        foreach (var spawner in SpawnerManager.Instances)
        {
            spawner.RestartSpawner();
        }

        // Load lại level hiện tại
        LevelManager.Instance.LoadLevel(LevelManager.Instance.currentLevelIndex);
    }


    /// <summary>
    /// Load level tiếp theo sau khi thắng
    /// </summary>
    public void NextLevel()
    {
        Time.timeScale = 1f;
        if (victoryPanel) victoryPanel.SetActive(false);

        LevelManager.Instance.CompleteLevel();
    }

    /// <summary>
    /// Exit game
    /// <summary>
    public void ExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }
}
