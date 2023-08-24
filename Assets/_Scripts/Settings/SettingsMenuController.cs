using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour {

    public ScreenResolution[] supportedResolutions;

    [Header("UI Elements")]
    public Dropdown elementResolutionDropdown;
    public Text elementResolutionLabel;
    public Toggle elementFullscreenToggle;
    public Dropdown elementPresetQualityDropdown;
    public Text elementPresetQualityLabel;

    public bool isOpen;

    private void Start()
    {
        List<Dropdown.OptionData> resolutionOptions = new List<Dropdown.OptionData>();
        foreach(ScreenResolution res in supportedResolutions)
        {
            resolutionOptions.Add(new Dropdown.OptionData(res.width + " x " + res.height));
        }
        elementResolutionDropdown.options = resolutionOptions;
    }

    public void OpenSettingsDialog() //NOTE: this needs to be called to initialise the menu's values!
    {
        bool standardResolution = false;
        for (int i = 0; i < supportedResolutions.Length; i++)
        {
            if (supportedResolutions[i].width == GameQualityControls.currentState.screenResWidth && supportedResolutions[i].height == GameQualityControls.currentState.screenResHeight)
            {
                elementResolutionDropdown.value = i;
                standardResolution = true;
            }
        }
        if (!standardResolution)
        {
            elementResolutionDropdown.value = -1;
            elementResolutionLabel.text = "CUSTOM";
        }

        elementFullscreenToggle.isOn = GameQualityControls.currentState.fullscreenEnabled;


        if ((elementPresetQualityDropdown.value = GameQualityControls.currentState.basePreset) == -1)
            elementPresetQualityLabel.text = "CUSTOM";


        //TODO: set all the UI options here based off current quality state.

        GetComponent<MenuSectionMover>().OpenSection();

        isOpen = true;
    }

    public void FinishSettingsDialog()
    {
        GameQualityControls.QualityState state = GameQualityControls.currentState.Copy();

        if(elementResolutionDropdown.value != -1)
        {
            ScreenResolution res = supportedResolutions[elementResolutionDropdown.value];
            state.screenResWidth = res.width;
            state.screenResHeight = res.height;
        }

        state.fullscreenEnabled = elementFullscreenToggle.isOn;

        state.basePreset = elementPresetQualityDropdown.value;

        //TODO: collect all the setting options here and put into state.

        GameQualityControls.SetQualityState(state);

        SaveFileManager.Statics.SaveQualitySettings(GameQualityControls.currentState);

        GetComponent<MenuSectionMover>().CloseSection();

        isOpen = false;
    }

    public void PresetQualitySelected()
    {
        GameQualityControls.QualityState state = new GameQualityControls.QualityState();

        switch (elementPresetQualityDropdown.value)
        {
            case 0:
                state = GameQualityControls.PresetLow;
                break;
            case 1:
                state = GameQualityControls.PresetMedium;
                break;
            case 2:
                state = GameQualityControls.PresetHigh;
                break;
            case 3:
                state = GameQualityControls.PresetLegendary;
                break;
        }
        //TEMP CODE
        state.screenResHeight = GameQualityControls.currentState.screenResHeight;
        state.screenResWidth = GameQualityControls.currentState.screenResWidth;
        state.fullscreenEnabled = GameQualityControls.currentState.fullscreenEnabled;
        GameQualityControls.currentState = state;

        //TODO: update *most of* the options UI here, based on 'state', but don't set currentState!
    }


    public void CustomQualityControlSelected()
    {
        elementPresetQualityDropdown.value = -1;
        elementPresetQualityLabel.text = "CUSTOM";
    }

    [System.Serializable]
    public class ScreenResolution {public int width; public int height; }
}
