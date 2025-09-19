using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;

    // Điểm mục tiêu
    public int targetRedScore = 0;
    public int targetBlueScore = 0;
    public int targetPurpleScore = 0;
    public int targetYellowScore = 0;

    [SerializeField] private TextMeshProUGUI redTargetText;
    [SerializeField] private TextMeshProUGUI blueTargetText;
    [SerializeField] private TextMeshProUGUI purpleTargetText;
    [SerializeField] private TextMeshProUGUI yellowTargetText;

    [SerializeField] private GameObject victoryPanel;

    // CONFIG: Điều chỉnh các ID này cho đúng với prefab của bạn
    [Header("Color ID Configuration")]
    public int redColorID = 0;
    public int blueColorID = 1;
    public int purpleColorID = 2;
    public int yellowColorID = 3;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        UpdateUI();
        victoryPanel.SetActive(false);
    }

    public void SubtractTargetScore(int colorID, int amount = 1)
    {
        // So sánh colorID với config
        if (colorID == redColorID)
        {
            targetRedScore = Mathf.Max(0, targetRedScore - amount);
        }
        else if (colorID == blueColorID)
        {
            targetBlueScore = Mathf.Max(0, targetBlueScore - amount);
        }
        else if (colorID == purpleColorID)
        {
            targetPurpleScore = Mathf.Max(0, targetPurpleScore - amount);
        }
        else if (colorID == yellowColorID)
        {
            targetYellowScore = Mathf.Max(0, targetYellowScore - amount);
        }
        UpdateUI();
        CheckLevelCompletion();
    }

    private void UpdateUI()
    {
        if (redTargetText != null) redTargetText.text = targetRedScore.ToString();
        if (blueTargetText != null) blueTargetText.text = targetBlueScore.ToString();
        if (purpleTargetText != null) purpleTargetText.text = targetPurpleScore.ToString();
        if (yellowTargetText != null) yellowTargetText.text = targetYellowScore.ToString();
    }

    private void CheckLevelCompletion()
    {
        if (IsTargetCompleted())
        {
            if (LevelManager.Instance != null)
            {
                // 0 pause game ngay lập tức
                ShowLevelCompletePopup();
            }
            else
            {
                Victory(); 
            }
        }
    }

    private void ShowLevelCompletePopup()
    {               
        StartCoroutine(ShowVictoryWithDelay(1.5f));
        AudioManager.Instance.PlayVFX("Victory");
        Debug.Log("Hoàn thành level! Nhấn tiếp tục để chơi level tiếp theo.");
    }

    // Hiện victory chậm hơn
    private System.Collections.IEnumerator ShowVictoryWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        victoryPanel.SetActive(true);
    }

    // Thêm method để tiếp tục game
    public void ContinueToNextLevel()
    {
        victoryPanel.SetActive(false);
        Time.timeScale = 1f; // Đảm bảo game không bị pause
        LevelManager.Instance.CompleteLevel();
    }

    private void Victory()
    {
        victoryPanel.SetActive(true);
        Debug.Log("Chiến thắng! Đã hoàn thành tất cả mục tiêu!");
        // Có thể pause game ở đây vì đã hoàn thành toàn bộ game
        Time.timeScale = 0f;
    }
    public bool IsTargetCompleted()
    {
        return targetRedScore <= 0 &&
               targetBlueScore <= 0 &&
               targetPurpleScore <= 0 &&
               targetYellowScore <= 0;
    }
    public void SetTargets(int red, int blue, int purple, int yellow)
    {
        targetRedScore = red;
        targetBlueScore = blue;
        targetPurpleScore = purple;
        targetYellowScore = yellow;
        UpdateUI();
    }
    public void SetColorIDs(int redID, int blueID, int purpleID, int yellowID)
    {
        redColorID = redID;
        blueColorID = blueID;
        purpleColorID = purpleID;
        yellowColorID = yellowID;
    }
}