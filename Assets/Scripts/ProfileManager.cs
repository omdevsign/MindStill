using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private Text usernameText;
    [SerializeField] private Text playFabIdText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text totalPlaytimeText;
    [SerializeField] private Text challengesFacedText;
    [SerializeField] private Text rankNameText;
    [SerializeField] private Text nextLevelText;

    public void OpenProfile()
    {
        profilePanel.SetActive(true);
        LoadProfileData();
        GetPlayerProfileData();
    }

    private void LoadProfileData()
    {
        var accountRequest = new GetPlayerCombinedInfoRequestParams
        {
            GetUserAccountInfo = true,
            GetPlayerStatistics = true
        };
        PlayFabClientAPI.GetPlayerCombinedInfo(new GetPlayerCombinedInfoRequest
        {
            InfoRequestParameters = accountRequest
        }, OnDataReceived, OnError);
    }

    private void OnDataReceived(GetPlayerCombinedInfoResult result)
    {
        playFabIdText.text = "PlayFab ID: " + result.PlayFabId;
        string name = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        usernameText.text = "Username: " + (string.IsNullOrEmpty(name) ? "Anonymous Soul" : name);
        int highScore = 0;
        int playtime = 0;
        int challenges = 0; 
        foreach (var stat in result.InfoResultPayload.PlayerStatistics)
        {
            if (stat.StatisticName == "TimeSurvivedHighScore") highScore = stat.Value;
            if (stat.StatisticName == "TotalPlaytime") playtime = stat.Value;
            
            
            if (stat.StatisticName == "TotalChallengesFaced") challenges = stat.Value;
        }
        highScoreText.text = "Personal Best: " + highScore + "s";
        totalPlaytimeText.text = "Total Playtime: " + FormatTime(playtime);
        challengesFacedText.text = "Challenges Faced: " + challenges;
        UpdateLevelDisplay(playtime);
    }

    private string FormatTime(int seconds)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}H:{1:D2}M:{2:D2}S", t.Hours, t.Minutes, t.Seconds);
    }
    public void GetPlayerProfileData()
    {
        var request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, result => {
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == "TotalChallengesFaced")
                {
                    challengesFacedText.text = "Challenges Faced: " + stat.Value;
                }
            }
        }, error => {
            Debug.LogError("Could not fetch profile statistics.");
        });
    }
    private void UpdateLevelDisplay(int totalSeconds)
    {
        string rank;
        string next;
        MindfulnessController mc = Object.FindFirstObjectByType<MindfulnessController>();
        if (totalSeconds < 1000)
        {
            rank = "Beginner";
            next = (1000 - totalSeconds) + "s until Calm Initiate";
        }
        else if (totalSeconds < 5000)
        {
            rank = "Calm Initiate";
            next = (5000 - totalSeconds) + "s until Resilient Soul";
            if (mc != null && !mc.AchievementIsAlreadyEarned("ACH_INITIATE")) {
                mc.SetLocalAchievementTrue("ACH_INITIATE");
                PlayFabAuth.SubmitPlayFabEvent("RankInitiateEvent");
            }
        }
        else if (totalSeconds < 15000)
        {
            rank = "Resilient Soul";
            next = (15000 - totalSeconds) + "s until Zen Master";
            if (mc != null && !mc.AchievementIsAlreadyEarned("ACH_RESILIENT")) {
                mc.SetLocalAchievementTrue("ACH_RESILIENT");
                PlayFabAuth.SubmitPlayFabEvent("RankResilientEvent");
            }
        }
        else
        {
            rank = "Zen Master";
            next = "Maximum Rank Achieved";
            if (mc != null && !mc.AchievementIsAlreadyEarned("ACH_ZEN")) {
                mc.SetLocalAchievementTrue("ACH_ZEN");
                PlayFabAuth.SubmitPlayFabEvent("RankZenEvent");
            }
        }
        rankNameText.text = "Rank: " + rank;
        nextLevelText.text = next;
    }
    public void Logout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        if (PlayerPrefs.HasKey("MyCustomDeviceID"))
        {
            PlayerPrefs.DeleteKey("MyCustomDeviceID");
        }
        if (PlayerPrefs.HasKey("MyPlayFabID"))
        {
            PlayerPrefs.DeleteKey("MyPlayFabID");
        }
        PlayerPrefs.Save(); 
        Debug.Log("Logged out and auto-login ID cleared.");
        SceneManager.LoadScene("LoginScene");
    }
    public void CloseProfile()
    {
        profilePanel.SetActive(false);
    }
    private void OnError(PlayFabError error)
    {
        Debug.LogError("Profile Error: " + error.GenerateErrorReport());
    }
}