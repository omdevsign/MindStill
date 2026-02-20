using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using System.Collections;
using UnityEngine.UI; 

public class AppInitializer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text loadingText; 
    [SerializeField] private string baseMessage = "Loading";

    private bool isDoneLoading = false;

    void Start()
    {
        StartCoroutine(AnimateLoadingDots());
        StartCoroutine(CheckLoginStatus());
    }
    private IEnumerator AnimateLoadingDots()
    {
        int dotCount = 0;
        while (!isDoneLoading)
        {
            dotCount++;
            if (dotCount > 3) dotCount = 0;

            string dots = new string('.', dotCount);
            if (loadingText != null)
            {
                loadingText.text = baseMessage + dots;
            }

            yield return new WaitForSeconds(0.5f); 
        }
    }
    private IEnumerator CheckLoginStatus()
    {
        yield return new WaitForSeconds(1.5f); 

        if (PlayerPrefs.HasKey("MyCustomDeviceID"))
        {
            string savedID = PlayerPrefs.GetString("MyCustomDeviceID");

            var request = new PlayFab.ClientModels.LoginWithCustomIDRequest
            {
                CustomId = savedID,
                CreateAccount = false
            };

            PlayFabClientAPI.LoginWithCustomID(request, 
                result => {
                    isDoneLoading = true; 
                    PlayFabAuth.SetPlayerId(result.PlayFabId);
                    SceneManager.LoadScene("MainMenu");
                }, 
                error => {
                    isDoneLoading = true;
                    PlayerPrefs.DeleteKey("MyCustomDeviceID");
                    SceneManager.LoadScene("LoginScene");
                }
            );
        }
        else
        {
            isDoneLoading = true;
            SceneManager.LoadScene("LoginScene");
        }
    }
}