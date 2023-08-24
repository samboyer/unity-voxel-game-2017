using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {

    public RectTransform worldsScrollContentBox;

    public GameObject worldsListItem;


    MenuSectionMover newWorldDialogMover, worldSelectMover;

	void Start () {

        SaveFileManager.Statics.CreateDirectoryStructure();

        newWorldDialogMover = GameObject.Find("NEWWORLD").GetComponent<MenuSectionMover>();
        worldSelectMover = GameObject.Find("WORLDSELECT").GetComponent<MenuSectionMover>();

        //if (WorldSessionController.current != null) SaveFileManager.current.CloseSaveFile(); //this could be veeeery bad .-.

        worldsScrollContentBox.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500); //unimportant values, but messing with it fixes some reload bug .-.

        FindWorlds(); //find all saved worlds and put in list.
	}
	
    void FindWorlds()
    {
        //MenuWorldData[] worlds = SaveFileManager.Statics.GetAllSaveFiles();
        List<MenuWorldData> unsortedWorlds = new List<MenuWorldData>(SaveFileManager.Statics.GetAllSaveFiles());

        worldsScrollContentBox.sizeDelta = new Vector2(0, unsortedWorlds.Count * 120);

        Debug.Log(unsortedWorlds.Count + " saved worlds found.");

        //sort the worlds by date modified
        //this is a selection sort.
        //cause im lazy.
        List<MenuWorldData> sortedWorlds = new List<MenuWorldData>(unsortedWorlds.Count);
        while (unsortedWorlds.Count > 0)
        {
            //find world with biggest lastPlayed value
            double biggest = double.MinValue;
            int biggestI = -1;
            for(int i = 0; i < unsortedWorlds.Count; i++)
            {
                if (unsortedWorlds[i].lastPlayed > biggest)
                {
                    biggest = unsortedWorlds[i].lastPlayed;
                    biggestI = i;
                }
            }
            sortedWorlds.Add(unsortedWorlds[biggestI]);
            unsortedWorlds.RemoveAt(biggestI);
        }


        for (int i = 0; i< sortedWorlds.Count;i++)
        {
            GameObject gO = Instantiate(worldsListItem, worldsScrollContentBox,false);
            gO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * 120);
            gO.transform.GetChild(0).Find("NAME").GetComponent<Text>().text = sortedWorlds[i].worldName;
            gO.transform.GetChild(0).Find("INFO").GetComponent<Text>().text = new System.DateTime(1970,1,1).AddSeconds(sortedWorlds[i].lastPlayed).ToString("yyyy-MM-dd HH:mm");
            gO.transform.GetChild(0).Find("SCREENSHOT").GetComponent<RawImage>().texture = sortedWorlds[i].screenshot;
            string name = sortedWorlds[i].fileName;
            gO.transform.GetChild(0).Find("PLAYBUTTON").GetComponent<Button>().onClick.AddListener(() => PlayWorld(name));
        }
    }

    void PlayWorld(string fileName)
    {
        WorldSessionController.StartSession(fileName, false);
        //WorldSessionController.worldName = worldName;
        //SceneManager.LoadScene("World");
    }


    //BUTTON STUFF
    public void ExitGame()
    {
        Application.Quit();
    }


    public void OpenNewWorldDialog()
    {
        newWorldDialogMover.OpenSection();
        worldSelectMover.easeOutDirection = MenuSectionMover.SwapPopupDirection(newWorldDialogMover.easeInDirection);
        worldSelectMover.CloseSection();
        RandomiseSeed();
    }
    public void CloseNewWorldDialog()
    {
        newWorldDialogMover.CloseSection();
        worldSelectMover.easeInDirection = MenuSectionMover.SwapPopupDirection(newWorldDialogMover.easeOutDirection);
        worldSelectMover.OpenSection();
    }

    public void StartNewWorld()
    {
        string name = GameObject.Find("NEWWORLDNAME").GetComponent<InputField>().text;
        if (name == "") return;
        int seed = int.Parse(GameObject.Find("NEWWORLDSEED").GetComponent<InputField>().text);
        SaveFileManager.Create(name, seed);

        newWorldDialogMover.CloseSection();

        WorldSessionController.StartSession(SaveFileManager.Statics.MakeStringFileSafe(name), true);
    }

    public void RandomiseSeed()
    {
        GameObject.Find("NEWWORLDSEED").GetComponent<InputField>().text = Random.Range(-2147483647,2147483647).ToString();
    }

}

public class MenuWorldData
{
    public string worldName, fileName;
    public double lastPlayed;
    public Texture2D screenshot;
}
