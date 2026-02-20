using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabAuth : MonoBehaviour
{
    private static string PlayFabId;
    void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            Debug.LogError("PlayFab Title ID is not set. Go to Window > PlayFab > Editor Extensions to set it.");
            return;
        }   
        //LoginAnonymously();
    }
    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }
    // Add this to PlayFabAuth.cs so other scripts can "hand off" the ID
    public static void SetPlayerId(string id)
    {
        PlayFabId = id;
        Debug.Log("PlayFabId successfully cached: " + PlayFabId);
    }
    public static string GetMyId()
    {
        return PlayFabId;
    }
    private void LoginAnonymously()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier, 
            CreateAccount = true 
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        PlayFabId = result.PlayFabId;
        Debug.Log("Successfully logged into PlayFab ID: " + PlayFabId);
        MenuManager menuManager = FindFirstObjectByType<MenuManager>();
        if (menuManager != null)
        {
            menuManager.GetAllCloudScores();
        }
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFab Login Failed: " + error.GenerateErrorReport());
    }
    public static void SubmitPlayFabEvent(string eventName, Dictionary<string, object> eventData = null)
    {
        if (string.IsNullOrEmpty(PlayFabId)) 
        {
            Debug.LogError("PlayFab ID not set. Cannot submit event. Log in first.");
            return;
        }

        var request = new WriteClientPlayerEventRequest
        {
            EventName = eventName, 
            Body = eventData ?? new Dictionary<string, object>(),
        };

        PlayFabClientAPI.WritePlayerEvent(request, 
            (WriteEventResponse result) => OnEventWriteSuccess(eventName),
            (PlayFabError error) => OnEventWriteFailure(error)
        );
    }
    private static void OnEventWriteSuccess(string eventName) 
    {
        Debug.Log($"PlayFab Event '{eventName}' submitted successfully."); 
        
        string friendlyName = "New Milestone!";

        // Original Set
        if (eventName == "FirstFocusEvent") friendlyName = "First Step";
        if (eventName == "MarathonerEvent") friendlyName = "Resilience Runner";
        if (eventName == "MindfulMasterEvent") friendlyName = "Master of Stillness";

        // New Mechanics Set
        if (eventName == "CenturyEvent") friendlyName = "Centurion";
        if (eventName == "BreathMasterEvent") friendlyName = "Rhythm Keeper";
        if (eventName == "ClutchCalmEvent") friendlyName = "Cool Under Pressure";

        // Progress & Rank Set
        if (eventName == "ChallengerEvent") friendlyName = "The Challenger";
        if (eventName == "HandleRankInitiate") friendlyName = "Calm Initiate";
        if (eventName == "RankResilientEvent") friendlyName = "Resilient Soul";
        if (eventName == "RankZenEvent") friendlyName = "Zen Master";

        if (AchievementNotification.Instance != null)
        {
            AchievementNotification.Instance.ShowNotification(friendlyName);
        }
    }
    private static void OnEventWriteFailure(PlayFabError error)
    {
        Debug.LogError($"PlayFab Event submission failed: {error.GenerateErrorReport()}");
    }
    
    public static void GetAchievementStatus()
    {
        
        if (string.IsNullOrEmpty(PlayFabId))
        {
            Debug.LogError("PlayFab ID not set. Cannot fetch achievements.");
            return;
        }

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetAchievementProgress", 
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, 
            (ExecuteCloudScriptResult result) => OnCloudScriptSuccess(result), 
            (PlayFabError error) => OnCloudScriptFailure(error)
        );
    }

    
    private static void OnCloudScriptSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("1. Cloud Script call successful.");
        
        if (result.FunctionResult == null)
        {
            Debug.LogError("2. Error: FunctionResult is NULL. Check if your Cloud Script is published.");
            return;
        }
        Debug.Log("3. Data received: " + result.FunctionResult.ToString());

        AchievementsUIManager uiManager = Object.FindFirstObjectByType<AchievementsUIManager>();
        if (uiManager != null)
        {
            Debug.Log("4. Found UIManager, passing data...");
            uiManager.DisplayAchievements(result.FunctionResult);
        }
    }

    private static void OnCloudScriptFailure(PlayFabError error)
    {
        Debug.LogError("Failed to fetch achievement status: " + error.GenerateErrorReport());
    }
}