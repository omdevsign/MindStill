using UnityEngine;

public class PathEffectController : MonoBehaviour
{
    [SerializeField] private float anxietyRateMultiplier = 2.0f; 

    private MindfulnessController mc;
    private bool playerIsOnPath = false;

    void Start()
    {
        mc = FindFirstObjectByType<MindfulnessController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsOnPath = true;
            
            if (mc != null)
            {
                mc.SetPassiveAnxietyMultiplier(anxietyRateMultiplier);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsOnPath = false;
            
            if (mc != null)
            {
                mc.SetPassiveAnxietyMultiplier(1.0f);
            }
        }
    }
}