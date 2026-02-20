using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Transform entryParent;
    [SerializeField] private GameObject leaderboardPanel;
    private string myId;

    public void OpenLeaderboard()
    {
        myId = PlayerPrefs.GetString("MyPlayFabID", "");
        leaderboardPanel.SetActive(true);
        FetchLeaderboard();
    }

    public void FetchLeaderboard()
    {
        // Clean old entries
        foreach (Transform child in entryParent)
        {
            Destroy(child.gameObject);
        }

        var request = new GetLeaderboardRequest
        {
            StatisticName = "TimeSurvivedHighScore",
            StartPosition = 0,
            MaxResultsCount = 10 // Top 10 players
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardSuccess, OnError);
    }

    private void OnLeaderboardSuccess(GetLeaderboardResult result)
    {
        foreach (var item in result.Leaderboard)
        {
            GameObject entry = Instantiate(entryPrefab, entryParent);
            
            // Get references to text components
            Text rankText = entry.transform.Find("RankText").GetComponent<Text>();
            Text nameText = entry.transform.Find("PlayerNameText").GetComponent<Text>();
            Text scoreText = entry.transform.Find("ScoreText").GetComponent<Text>();

            // PlayFab rank starts at 0, so we add 1
            rankText.text = (item.Position + 1).ToString();
            
            // Use DisplayName if available, otherwise use PlayFabId
            nameText.text = !string.IsNullOrEmpty(item.DisplayName) ? item.DisplayName : "Player " + item.PlayFabId;
            
            scoreText.text = item.StatValue.ToString() + "s";

            if (item.PlayFabId == myId) 
            {
                // Change the colors to make the player stand out
                nameText.color = Color.yellow;
                rankText.color = Color.yellow;
                scoreText.color = Color.yellow;
                
                // Optional: Make the text bold
                nameText.fontStyle = FontStyle.Bold; 
            }
            Canvas.ForceUpdateCanvases();
            entryParent.GetComponent<VerticalLayoutGroup>().enabled = false;
            entryParent.GetComponent<VerticalLayoutGroup>().enabled = true;
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Leaderboard Error: " + error.GenerateErrorReport());
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }
}