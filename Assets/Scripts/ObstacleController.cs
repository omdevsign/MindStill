using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] private float anxietyPenalty = 10f; 

    [Header("Audio")]
    [SerializeField] private AudioSource obstacleAudioSource; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {       
            MindfulnessController mc = FindFirstObjectByType<MindfulnessController>();
            ResilienceManager rm = FindFirstObjectByType<ResilienceManager>();
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (mc != null)
            {
                if (obstacleAudioSource != null && obstacleAudioSource.clip != null)
                {
                    AudioSource.PlayClipAtPoint(obstacleAudioSource.clip, transform.position, 1.0f);
                }
                mc.ApplyAnxietyPenalty(anxietyPenalty);
                if (mc.GetCalmness() <= 0)
                {
                    if (gm != null) gm.GameOver();
                    Destroy(gameObject);
                    return; 
                }
                if (rm != null)
                {
                    int r = Random.Range(0, 3);
                    ResilienceManager.ChallengeType type;
                    if (r == 0) type = ResilienceManager.ChallengeType.InnerCritic;
                    else if (r == 1) type = ResilienceManager.ChallengeType.TensionRelease;
                    else type = ResilienceManager.ChallengeType.Grounding;

                    rm.TriggerChallenge(ResilienceManager.TriggerType.ObstacleHit, type);   
                }
            }
            Destroy(gameObject);
        }
    }
}