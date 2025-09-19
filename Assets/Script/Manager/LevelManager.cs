using TMPro;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public GameObject levelObj; // Obj level có sẵn trong scene

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
        if (index < 0 || index >= levels.Length) return;

        // TẮT TẤT CẢ LEVEL
        foreach (LevelData level in levels)
        {
            if (level.levelObj != null)
                level.levelObj.SetActive(false);
        }

        // BẬT LEVEL HIỆN TẠI
        if (levels[index].levelObj != null)
        {
            levels[index].levelObj.SetActive(true);

            // ĐẢM BẢO BOARD MANAGER ĐƯỢC SET ACTIVE
            BoardManager levelBoard = levels[index].levelObj.GetComponent<BoardManager>();
            if (levelBoard != null)
            {
                levelBoard.SetAsActiveBoard();
            }

            currentLevelIndex = index;
            if (levelText != null)
                levelText.text = "Level " + levels[index].levelName;

            SetLevelGoals(levels[index]);

            Debug.Log($"Loaded level: {levels[index].levelName}");
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
        Debug.Log("Hoàn thành " + levels[currentLevelIndex].levelName);

        int nextIndex = currentLevelIndex + 1;
        if (nextIndex < levels.Length)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            Debug.Log("Bạn đã hoàn thành tất cả các level!");
            if (levelText != null)
                levelText.text = "🎉 Hoàn thành tất cả level 🎉";
        }
    }

    // Method để kiểm tra nếu level hiện tại đã hoàn thành
    public bool IsCurrentLevelCompleted()
    {
        if (GoalManager.Instance != null)
        {
            return GoalManager.Instance.IsTargetCompleted();
        }
        return false;
    }
}