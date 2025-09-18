using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] Button _startBtn;
    [SerializeField] Button _exitBtn;
    [SerializeField] Button _settingBtn;
    [SerializeField] GameObject _settingMenu;
    void Start()
    {
        _startBtn.onClick.AddListener(StartGame);
        _exitBtn.onClick.AddListener(ExitGame);
        _settingBtn.onClick.AddListener(OpenSetting);
    }

    public void StartGame()
    {
        AudioManager.Instance.PlayVFX("Choose");
        SceneManager.LoadScene("GamePlay");
    }

    public void ExitGame()
    {
        AudioManager.Instance.PlayVFX("Choose");
        Application.Quit();
        Debug.Log("Exit Game");
    }

    public void OpenSetting()
    {
        AudioManager.Instance.PlayVFX("Choose");
        // Open setting menu
        _settingMenu.SetActive(true);
    }
}
