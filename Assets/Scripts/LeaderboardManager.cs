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
        // GET THE ID FROM THE SOURCE OF TRUTH
        string currentUserId = PlayFabAuth.GetMyId(); 
        Debug.Log("Highlighting check for ID: " + currentUserId);

        foreach (var item in result.Leaderboard)
        {
            GameObject entry = Instantiate(entryPrefab, entryParent);
            
            Text rankText = entry.transform.Find("RankText").GetComponent<Text>();
            Text nameText = entry.transform.Find("PlayerNameText").GetComponent<Text>();
            Text scoreText = entry.transform.Find("ScoreText").GetComponent<Text>();

            rankText.text = (item.Position + 1).ToString();
            nameText.text = !string.IsNullOrEmpty(item.DisplayName) ? item.DisplayName : "Player " + item.PlayFabId;
            scoreText.text = item.StatValue.ToString() + "s";

            // COMPARE
            if (item.PlayFabId == currentUserId) 
            {
                // 1. Text Highlight
                Color highlightColor = new Color(1f, 0.92f, 0.016f, 1f); // Gold/Yellow
                nameText.color = highlightColor;
                rankText.color = highlightColor;
                scoreText.color = highlightColor;
                nameText.fontStyle = FontStyle.Bold;
                
                Debug.Log("Highlighted current user: " + item.DisplayName);
            }
        }
        
        // Refresh UI layout once after the loop, not inside it (Better for performance)
        LayoutRebuilder.ForceRebuildLayoutImmediate(entryParent.GetComponent<RectTransform>());
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