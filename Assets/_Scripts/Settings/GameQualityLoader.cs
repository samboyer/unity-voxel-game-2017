using UnityEngine;

public class GameQualityLoader : MonoBehaviour
{
    private void Start()
    {
        GameQualityControls.QualityState state = SaveFileManager.Statics.LoadQualitySettings();

        if (state != null)
        {
            GameQualityControls.SetQualityState(state);
        }
        else
        {
            //HORRIBLE BUILTIN QUALITY SETTER
            switch (QualitySettings.GetQualityLevel())
            {
                case 0:
                    GameQualityControls.SetQualityState(GameQualityControls.PresetLow);
                    break;
                case 1:
                    GameQualityControls.SetQualityState(GameQualityControls.PresetMedium);
                    break;
                case 2:
                    GameQualityControls.SetQualityState(GameQualityControls.PresetHigh);
                    break;
                case 3:
                    GameQualityControls.SetQualityState(GameQualityControls.PresetLegendary);
                    break;
            }
            SaveFileManager.Statics.SaveQualitySettings(GameQualityControls.currentState);
        }
    }
}
