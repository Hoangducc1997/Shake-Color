using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SceneGamePlayManager : MonoBehaviour
{
    [SerializeField] Button pvpBtn;
    [SerializeField] Button pveBtn;
    [SerializeField] Button eveBtn;
    [SerializeField] GameObject panelChooseGamePlay;
    [SerializeField] GameObject panelVictory;
    [SerializeField] GameObject panelGameModePvE;

    public static SceneGamePlayManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pvpBtn.onClick.AddListener(HidePanelChooseGamePlay);
        pveBtn.onClick.AddListener(ShowpanelGameModePvE);
        eveBtn.onClick.AddListener(HidePanelChooseGamePlay);
        panelChooseGamePlay.SetActive(true);
        panelVictory.SetActive(false);
        panelGameModePvE.SetActive(false);
    }

    public void HidePanelChooseGamePlay()
    {
        panelChooseGamePlay.SetActive(false);
    }

    public void ShowpanelGameModePvE()
    {
        panelGameModePvE.SetActive(true);
    }

    //Victory Panel
    public void UnHidePanelVictory()
    {
        panelVictory.SetActive(true);
    }

    public void ShowPanelVictory()
    {
        panelVictory.SetActive(true);
    }

    //Exit Game
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
