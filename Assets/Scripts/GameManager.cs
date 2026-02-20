using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{

    [Header("Manager References")]
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private MindfulnessController mindfulnessController;
    [SerializeField] private ResilienceManager resilienceManager;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text realtimeScoreText;

    [Header("Difficulty and Pace")]
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float speedIncreaseRate = 0.05f;
    [SerializeField] private float currentSpeed;

    [Header("Invisible Challenge Settings")]
    [SerializeField] private float minAnxietyTime = 45f;
    [SerializeField] private float maxAnxietyTime = 90f;
    private float anxietyTimer;
    private bool challengeTriggered = false;

    [Header("Game State")]
    public float sessionScore = 0f;
    public bool isGameOver = false;
    private bool isPaused = false;
    [SerializeField] private GameObject pausePanel;

    [Header("Resume Countdown")]
    [SerializeField] public Text countdownText;

    public bool centuryAchieved = false;

    void Start()
    {
        Time.timeScale = 1f;
        sessionScore = 0f;
        isGameOver = false;
        ResetAnxietyTimer();

        if (resilienceManager == null) 
            resilienceManager = FindFirstObjectByType<ResilienceManager>();
    }

    void Update()
    {
        if (isGameOver) return; 

        sessionScore += Time.deltaTime;
        if (realtimeScoreText != null)
            realtimeScoreText.text = "Score: " + sessionScore.ToString("F1") + "s";
        
        if (sessionScore >= 100f)
        {
            MindfulnessController mc = FindFirstObjectByType<MindfulnessController>();
            if (mc != null && !mc.AchievementIsAlreadyEarned("ACH_CENTURY"))
            {
                // We use the same 'SendAchievement' logic pattern
                PlayFabAuth.SubmitPlayFabEvent("CenturyEvent");
                // Mark it true locally in MC so it doesn't run every frame
                mc.SetLocalAchievementTrue("ACH_CENTURY"); 
            }
        }
        currentSpeed = baseSpeed + (sessionScore * speedIncreaseRate);
        currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);
        
        if (levelGenerator != null)
            levelGenerator.SetMoveSpeed(currentSpeed);
        
        anxietyTimer -= Time.deltaTime;
        if (anxietyTimer <= 0)
        {
            TriggerInvisibleChallenge();
            ResetAnxietyTimer();
        }
        
        if (sessionScore > 10 && Mathf.FloorToInt(sessionScore) % 100 == 0 && !challengeTriggered)
        {
            TriggerMilestoneChallenge();
            challengeTriggered = true; 
            Invoke("ResetChallengeFlag", 2f); 
        }
    
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        if (mindfulnessController != null && mindfulnessController.GetCalmness() <= 0)
        {
            Debug.Log("Calmness reached zero. Game Over.");
            GameOver();
        }
    }
    
    public bool IsGameFlowBlocked()
    {
        return isGameOver || isPaused;
    }
    
    public bool pausePanelActive()
    {
        return Time.timeScale == 0f && !isGameOver; 
    }
    private void ResetAnxietyTimer()
    {
        anxietyTimer = Random.Range(minAnxietyTime, maxAnxietyTime);
    }

    private void ResetChallengeFlag()
    {
        challengeTriggered = false;
    }

    private void TriggerInvisibleChallenge()
    {
        if (resilienceManager != null)
        {
            resilienceManager.TriggerChallenge(
                ResilienceManager.TriggerType.InvisibleAnxiety, 
                ResilienceManager.ChallengeType.Paradox
            );
        }
    }

    private void TriggerMilestoneChallenge()
    {
        if (resilienceManager != null)
        {
            int r = Random.Range(3, 5); 
            resilienceManager.TriggerChallenge(
                ResilienceManager.TriggerType.Milestone, 
                (ResilienceManager.ChallengeType)r
            );
        }
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
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame()
    {
        if (isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        Debug.Log("Game Paused.");
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        StartCoroutine(ResumeCountdownRoutine());
        /*ResilienceManager rm = FindFirstObjectByType<ResilienceManager>();
        if (rm != null && rm.IsChallengeRunning()) 
        {
            Time.timeScale = 0.05f; 
            Debug.Log("Resuming back into Challenge Slow-Mo");
        }
        else
        {
            Time.timeScale = 1f; 
        }

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("Game Resumed.");*/
    }

    private IEnumerator ResumeCountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
            count--;
        }

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.5f);
        countdownText.gameObject.SetActive(false);

        ApplyCorrectTimeScale();
    }
    
    private void ApplyCorrectTimeScale()
    {
        ResilienceManager rm = FindFirstObjectByType<ResilienceManager>();
        
        if (rm != null && rm.IsChallengeRunning())
        {
            Time.timeScale = 0.05f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /*private void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f; 
        
        Debug.Log("Game Over! Time Survived: " + sessionScore.ToString("F1") + " seconds.");
        if (sessionScore > PlayerPrefs.GetFloat("HighScore", 0))
        {
            PlayerPrefs.SetFloat("HighScore", sessionScore);
            PlayerPrefs.Save();
        }
    }*/

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
        finalScoreText.text = "Time Survived: " + sessionScore.ToString("F1") + "s";
        highScoreText.text = "High Score: Loading...";
        gameOverPanel.SetActive(true);
        SendAllStatistics((int)sessionScore);
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            RetrieveAndDisplayHighScore();
        }
        MindfulnessController mc = Object.FindFirstObjectByType<MindfulnessController>();
        if (mc != null)
        {
            mc.CheckRankAchievementsAtGameOver();
        }
        Debug.Log("Game Over. Scores Sent.");
    }

    private void SendAllStatistics(int sessionScore)
    {
        var statsToUpdate = new List<StatisticUpdate>();
        statsToUpdate.Add(new StatisticUpdate
        {
            StatisticName = "TimeSurvivedHighScore",
            Value = sessionScore
        });
        statsToUpdate.Add(new StatisticUpdate
        {
            StatisticName = "TotalPlaytime",
            Value = sessionScore
        });
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = statsToUpdate
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreUploadSuccess, OnScoreUploadError);
    }

    private void OnScoreUploadSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("All statistics uploaded successfully!");
    }

    private void OnScoreUploadError(PlayFabError error)
    {
        Debug.LogError("Error uploading statistics: " + error.GenerateErrorReport());
    }
    public void DisplayFinalHighScore(int highScore)
    {
        highScoreText.text = "High Score: " + highScore.ToString("F1") + "s";
    }
    private void RetrieveAndDisplayHighScore()
    {
        // Request only the current player's stats
        var dataRequest = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(dataRequest, OnGetPersonalStatsSuccess, OnGetLeaderboardError);
    }
    private void OnGetPersonalStatsSuccess(GetPlayerStatisticsResult result)
    {
        if (result.Statistics != null)
        {
            int personalHighScore = 0;
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == "TimeSurvivedHighScore")
                {
                    personalHighScore = stat.Value;
                    break;
                }
            }
            highScoreText.text = "High Score: " + personalHighScore.ToString() + "s";
        }
    }
    private void OnGetLeaderboardError(PlayFabError error)
    {
        Debug.LogError("Error retrieving leaderboard for Game Over screen: " + error.GenerateErrorReport());
        highScoreText.text = "High Score: Error Loading";
    }
}