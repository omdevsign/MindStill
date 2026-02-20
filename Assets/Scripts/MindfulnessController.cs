using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System.Data.Common;

public class MindfulnessController : MonoBehaviour
{
    [Header("Skybox Dynamics")] 
    [SerializeField] private Material blueSky;   
    [SerializeField] private Material purpleSky; 
    [SerializeField] private Material redSky;    

    [Header("Lighting Dynamics")]
    [SerializeField] private Light mainDirectionalLight; 
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.2f;

    [Header("UI References")]
    [SerializeField] private Slider calmnessSlider; 
    [SerializeField] private RectTransform breathingGuide; 

    [Header("Mechanics")]
    [SerializeField] private float breathDuration = 4f; 
    [SerializeField] private float calmnessChangeRate = 5f; 
    [SerializeField] private float anxietyRate = 1f; 
    
    [Header("Affirmation Settings")]
    [SerializeField] private string[] affirmations = new string[] 
    {
        "Clarity is your reward.",
        "You are resilient and strong.",
        "Breathe in peace, breathe out anxiety.",
        "This path is yours to shape.",
        "Stillness brings focus."
    };
    [SerializeField] private Text affirmationText; 

    [Header("Breathing Dynamics")]
    [SerializeField] private float minFocusDuration = 3.0f; 
    [SerializeField] private float maxFocusDuration = 6.0f; 
    
    [SerializeField] private float currentAnxietyMultiplier = 1.0f; 
    private float timer = 0f;
    private bool isExhaling = false; 
    
    [Header("Achievement Tracking")]
    private float totalSessionTime = 0f;
    private float highCalmnessTimer = 0f;
    private bool firstFocusAchieved = false;
    private bool marathonAchieved = false;
    private bool mindfulMasterAchieved = false;
    

    void Start()
    {
        SyncAchievementStatus();
        calmnessSlider.value = 50f; 
        CalculateNewFocusDuration();
    }

    void Update()
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (Time.timeScale == 0f) return;
        ManageBreathingRhythm();
        HandlePlayerInput();
        UpdateFogAndAnxiety();

        if (gm.isGameOver || gm.countdownText.gameObject.activeSelf) return;
        
        totalSessionTime += Time.deltaTime;
        if (totalSessionTime >= 300f && !marathonAchieved)
        {
            marathonAchieved = true;
            SendAchievement("MarathonerEvent", "ACH_MARATHON");
            Debug.Log("Achievement Sent: Resilience Runner");
        }

        
        if (calmnessSlider.value >= 70f)
        {
            highCalmnessTimer += Time.deltaTime;
            if (highCalmnessTimer >= 60f && !mindfulMasterAchieved)
            {
                mindfulMasterAchieved = true;
                SendAchievement("MindfulMasterEvent", "ACH_MINDFUL");
                Debug.Log("Achievement Sent: Master of Stillness");
            }
        }
        else
        {
            highCalmnessTimer = 0f; 
        }
    }

    private void ManageBreathingRhythm()
    {
        timer += Time.deltaTime;
        if (timer >= breathDuration)
        {
            timer = 0f;
            if (!firstFocusAchieved)
            {
                firstFocusAchieved = true; 
                Debug.Log("Breath Cycle Complete! Sending Achievement Signal...");
                SendAchievement("FirstFocusEvent", "ACH_FIRST_FOCUS");
            }
        }
        isExhaling = timer > (breathDuration / 2f); 
        float normalizedTime = timer / breathDuration;
        float scaleMultiplier = 1f;

        if (isExhaling)
        {
            scaleMultiplier = Mathf.Lerp(1.5f, 0.5f, (normalizedTime - 0.5f) * 2f);
        }
        else
        {
            scaleMultiplier = Mathf.Lerp(0.5f, 1.5f, normalizedTime * 2f);
        }

        breathingGuide.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1f);
    }

    private void HandlePlayerInput()
    {
        if (Input.GetKey(KeyCode.Space)) 
        {
            if (isExhaling)
            {
                calmnessSlider.value += calmnessChangeRate * Time.deltaTime;
                CalculateNewFocusDuration();                
                if (calmnessSlider.value >= 75f && calmnessSlider.value - calmnessChangeRate * Time.deltaTime < 75f) 
                {
                    DisplayAffirmation();
                }
            }
            else
            {
                calmnessSlider.value -= calmnessChangeRate * Time.deltaTime;
            }
        }
        else 
        {   
            calmnessSlider.value -= (anxietyRate * currentAnxietyMultiplier) * Time.deltaTime;
        }
        calmnessSlider.value = Mathf.Clamp(calmnessSlider.value, 0f, 100f);
    }

    private void UpdateFogAndAnxiety()
    {
        float normalizedCalmness = calmnessSlider.value / 100f;
        float maxFogDensity = 0.04f; 
        float minFogDensity = 0.005f;
        RenderSettings.fog = true;
        float fogDensity = Mathf.Lerp(minFogDensity, maxFogDensity, normalizedCalmness);
        RenderSettings.fogDensity = fogDensity;

        Color targetColor;

        Color redAnxious = new Color(0.7f, 0.01f, 0.01f, 1f); 
        Color purpleNeutral = new Color(0.6f, 0.4f, 0.8f, 1f); 
        Color whiteCalm = new Color(0.85f, 0.83f, 0.8f, 1f);
        if (normalizedCalmness < 0.5f)
        {
            float t = normalizedCalmness * 2f; 
            targetColor = Color.Lerp(redAnxious, purpleNeutral, t);
        }
        else
        {
            float t = (normalizedCalmness - 0.5f) * 2f; 
            targetColor = Color.Lerp(purpleNeutral, whiteCalm, t);
        }

        RenderSettings.fogColor = targetColor;
        
        if (calmnessSlider.value > 75f)
        {
            if (RenderSettings.skybox != blueSky) RenderSettings.skybox = blueSky;
        }
        else if (calmnessSlider.value > 25f)
        {
            if (RenderSettings.skybox != purpleSky) RenderSettings.skybox = purpleSky;
        }
        else
        {
            if (RenderSettings.skybox != redSky) RenderSettings.skybox = redSky;
        }
        DynamicGI.UpdateEnvironment();
        
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedCalmness);
        }
        
        if (calmnessSlider.value <= 0)
        {
            Debug.Log("Game Over! Anxiety overcame you.");
        }
    }
    public float GetCalmness()
    {
        return calmnessSlider.value;
    }
    public void DisplayAffirmation()
    {
        if (affirmationText == null || affirmations.Length == 0) return;
        string message = affirmations[Random.Range(0, affirmations.Length)];
        affirmationText.text = message;
        Invoke("ClearAffirmation", 3f);
    }

    private void ClearAffirmation()
    {
        if (affirmationText != null)
        {
            affirmationText.text = "";
        }
    }
    
    public void ApplyAnxietyPenalty(float penaltyAmount)
    {
        calmnessSlider.value -= penaltyAmount;
        calmnessSlider.value = Mathf.Clamp(calmnessSlider.value, 0f, 100f);

        CalculateNewFocusDuration();
    }
    public void SetPassiveAnxietyMultiplier(float multiplier)
    {
        currentAnxietyMultiplier = multiplier;
    }
    private void CalculateNewFocusDuration()
    {
        float normalizedCalmness = calmnessSlider.value / 100f;
        float t = 1f - normalizedCalmness; 
        float newDuration = Mathf.Lerp(minFocusDuration, maxFocusDuration, t);
        breathDuration = newDuration;
        Debug.Log($"Calmness: {calmnessSlider.value:F0}, New Focus Duration Required: {breathDuration:F2}s");
    }

    private void TriggerGameOver()
    {
        Debug.Log("GAME OVER: All focus lost.");
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.GameOver();
        }
    }
    private void SendAchievement(string eventName, string achievementId)
    {
        PlayFabAuth.SubmitPlayFabEvent(eventName);
    }
    private bool AchievementIsAlreadyEarned(string id)
    {
        if (id == "ACH_FIRST_FOCUS" && firstFocusAchieved) return true;
        if (id == "ACH_MARATHON" && marathonAchieved) return true;
        if (id == "ACH_MINDFUL" && mindfulMasterAchieved) return true;
        return false;
    }
    private void SyncAchievementStatus()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result => {
            if (result.Data.ContainsKey("ACH_FIRST_FOCUS")) firstFocusAchieved = true;
            if (result.Data.ContainsKey("ACH_MARATHON")) marathonAchieved = true;
            if (result.Data.ContainsKey("ACH_MINDFUL")) mindfulMasterAchieved = true;
            Debug.Log("I checked with the server! I know which achievements you already have.");
        }, error => {
            Debug.LogError("Oh no! I couldn't talk to the server to check achievements.");
        });
    }
}