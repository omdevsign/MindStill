using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

public class ResilienceManager : MonoBehaviour
{
    public enum ChallengeType { InnerCritic, TensionRelease, Paradox, BoxBreathing, Grounding }
    public enum TriggerType { ObstacleHit, InvisibleAnxiety, Milestone }

    [Header("Main References")]
    [SerializeField] private GameObject resiliencePanel; 
    [SerializeField] private Text titleText; 
    [SerializeField] private Text instructionText; 
    [SerializeField] private Image overlayImage; 
    
    [Header("Reward/Penalty")]
    [SerializeField] private float successBonus = 20f; 
    [SerializeField] private float failurePenalty = 30f; 

    [Header("Challenge 1: Inner Critic (Tap)")]
    [SerializeField] private GameObject innerCriticUI;
    [SerializeField] private Text tapCountText; 
    [SerializeField] private int requiredTaps = 5;

    [Header("Challenge 2: Tension Release (Hold)")]
    [SerializeField] private GameObject tensionUI;
    [SerializeField] private Slider tensionSlider;
    [SerializeField] private Text tensionInstructionText; 

    [Header("Challenge 3: Paradox (Do Nothing)")]
    [SerializeField] private GameObject paradoxUI;
    [SerializeField] private Text paradoxTimerText;

    [Header("Challenge 4: Box Breathing (Timed)")]
    [SerializeField] private GameObject breathingUI;
    [SerializeField] private Text breatheStatusText; 
    [SerializeField] private Slider breathSlider;
    private float transitionBuffer = 0f; 

    [Header("Challenge 5: Grounding (Key Sequence)")]
    [SerializeField] private GameObject groundingUI;
    [SerializeField] private Text sequenceText; 

    private bool challengeActive = false;
    private ChallengeType currentChallenge;
    private MindfulnessController mc;
    private GameManager gm;
    private float originalFixedDeltaTime;
    private int currentTaps = 0;
    private float holdTimer = 0f;
    private float paradoxTimer = 0f;
    private int breathingStage = 0; 
    private KeyCode[] groundingSequence;
    private int groundingIndex = 0;
    public bool clutchCalmAchieved = false;
    private int localChallengesFaced = 0;

    void Start()
    {
        mc = GetComponent<MindfulnessController>();
        gm = GetComponent<GameManager>();
        resiliencePanel.SetActive(false); 
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {
        if (gm.isGameOver || gm.countdownText.gameObject.activeSelf) return;
        if (gm != null && gm.sessionScore > 0 && gm.IsGameFlowBlocked() && !gm.pausePanelActive()) 
        {
            if (challengeActive) 
            {
                challengeActive = false;
                resiliencePanel.SetActive(false);
            }
            return; 
        }
        if (gm != null && gm.IsGameFlowBlocked())
        {
            return; 
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) TriggerChallenge(TriggerType.Milestone, ChallengeType.InnerCritic);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TriggerChallenge(TriggerType.Milestone, ChallengeType.TensionRelease);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TriggerChallenge(TriggerType.Milestone, ChallengeType.Paradox);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TriggerChallenge(TriggerType.Milestone, ChallengeType.BoxBreathing);
        if (Input.GetKeyDown(KeyCode.Alpha5)) TriggerChallenge(TriggerType.Milestone, ChallengeType.Grounding);

        if (!challengeActive) return;
        switch (currentChallenge)
        {
            case ChallengeType.InnerCritic: UpdateInnerCritic(); break;
            case ChallengeType.TensionRelease: UpdateTensionRelease(); break;
            case ChallengeType.Paradox: UpdateParadox(); break;
            case ChallengeType.BoxBreathing: UpdateBoxBreathing(); break;
            case ChallengeType.Grounding: UpdateGrounding(); break;
        }
    }
    
    public void TriggerChallenge(TriggerType trigger, ChallengeType specificChallenge)
    {
        if (challengeActive) return;
        Time.timeScale = 0.05f; 
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale; 
        challengeActive = true;
        currentChallenge = specificChallenge;
        SetupTriggerVisuals(trigger);
        resiliencePanel.SetActive(true);
        DeactivateAllSubPanels();
        switch (specificChallenge)
        {
            case ChallengeType.InnerCritic:
                innerCriticUI.SetActive(true);
                currentTaps = 0;
                tapCountText.text = "0 / " + requiredTaps;
                instructionText.text = "Silience the critic! Tap SPACE rapidly!";
                break;

            case ChallengeType.TensionRelease:
                tensionUI.SetActive(true);
                holdTimer = 0f;
                tensionSlider.value = 0f;
                instructionText.text = "You are carrying too much tension.";
                tensionInstructionText.text = "Hold SPACE to squeeze...";
                break;

            case ChallengeType.Paradox:
                paradoxUI.SetActive(true);
                paradoxTimer = 0f;
                instructionText.text = "A panic attack is a wave. Don't fight it.";
                paradoxTimerText.text = "Hands off the keyboard...";
                break;

            case ChallengeType.BoxBreathing:
                breathingUI.SetActive(true);
                holdTimer = 0f;
                breathingStage = 0;
                instructionText.text = "Reset your breath. Follow the guide.";
                break;

            case ChallengeType.Grounding:
                groundingUI.SetActive(true);
                instructionText.text = "Foggy mind? Press the Arrow Keys to Ground yourself.";
                GenerateGroundingSequence();
                UpdateGroundingText();
                break;
        }
        localChallengesFaced++;
        UpdateChallengeStat(1); 
        MindfulnessController mc = Object.FindFirstObjectByType<MindfulnessController>();
        if (mc != null)
        {
            mc.IncrementChallengeCount();
        }
        if (localChallengesFaced >= 50 && mc != null && !mc.AchievementIsAlreadyEarned("ACH_CHALLENGER"))
        {
            mc.SetLocalAchievementTrue("ACH_CHALLENGER");
            PlayFabAuth.SubmitPlayFabEvent("ChallengerEvent");
        }
    }
    private void UpdateChallengeStat(int amount)
    {
        var request = new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { 
                    StatisticName = "TotalChallengesFaced", 
                    Value = amount 
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, result => {
            Debug.Log("Successfully updated TotalChallengesFaced on PlayFab.");
        }, error => {
            Debug.LogError("Error updating stats: " + error.GenerateErrorReport());
        });
    }
    public bool IsChallengeRunning()
    {
        return challengeActive;
    }
    private void UpdateInnerCritic()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentTaps++;
            tapCountText.text = currentTaps + " / " + requiredTaps;
            if (currentTaps >= requiredTaps) CompleteChallenge(true);
        }
    }
    private void UpdateTensionRelease()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            holdTimer += Time.unscaledDeltaTime;
            tensionSlider.value = holdTimer / 3f; 
            
            if (holdTimer >= 3f)
            {
                tensionInstructionText.text = "RELEASE NOW!";
                tensionInstructionText.color = Color.red;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (holdTimer >= 3f) CompleteChallenge(true); 
            else 
            {
                holdTimer = 0f;
                tensionSlider.value = 0f;
                tensionInstructionText.text = "Hold SPACE to squeeze...";
                tensionInstructionText.color = Color.white;
            }
        }
    }
    private void UpdateParadox()
    {
        if (Input.anyKey)
        {
            paradoxTimer = 0f;
            paradoxTimerText.text = "Don't fight it! Let go.";
            paradoxTimerText.color = Color.red;
        }
        else
        {
            paradoxTimer += Time.unscaledDeltaTime;
            paradoxTimerText.color = Color.white;
            paradoxTimerText.text = "Stay still... " + (10f - paradoxTimer).ToString("F1") + "s";
            if (paradoxTimer >= 10f) CompleteChallenge(true);
        }
    }

    private void UpdateBoxBreathing()
    {
        holdTimer += Time.unscaledDeltaTime;
        if (breathingStage == 0) 
        {
            breatheStatusText.text = "Inhale... (Hold Space)";
            breathSlider.value = holdTimer / 4f;
            if (!Input.GetKey(KeyCode.Space)) 
            {
                holdTimer = 0;
                breathSlider.value = 0;
            }
            if (holdTimer >= 4f) 
            {
                holdTimer = 0;
                transitionBuffer = 0.2f; 
                breathingStage = 1; 
            }
        }
        
        else if (breathingStage == 1) 
        {
            breatheStatusText.text = "Hold... (Keep Holding for 4s)";
            breathSlider.value = 1f; 
            if (transitionBuffer > 0) transitionBuffer -= Time.unscaledDeltaTime;
            else if (!Input.GetKey(KeyCode.Space)) 
            {
                breathingStage = 0; 
                holdTimer = 0;
            }
            if (holdTimer >= 4f)
            {
                holdTimer = 0;
                transitionBuffer = 0.6f; 
                breathingStage = 2; 
            }
        }
        
        else if (breathingStage == 2) 
        {
            breatheStatusText.text = "Exhale... (Release Space)";
            breathSlider.value = 1f - (holdTimer / 4f);
            if (transitionBuffer > 0)
            {
                transitionBuffer -= Time.unscaledDeltaTime;
            }
            else
            {
                if (Input.GetKey(KeyCode.Space)) 
                {
                    breathingStage = 0; 
                    holdTimer = 0;
                }
            }
            if (holdTimer >= 4f) 
            {
                CompleteChallenge(true); 
            }
        }
    }
    private void UpdateGrounding()
    {
        if (Input.GetKeyDown(groundingSequence[groundingIndex]))
        {
            groundingIndex++;
            UpdateGroundingText();
            if (groundingIndex >= groundingSequence.Length) CompleteChallenge(true);
        }
        else if (Input.anyKeyDown) { }
    }
    private void GenerateGroundingSequence()
    {
        groundingSequence = new KeyCode[4];
        groundingIndex = 0;
        for(int i=0; i<4; i++)
        {
            int r = Random.Range(0, 4); 
            if (r==0) groundingSequence[i] = KeyCode.UpArrow;
            if (r==1) groundingSequence[i] = KeyCode.DownArrow;
            if (r==2) groundingSequence[i] = KeyCode.LeftArrow;
            if (r==3) groundingSequence[i] = KeyCode.RightArrow;
        }
    }
    private void UpdateGroundingText()
    {
        string display = "";
        for(int i=0; i<groundingSequence.Length; i++)
        {
            if (i < groundingIndex) display += "<color=green>OK</color>  "; 
            else display += groundingSequence[i].ToString() + "  ";
        }
        sequenceText.text = display;
    }
    private void SetupTriggerVisuals(TriggerType trigger)
    {
        switch (trigger)
        {
            case TriggerType.ObstacleHit:
                titleText.text = "IMPACT DETECTED";
                overlayImage.color = new Color(0.8f, 0.2f, 0.2f, 0.5f); 
                break;
            case TriggerType.InvisibleAnxiety:
                titleText.text = "ANXIETY SPIKE";
                overlayImage.color = new Color(0.5f, 0.2f, 0.8f, 0.5f); 
                break;
            case TriggerType.Milestone:
                titleText.text = "DEEPEN PRACTICE";
                overlayImage.color = new Color(0.2f, 0.6f, 0.8f, 0.5f); 
                break;
        }
    }
    private void DeactivateAllSubPanels()
    {
        innerCriticUI.SetActive(false);
        tensionUI.SetActive(false);
        paradoxUI.SetActive(false);
        breathingUI.SetActive(false);
        groundingUI.SetActive(false);
    }
    private void CompleteChallenge(bool success)
    {
        challengeActive = false;
        resiliencePanel.SetActive(false);
        Time.timeScale = 1f; 
        Time.fixedDeltaTime = originalFixedDeltaTime;
        if (success && mc != null)
        {
            if (mc.GetCalmness() < 20f && !mc.AchievementIsAlreadyEarned("ACH_CLUTCH_CALM"))
            {
                clutchCalmAchieved = true;
                PlayFabAuth.SubmitPlayFabEvent("ClutchCalmEvent");
                mc.SetLocalAchievementTrue("ACH_CLUTCH_CALM");
                Debug.Log("Clutch Achievement Triggered!");
            }
            mc.ApplyAnxietyPenalty(-successBonus); 
            Debug.Log("Challenge Passed!");
        }
        else
        {
            mc.ApplyAnxietyPenalty(failurePenalty);
        }
    }
}