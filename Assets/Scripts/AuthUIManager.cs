using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AuthUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signupPanel;

    [Header("Input Fields")]
    [SerializeField] private InputField loginEmail;
    [SerializeField] private InputField loginPassword;
    [SerializeField] private InputField signupUsername;
    [SerializeField] private InputField signupEmail;
    [SerializeField] private InputField signupPassword;

    [Header("Status")]
    [SerializeField] private Text statusText;

    public void ShowSignup() 
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        statusText.text = "";
    }
    public void ShowLogin() 
    {
        signupPanel.SetActive(false);
        loginPanel.SetActive(true);
        statusText.text = "";
    }
    public void OnLoginClicked()
    {
        var request = new LoginWithEmailAddressRequest 
        {
            Email = loginEmail.text,
            Password = loginPassword.text
        };

        statusText.text = "Logging in...";
        PlayFabClientAPI.LoginWithEmailAddress(request, OnAuthSuccess, OnError);
    }
    public void OnSignupClicked()
    {
        if (string.IsNullOrEmpty(signupEmail.text) || string.IsNullOrEmpty(signupPassword.text)) 
        {
            statusText.text = "Please fill all fields.";
            return;
        }

        statusText.text = "Creating account...";
        var request = new RegisterPlayFabUserRequest 
        {
            Email = signupEmail.text,
            Password = signupPassword.text,
            Username = signupUsername.text,
            DisplayName = signupUsername.text
        };

        statusText.text = "Creating account...";
        PlayFabClientAPI.RegisterPlayFabUser(request, OnAuthSuccess, OnError);
    }
    private void OnAuthSuccess(LoginResult result) 
    {
        PlayFabAuth.SetPlayerId(result.PlayFabId);
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        PlayerPrefs.SetString("MyCustomDeviceID", deviceID);
        PlayerPrefs.Save();
        var linkRequest = new LinkCustomIDRequest {
            CustomId = deviceID,
            ForceLink = true
        };
        PlayFabClientAPI.LinkCustomID(linkRequest, 
            linkResult => Debug.Log("Device Linked for Silent Login"),
            error => Debug.Log("Link Error: " + error.GenerateErrorReport())
        );

        SceneManager.LoadScene("MainMenu");
    }
    private void OnAuthSuccess(RegisterPlayFabUserResult result) 
    {
       statusText.text = "Account created! Syncing...";
        PlayFabAuth.SetPlayerId(result.PlayFabId);
        SceneManager.LoadScene("MainMenu");
    }
    private void OnError(PlayFabError error)
    {
        statusText.text = "Error: " + error.ErrorMessage;
    }
    private void UpdateDisplayName(string name)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, 
            result => Debug.Log("Display Name Set!"), 
            error => Debug.Log(error.GenerateErrorReport())
        );
    }
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}