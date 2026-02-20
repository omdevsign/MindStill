using UnityEngine;
using System.Collections.Generic;
using PlayFab;           
using PlayFab.Json;      
using UnityEngine.UI;

public class AchievementsUIManager : MonoBehaviour
{
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private Transform contentParent;

    public void OpenPanel()
    {
        Debug.Log("UI: OpenPanel triggered by button."); 
        gameObject.SetActive(true);
        
        if (contentParent == null) {
            Debug.LogError("UI Error: Content Parent is not assigned in the Inspector!");
            return;
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("UI: Requesting achievement data from PlayFabAuth...");
        PlayFabAuth.GetAchievementStatus();
    }

    public void DisplayAchievements(object resultData)
    {
        Debug.Log("5. UI Manager started processing data.");
        
        string json = PlayFab.Json.PlayFabSimpleJson.SerializeObject(resultData);
        var achievementData = PlayFab.Json.PlayFabSimpleJson.DeserializeObject<PlayFab.Json.JsonObject>(json);
        
        if (achievementData == null || !achievementData.ContainsKey("Achievements")) 
        {
            Debug.LogError("6. Error: JSON does not contain 'Achievements' key!");
            return;
        }
        
        PlayFab.Json.JsonArray achievements = achievementData["Achievements"] as PlayFab.Json.JsonArray;
        Debug.Log("7. Number of achievements found in data: " + achievements.Count);

        foreach (var achievementObj in achievements)
        {
            PlayFab.Json.JsonObject ach = achievementObj as PlayFab.Json.JsonObject;
            if (ach == null) continue;

            GameObject item = Instantiate(achievementItemPrefab, contentParent);
            Text nameText = item.transform.Find("NameText").GetComponent<Text>();
            Text descText = item.transform.Find("DescText").GetComponent<Text>();
            
            
            Text statusText = item.transform.Find("StatusText").GetComponent<Text>();

            string rawNameData = ach["Name"].ToString();
            string status = ach["Status"].ToString();
            Debug.Log($"Checking {ach["Id"]}: Server says status is {status}");

            try 
            {
                var metadata = PlayFab.Json.PlayFabSimpleJson.DeserializeObject<PlayFab.Json.JsonObject>(rawNameData);
                nameText.text = metadata["Name"].ToString();
                descText.text = metadata["Description"].ToString();
            }
            catch 
            {
                nameText.text = rawNameData;
                descText.text = "";
            }

            if (status == "Granted")
            {
                statusText.text = "Earned";
                statusText.color = Color.green;
            }
            else
            {
                statusText.text = "Locked";
                statusText.color = Color.gray;
            }
        }
    }
    public void ClosePanel()
    {
        gameObject.SetActive(false);
        Debug.Log("UI: Achievement panel closed.");
    }
}