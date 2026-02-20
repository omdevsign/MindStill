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

    // --- TOGGLE LOGIC ---
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

    // --- LOGIN LOGIC ---
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

    // --- SIGNUP LOGIC ---
    public void OnSignupClicked()
    {
        if (string.IsNullOrEmpty(signupEmail.text) || string.IsNullOrEmpty(signupPassword.text)) 
        {
            statusText.text = "Please fill all fields.";
            return;
        }

        //signupButton.interactable = false; 
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

    // --- CALLBACKS ---
    private void OnAuthSuccess(LoginResult result) 
    {
        PlayFabAuth.SetPlayerId(result.PlayFabId);

        // Generate a unique ID for this specific PC/Device
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        PlayerPrefs.SetString("MyCustomDeviceID", deviceID);
        PlayerPrefs.Save();

        // LINK this device to the account so Silent Login works later
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
        
        // We manually set the ID immediately from the registration result
        PlayFabAuth.SetPlayerId(result.PlayFabId);

        // Instead of calling OnLoginClicked (which uses Login UI fields), 
        // we jump straight to the Main Menu because the user is technically authorized.
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