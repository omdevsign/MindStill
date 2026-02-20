using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider volumeSlider;

    // We use unique keys to ensure no overlap
    private const string VOL_KEY = "GameVolume";
    private const string QUAL_KEY = "GameQuality";
    private const string FULL_KEY = "GameFullscreen";

    void Start()
    {
        // APPLY TO ENGINE ONCE AT START
        AudioListener.volume = PlayerPrefs.GetFloat(VOL_KEY, 0.75f);
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(QUAL_KEY, 2));
        Screen.fullScreen = PlayerPrefs.GetInt(FULL_KEY, 1) == 1;
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        
        // Sync the UI sliders/toggles to match what is ALREADY in PlayerPrefs
        volumeSlider.value = PlayerPrefs.GetFloat(VOL_KEY, 0.75f);
        qualityDropdown.value = PlayerPrefs.GetInt(QUAL_KEY, 2);
        fullscreenToggle.isOn = PlayerPrefs.GetInt(FULL_KEY, 1) == 1;
    }

    // TRIGGERED BY SLIDER
    public void OnVolumeChange(float val)
    {
        AudioListener.volume = val;
        PlayerPrefs.SetFloat(VOL_KEY, val);
    }

    // TRIGGERED BY DROPDOWN
    public void OnQualityChange(int val)
    {
        QualitySettings.SetQualityLevel(val);
        PlayerPrefs.SetInt(QUAL_KEY, val);
    }

    // TRIGGERED BY TOGGLE
    public void OnFullscreenChange(bool val)
    {
        Screen.fullScreen = val;
        PlayerPrefs.SetInt(FULL_KEY, val ? 1 : 0);
    }

    public void CloseSettings()
    {
        PlayerPrefs.Save(); // Forces the file to write
        settingsPanel.SetActive(false);
    }
}