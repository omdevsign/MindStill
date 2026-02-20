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
    [SerializeField] private Text totalAchievementsText;
    [SerializeField] private Text rankNameText;
    [SerializeField] private Text nextLevelText;

    public void OpenProfile()
    {
        profilePanel.SetActive(true);
        LoadProfileData();
    }

    private void LoadProfileData()
    {
        // 1. Get Account Info (Username/ID)
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
        // Set ID and Name
        playFabIdText.text = "PlayFab ID: " + result.PlayFabId;
        
        string name = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        usernameText.text = "Username: " + (string.IsNullOrEmpty(name) ? "Anonymous Soul" : name);

        // Set Stats
        int highScore = 0;
        int playtime = 0;

        foreach (var stat in result.InfoResultPayload.PlayerStatistics)
        {
            if (stat.StatisticName == "TimeSurvivedHighScore") highScore = stat.Value;
            if (stat.StatisticName == "TotalPlaytime") playtime = stat.Value;
        }

        highScoreText.text = "Personal Best: " + highScore + "s";
        totalPlaytimeText.text = "Total Playtime: " + FormatTime(playtime);
        UpdateLevelDisplay(playtime);
    }

    private string FormatTime(int seconds)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}H:{1:D2}M:{2:D2}S", t.Hours, t.Minutes, t.Seconds);
    }
    public void ProcessAchievementCount(object resultData)
    {
        // Serialize and Deserialize the cloud script result just like in your AchievementsUIManager
        string json = PlayFab.Json.PlayFabSimpleJson.SerializeObject(resultData);
        var achievementData = PlayFab.Json.PlayFabSimpleJson.DeserializeObject<PlayFab.Json.JsonObject>(json);
        
        if (achievementData == null || !achievementData.ContainsKey("Achievements")) return;
        
        PlayFab.Json.JsonArray achievements = achievementData["Achievements"] as PlayFab.Json.JsonArray;
        
        int earnedCount = 0;
        int totalCount = achievements.Count;

        foreach (var achievementObj in achievements)
        {
            PlayFab.Json.JsonObject ach = achievementObj as PlayFab.Json.JsonObject;
            if (ach != null && ach["Status"].ToString() == "Granted")
            {
                earnedCount++;
            }
        }

        totalAchievementsText.text = "Achievements: " + earnedCount + " / " + totalCount;
    }
    private void UpdateLevelDisplay(int totalSeconds)
    {
        string rank;
        string next;
        
        // Get reference to MindfulnessController to check/set achievements
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
            
            // TRIGGER RANK 1
            if (mc != null && !mc.AchievementIsAlreadyEarned("ACH_INITIATE")) {
                mc.SetLocalAchievementTrue("ACH_INITIATE");
                PlayFabAuth.SubmitPlayFabEvent("RankInitiateEvent");
            }
        }
        else if (totalSeconds < 15000)
        {
            rank = "Resilient Soul";
            next = (15000 - totalSeconds) + "s until Zen Master";

            // TRIGGER RANK 2
            if (mc != null && !mc.AchievementIsAlreadyEarned("ACH_RESILIENT")) {
                mc.SetLocalAchievementTrue("ACH_RESILIENT");
                PlayFabAuth.SubmitPlayFabEvent("RankResilientEvent");
            }
        }
        else
        {
            rank = "Zen Master";
            next = "Maximum Rank Achieved";

            // TRIGGER RANK 3
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
        // 1. Tell PlayFab to forget this session
        PlayFabClientAPI.ForgetAllCredentials();

        // 2. Delete the local auto-login keys
        if (PlayerPrefs.HasKey("MyCustomDeviceID"))
        {
            PlayerPrefs.DeleteKey("MyCustomDeviceID");
        }
        
        // Also delete the PlayFabID we were using for UI
        if (PlayerPrefs.HasKey("MyPlayFabID"))
        {
            PlayerPrefs.DeleteKey("MyPlayFabID");
        }

        PlayerPrefs.Save(); // Force the deletion to disk

        Debug.Log("Logged out and auto-login ID cleared.");

        // 3. Back to the login screen
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