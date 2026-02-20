using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Text highScoreText; 
    [SerializeField] private Text totalPlaytimeText;
void Start()
{
    if (PlayFabClientAPI.IsClientLoggedIn())
    {
        GetAllCloudScores(); 
    }
    else
    {
        highScoreText.text = "High Score: Connecting...";
        totalPlaytimeText.text = "Total Playtime: Connecting..."; 
    }
}
public void GetAllCloudScores() 
{
    var dataRequest = new GetPlayerStatisticsRequest();
    PlayFabClientAPI.GetPlayerStatistics(dataRequest, OnGetStatsSuccess, OnGetStatsError);
}
private void OnGetStatsSuccess(GetPlayerStatisticsResult result)
{
    if (result.Statistics != null)
    {
        int totalTime = 0;
        int personalHighScore = 0;

        foreach (var stat in result.Statistics)
        {
            if (stat.StatisticName == "TotalPlaytime")
            {
                totalTime = stat.Value;
            }
            // CHANGE: Fetch your personal high score here instead of the Leaderboard
            else if (stat.StatisticName == "TimeSurvivedHighScore")
            {
                personalHighScore = stat.Value;
            }
        }

        totalPlaytimeText.text = "Total Playtime: " + totalTime.ToString() + " seconds";
        highScoreText.text = "High Score: " + personalHighScore.ToString() + " seconds";
    }
}
/*private void GetLeaderboardHighScore()
{
    var leaderboardRequest = new GetLeaderboardRequest
    {
        StatisticName = "TimeSurvivedHighScore",
        StartPosition = 0,
        MaxResultsCount = 1 
    };

    PlayFabClientAPI.GetLeaderboard(leaderboardRequest, OnGetLeaderboardSuccess, OnGetStatsError);
}*/
/*private void OnGetStatsSuccess(GetPlayerStatisticsResult result)
{
    if (result.Statistics != null)
    {
        int totalTime = 0;
        foreach (var stat in result.Statistics)
        {
            if (stat.StatisticName == "TotalPlaytime")
            {
                totalTime = stat.Value;
                break;
            }
        }
        totalPlaytimeText.text = "Total Playtime: " + totalTime.ToString() + " seconds";
    }
}*/

/*private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
{
    if (result.Leaderboard.Count > 0)
    {
        int score = result.Leaderboard[0].StatValue;
        highScoreText.text = "High Score: " + score.ToString() + " seconds";
    }
    else
    {
        highScoreText.text = "High Score: 0 seconds (Cloud)";
    }
}*/

private void OnGetStatsError(PlayFabError error)
{
    Debug.LogError("Error retrieving statistics: " + error.GenerateErrorReport());
    highScoreText.text = "High Score: Error Loading";
    totalPlaytimeText.text = "Total Playtime: Error Loading";
}
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); 
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}