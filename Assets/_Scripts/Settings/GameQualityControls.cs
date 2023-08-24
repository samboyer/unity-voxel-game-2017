using UnityEngine;

public static class GameQualityControls
{
    public static QualityState currentState = new QualityState();

    public delegate void QualityChangeHandler();
    public static event QualityChangeHandler QualityChanged;

    public static void SetQualityState(QualityState newState)
    {
        currentState = newState;
        if(QualityChanged!=null)QualityChanged();
        ApplyGlobalQualityChanges();
    }

    static void ApplyGlobalQualityChanges()
    {
        switch (currentState.shadowQuality)
        {
            case 0:
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowCascades = 2;
                break;
            case 1:
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowCascades = 2;
                break;
            case 2:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.shadowCascades = 4;
                break;
            case 3:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.shadowCascades = 4;
                break;
        }
        QualitySettings.shadowDistance = currentState.shadowDistance;

        QualitySettings.vSyncCount = currentState.vSyncEnabled ? 1 : 0;
        Screen.SetResolution(currentState.screenResWidth, currentState.screenResHeight, currentState.fullscreenEnabled);
    }

    [System.Serializable]
    public class QualityState
    {
        public int basePreset = -1;

        //post FX (CPU)
        public bool aoEnabled = true, motionBlurEnabled = false, fxaaEnabled = true, dofEnabled = false, waterReflectionsEnabled = false;

        //global quality FX (CPU)
        //public int shadowCascades = 4; //should be 0,2,4
        //public int shadowLevel = 2; // 0, 1 or 2 for none, hard, soft
        public float shadowDistance = 150;
        public int shadowQuality = 2; //0,1,2 or 3
        public bool vSyncEnabled = true;

        public int screenResWidth = 1280, screenResHeight = 720;
        public bool fullscreenEnabled = true;

        //Camera settings (GPU)
        public float cameraFOV = 60;
        public float maxRenderDistance = 25000;

        //worldGen settings (CPU) (Only set when World is reloaded (WorldTerrainGenerator.Start))
        public float worldGenMaxDistance = 20000;
        public float worldGenHiResDistance = 1000; //minimum distance where chunks are max gen
        public float worldGenResolutionFalloff = 0.0001f;
        public float worldGenFoliageMaxDistance = 1000;
        public float worldGenObjectMaxDistance = 1000;

        public QualityState Copy()
        {
            return MemberwiseClone() as QualityState;
        }
    }

    public static QualityState PresetLegendary = new QualityState
    {
        basePreset = 3,

        aoEnabled = true,
        motionBlurEnabled = true,
        fxaaEnabled = true,
        dofEnabled = true,
        waterReflectionsEnabled = true,

        shadowDistance = 300, 
        shadowQuality = 3,
        vSyncEnabled = true,

        screenResWidth = 1280,
        screenResHeight = 720,
        fullscreenEnabled = true,

        cameraFOV = 60,
        maxRenderDistance = 50000,

        worldGenMaxDistance = 20000,
        worldGenHiResDistance = 2000,
        worldGenResolutionFalloff = 0.0001f,
        worldGenFoliageMaxDistance = 1500,
        worldGenObjectMaxDistance = 2000,
    };

    public static QualityState PresetHigh = new QualityState
    {
        basePreset = 2,

        aoEnabled = true,
        motionBlurEnabled = false,
        fxaaEnabled = true,
        dofEnabled = false,
        waterReflectionsEnabled = false,

        shadowDistance = 150,
        shadowQuality = 2,
        vSyncEnabled = true,

        screenResWidth = 1280,
        screenResHeight = 720,
        fullscreenEnabled = true,

        cameraFOV = 60,
        maxRenderDistance = 50000,

        worldGenMaxDistance = 20000,
        worldGenHiResDistance = 1500,
        worldGenResolutionFalloff = 0.0001f,
        worldGenFoliageMaxDistance = 1000,
        worldGenObjectMaxDistance = 1000,
    };

    public static QualityState PresetMedium = new QualityState
    {
        basePreset = 1,

        aoEnabled = false,
        motionBlurEnabled = false,
        fxaaEnabled = false,
        dofEnabled = false,
        waterReflectionsEnabled = false,

        shadowDistance = 100,
        shadowQuality = 1,
        vSyncEnabled = true,

        screenResWidth = 1280,
        screenResHeight = 720,
        fullscreenEnabled = true,

        cameraFOV = 60,
        maxRenderDistance = 30000,

        worldGenMaxDistance = 15000,
        worldGenHiResDistance = 1000,
        worldGenResolutionFalloff = 0.0001f,
        worldGenFoliageMaxDistance = 1000,
        worldGenObjectMaxDistance = 1000,
    };

    public static QualityState PresetLow = new QualityState
    {
        basePreset = 0,

        aoEnabled = false,
        motionBlurEnabled = false,
        fxaaEnabled = false,
        dofEnabled = false,
        waterReflectionsEnabled = false,

        shadowDistance = 100,
        shadowQuality = 0,
        vSyncEnabled = true,

        screenResWidth = 1280,
        screenResHeight = 720,
        fullscreenEnabled = true,

        cameraFOV = 60,
        maxRenderDistance = 20000,

        worldGenMaxDistance = 15000,
        worldGenHiResDistance = 1000,
        worldGenResolutionFalloff = 0.0001f,
        worldGenFoliageMaxDistance = 1000,
        worldGenObjectMaxDistance = 1000,
    };
}