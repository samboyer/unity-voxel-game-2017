using UnityEngine;

public class VoxelEditorMultiFinisher : MonoBehaviour
{
    public void FinishEditModels()
    {
        if (VoxelEditorMulti.voxelModels[0].modelName == "editor/editor_newModel")
        {
            VoxelEditorMulti.voxelModels[0].modelName = Extensions.GenerateBase64String(10);
        }
        GameObject.Find("BETA_EDITOR").GetComponent<CharacterEditorTest>().FinishEditModelCollection();
    }

    public void SetModelName()
    {
        VoxelEditorMulti.voxelModels[0].modelName = GameObject.Find("MODELNAMEFIELD").GetComponent<UnityEngine.UI.InputField>().text;

        FinishEditModels();
    }
}
