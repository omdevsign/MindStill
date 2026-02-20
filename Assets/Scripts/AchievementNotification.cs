using UnityEngine;
using System.Collections;
using UnityEngine.UI;   

public class AchievementNotification : MonoBehaviour
{
    public static AchievementNotification Instance;

    [SerializeField] private GameObject popUpPanel;
    [SerializeField] private Text achievementNameText;
    [SerializeField] private float displayDuration = 3f;

    void Awake()
    {
        Instance = this;
        popUpPanel.SetActive(false);
    }

    public void ShowNotification(string name)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayRoutine(name));
    }

    private IEnumerator DisplayRoutine(string name)
    {
        achievementNameText.text = name;
        popUpPanel.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        popUpPanel.SetActive(false);
    }
}