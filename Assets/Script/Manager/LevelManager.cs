using TMPro;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string levelName;

    [Header("Cấu hình Board")]
    public int boardRows = 3;
    public int boardCols = 3;

    [Header("Layout thủ công")]
    public float posY = 0f;
    public float width = 600f;
    public float height = 600f;
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 spacing = new Vector2(10, 10);

    [Header("Mục tiêu level")]
    public int redTarget = 0;
    public int blueTarget = 0;
    public int purpleTarget = 0;
    public int yellowTarget = 0;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Danh sách level")]
    public LevelData[] levels;
    public int currentLevelIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Length)
        {
            Debug.LogWarning("Level index không hợp lệ!");
            return;
        }

        // CẤU HÌNH LẠI BOARD CHO LEVEL NÀY
        ConfigureBoardForLevel(levels[index]);

        // CẬP NHẬT UI
        if (levelText != null)
            levelText.text = " " + levels[index].levelName;

        // SET MỤC TIÊU
        SetLevelGoals(levels[index]);

        Debug.Log("Đang chơi: " + levels[index].levelName);
    }

    private void ConfigureBoardForLevel(LevelData levelData)
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ConfigureBoard(levelData.boardRows, levelData.boardCols);
        }
    }

    private void SetLevelGoals(LevelData levelData)
    {
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.SetTargets(
                levelData.redTarget,
                levelData.blueTarget,
                levelData.purpleTarget,
                levelData.yellowTarget
            );
        }
    }

    public void CompleteLevel()
    {
        Debug.Log("Hoàn thành level " + levels[currentLevelIndex].levelName);

        int nextIndex = currentLevelIndex + 1;
        if (nextIndex < levels.Length)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            Debug.Log("Bạn đã hoàn thành tất cả các level!");
            if (levelText != null)
                levelText.text = "🎉 Hoàn thành game! 🎉";
        }
    }

    public bool IsCurrentLevelCompleted()
    {
        if (GoalManager.Instance != null)
        {
            return GoalManager.Instance.IsTargetCompleted();
        }
        return false;
    }
}