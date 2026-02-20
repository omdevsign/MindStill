using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class MenuManager : MonoBehaviour
{
void Start()
{
    if (PlayFabClientAPI.IsClientLoggedIn())
    {
        GetAllCloudScores(); 
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
            else if (stat.StatisticName == "TimeSurvivedHighScore")
            {
                personalHighScore = stat.Value;
            }
        }
    }
}
    private void OnGetStatsError(PlayFabError error)
    {
        Debug.LogError("Error retrieving statistics: " + error.GenerateErrorReport());
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