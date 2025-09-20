using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;

    // Điểm mục tiêu
    public int targetRedScore = 0;
    public int targetBlueScore = 0;
    public int targetPurpleScore = 0;
    public int targetYellowScore = 0;

    // Lưu trữ điểm mục tiêu gốc để reset
    private int originalRedScore = 0;
    private int originalBlueScore = 0;
    private int originalPurpleScore = 0;
    private int originalYellowScore = 0;

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
        // Lưu lại điểm mục tiêu gốc để reset
        originalRedScore = targetRedScore;
        originalBlueScore = targetBlueScore;
        originalPurpleScore = targetPurpleScore;
        originalYellowScore = targetYellowScore;

        Debug.Log($"Original goals saved: R{originalRedScore} B{originalBlueScore} P{originalPurpleScore} Y{originalYellowScore}");

        UpdateUI();
        victoryPanel.SetActive(false);
    }

    public void SubtractTargetScore(int colorID, int amount = 1)
    {
        StartCoroutine(SubtractTargetScoreWithEffects(colorID, amount));
    }

    private IEnumerator SubtractTargetScoreWithEffects(int colorID, int amount)
    {
        // DELAY 1 GIÂY TRƯỚC KHI TRỪ GOAL
        yield return new WaitForSeconds(1f);

        // LẤY TEXT TƯƠNG ỨNG VỚI MÀU
        TextMeshProUGUI targetText = GetTargetTextByColorID(colorID);

        if (targetText != null)
        {
            // HIỆU ỨNG SHAKE/RUNG
            StartCoroutine(ShakeGoalText(targetText));

            // ĐỔI MÀU TEXT TẠM THỜI
            StartCoroutine(FlashTextColor(targetText, GetColorByID(colorID)));

            // PHÁT ÂM THANH
            AudioManager.Instance.PlayVFX("Pop");
        }

        // TRỪ ĐIỂM
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

    private TextMeshProUGUI GetTargetTextByColorID(int colorID)
    {
        if (colorID == redColorID) return redTargetText;
        if (colorID == blueColorID) return blueTargetText;
        if (colorID == purpleColorID) return purpleTargetText;
        if (colorID == yellowColorID) return yellowTargetText;
        return null;
    }


    // HIỆU ỨNG SHAKE/RUNG - GIỮ NGUYÊN NHƯ CŨ
    private IEnumerator ShakeGoalText(TextMeshProUGUI text)
    {
        Vector3 originalPosition = text.transform.localPosition;
        float shakeDuration = 0.3f;
        float shakeMagnitude = 5f;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            text.transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
        text.transform.localPosition = originalPosition;
    }

    // HIỆU ỨNG ĐỔI MÀU TEXT
    private IEnumerator FlashTextColor(TextMeshProUGUI text, Color flashColor)
    {
        Color originalColor = text.color;
        float flashDuration = 0.2f;

        // ĐỔI MÀU
        text.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        // TRỞ LẠI MÀU BAN ĐẦU
        text.color = originalColor;
    }

    private Color GetColorByID(int colorID)
    {
        switch (colorID)
        {
            case 0: return Color.red;
            case 1: return Color.blue;
            case 2: return new Color(0.5f, 0f, 0.5f); // Purple
            case 3: return Color.yellow;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Set goals mới và lưu làm giá trị gốc
    /// </summary>
    public void SetTargets(int red, int blue, int purple, int yellow)
    {
        targetRedScore = red;
        targetBlueScore = blue;
        targetPurpleScore = purple;
        targetYellowScore = yellow;

        // CẬP NHẬT GIÁ TRỊ GỐC ĐỂ RESET
        originalRedScore = red;
        originalBlueScore = blue;
        originalPurpleScore = purple;
        originalYellowScore = yellow;

        UpdateUI();
        Debug.Log($"Goals set to: R{red} B{blue} P{purple} Y{yellow}");
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
        StartCoroutine(ShowVictoryWithDelay(1.2f));
        AudioManager.Instance.PlayVFX("Victory");
        Debug.Log("Hoàn thành level! Nhấn tiếp tục để chơi level tiếp theo.");
    }

    private System.Collections.IEnumerator ShowVictoryWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        victoryPanel.SetActive(true);
    }

    public void ContinueToNextLevel()
    {
        victoryPanel.SetActive(false);
        Time.timeScale = 1f;
        LevelManager.Instance.CompleteLevel();
    }

    private void Victory()
    {
        victoryPanel.SetActive(true);
        Debug.Log("Chiến thắng! Đã hoàn thành tất cả mục tiêu!");
        Time.timeScale = 0f;
    }

    public bool IsTargetCompleted()
    {
        return targetRedScore <= 0 &&
               targetBlueScore <= 0 &&
               targetPurpleScore <= 0 &&
               targetYellowScore <= 0;
    }

    public void SetColorIDs(int redID, int blueID, int purpleID, int yellowID)
    {
        redColorID = redID;
        blueColorID = blueID;
        purpleColorID = purpleID;
        yellowColorID = yellowID;
    }

    public void ResetGoals()
    {
        targetRedScore = originalRedScore;
        targetBlueScore = originalBlueScore;
        targetPurpleScore = originalPurpleScore;
        targetYellowScore = originalYellowScore;

        UpdateUI();
        victoryPanel.SetActive(false);
        Debug.Log($"Goals reset to: R{originalRedScore} B{originalBlueScore} P{originalPurpleScore} Y{originalYellowScore}");
    }
}