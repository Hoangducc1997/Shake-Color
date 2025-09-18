using UnityEngine;
using UnityEngine.UI;

public class GoalManager : MonoBehaviour
{
    //Điểm kiếm được
    private int currentRedScore = 0;
    private int currentBlueScore = 0;
    private int currentPurpleScore = 0;
    private int currentYellowScore = 0;

    //Điểm mục tiêu
    public int targetRedScore = 10;
    public int targetBlueScore = 10;
    public int targetPurpleScore = 10;
    public int targetYellowScore = 10;

    [SerializeField] private Text redTargetScore;
    [SerializeField] private Text blueTargetScore;
    [SerializeField] private Text purpTargetleScore;
    [SerializeField] private Text yellowTargetScore;


}
