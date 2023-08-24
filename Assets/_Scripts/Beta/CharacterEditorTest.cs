using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

using SamBoyer.VoxelEngine;

public class CharacterEditorTest : MonoBehaviour
{
    static VoxelModel[] modelCollectionBuffer;
    static string modelCollectionName;

    public Component[] componentsToDisable;

    bool floorGridDisabled;

    private void Start()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    public void StartEditModel()
    {
        modelCollectionBuffer = new VoxelModel[] { VoxelModelLibrary.AddModelFromResources("editor/editor_newModel", true) };
        modelCollectionName = "newModel";

        floorGridDisabled = false;
        SceneManager.LoadScene("VoxelEditorMulti", LoadSceneMode.Additive);
    }

    public void StartEditCharacter()
    {
        //get models from world
        modelCollectionBuffer = CollectCharacterModels();
        modelCollectionName = "player";

        floorGridDisabled = true;

        SceneManager.LoadSceneAsync("VoxelEditorMulti",LoadSceneMode.Additive);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == "VoxelEditorMulti")
        {
            if (modelCollectionName == "player")
                GameObject.Find("EDITOR").GetComponent<VoxelEditorMulti>().LoadModelsToEdit(modelCollectionBuffer, new Vector3(10, 0, 10));

            else if(modelCollectionName=="newModel")
                GameObject.Find("EDITOR").GetComponent<VoxelEditorMulti>().LoadModelToEdit(modelCollectionBuffer[0]);

            foreach (Component c in componentsToDisable)
            {
                //print(c);
                //print(c as Behaviour);
                (c as Behaviour).enabled = false;
            }
            PieMenuController.pieEnabled = false;

            SceneManager.SetActiveScene(arg0);

            if (floorGridDisabled) GameObject.Find("EDITORFLOORGRID").SetActive(false);

        }
    }

    public void FinishEditModelCollection()
    {
        //get models from editor
        modelCollectionBuffer = VoxelEditorMulti.voxelModels;

        if (modelCollectionName == "player")
        {
            ApplyCharacterModels(modelCollectionBuffer);

            foreach(VoxelModel mdl in modelCollectionBuffer) //save the character model
            {
#if UNITY_EDITOR
                if (WorldSessionController.file != null) WorldSessionController.file.SaveVoxelModel(mdl);
#else
                WorldSessionController.file.SaveVoxelModel(mdl);
#endif
            }
        }

        else if (modelCollectionName == "newModel") //save the new model
        {
#if UNITY_EDITOR
            if(WorldSessionController.file!=null)WorldSessionController.file.SaveVoxelModel(modelCollectionBuffer[0], "customObjects/" + modelCollectionBuffer[0].modelName);
#else
            WorldSessionController.file.SaveVoxelModel(modelCollectionBuffer[0],"customObjects/"+modelCollectionBuffer[0].modelName);
#endif
            VoxelModelLibrary.AddModelToLibrary(modelCollectionBuffer[0], false);

            GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterInteractionController>().CurrentModel = modelCollectionBuffer[0];
        }


        modelCollectionBuffer = null;

        foreach (Behaviour c in componentsToDisable)
        {
            c.enabled = true;
        }
        PieMenuController.pieEnabled = true;

        SceneManager.UnloadSceneAsync("VoxelEditorMulti");

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("World"));
    }

    VoxelModel[] CollectCharacterModels()
    {
        Transform playerModel = GameObject.FindGameObjectWithTag("Player").transform.Find("MODEL");
        VoxelModel[] models = new VoxelModel[6];
        models[0] = playerModel.GetChild(0).GetComponent<VoxelRenderer>().Model; //torso
        models[1] = playerModel.GetChild(0).GetChild(0).GetComponent<VoxelRenderer>().Model;
        models[2] = playerModel.GetChild(0).GetChild(1).GetComponent<VoxelRenderer>().Model;
        models[3] = playerModel.GetChild(0).GetChild(2).GetComponent<VoxelRenderer>().Model;
        models[4] = playerModel.GetChild(0).GetChild(3).GetComponent<VoxelRenderer>().Model;
        models[5] = playerModel.GetChild(0).GetChild(4).GetComponent<VoxelRenderer>().Model;
        return models;
    }

    void ApplyCharacterModels(VoxelModel[] models)
    {
        Transform playerModel = GameObject.FindGameObjectWithTag("Player").transform.Find("MODEL");
        playerModel.GetChild(0).GetComponent<VoxelRenderer>().Model = models[0]; //torso
        playerModel.GetChild(0).GetChild(0).GetComponent<VoxelRenderer>().Model = models[1];
        playerModel.GetChild(0).GetChild(1).GetComponent<VoxelRenderer>().Model = models[2];
        playerModel.GetChild(0).GetChild(2).GetComponent<VoxelRenderer>().Model = models[3];
        playerModel.GetChild(0).GetChild(3).GetComponent<VoxelRenderer>().Model = models[4];
        playerModel.GetChild(0).GetChild(4).GetComponent<VoxelRenderer>().Model = models[5];
    }
}
