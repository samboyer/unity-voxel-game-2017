using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Ionic.Zip;
//using LitJson;
using Newtonsoft.Json;
using SamBoyer.VoxelEngine;

public partial class SaveFileManager{

    const string VERSIONNUMBER = "0";
    const string WORLDFILEEXTENSION = ".voxelworld";

    //public static SaveFileManager current;
    public static string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\" + Application.companyName + "\\" + Application.productName;

    static string worldsDataPath { get { return appDataPath + "\\worlds"; } }

    static string[] startingColors = new string[]
    {
        "101010FF",
        "808080FF",
        "F0F0F0FF",
        "FF3232FF",
        "32FF32FF",
        "3232FFFF",
        "803232FF",
        "328032FF",
        "323280FF",
    };

    /*static byte[][] startingColours = new byte[][]
    {
        new byte[]{ 16, 16, 16,255},
        new byte[]{128,128,128,255},
        new byte[]{240,240,240,255},
        new byte[]{255, 50, 50,255},
        new byte[]{ 50,255, 50,255},
        new byte[]{ 50, 50,255,255},
        new byte[]{127, 25, 25,255},
        new byte[]{ 25,127, 25,255},
        new byte[]{ 25, 25,127,255},
    };*/

    //static stuff
    public class Statics
    {

        public static GameQualityControls.QualityState LoadQualitySettings()
        {
            StreamReader fs;
            if (!File.Exists(appDataPath + "\\qualitySettings")) return null;
            try
            {
                fs = File.OpenText(appDataPath + "\\qualitySettings");
            }
            catch
            {
                return null;
            }
            if (fs == null) return null;
            //GameQualityControls.QualityState state = JsonMapper.ToObject<GameQualityControls.QualityState>(fs);
            JsonSerializer jsonSerializer = new JsonSerializer();
            GameQualityControls.QualityState state = jsonSerializer.Deserialize<GameQualityControls.QualityState>(new JsonTextReader(fs));
            fs.Close();
            return state;
        }
        public static bool SaveQualitySettings(GameQualityControls.QualityState state)
        {
            if (!File.Exists(appDataPath + "\\qualitySettings"))
                File.Create(appDataPath + "\\qualitySettings").Close();

            FileStream fs = File.Open(appDataPath + "\\qualitySettings", FileMode.Truncate);
            fs.Flush();
            StreamWriter sw = new StreamWriter(fs);
            //sw.Write(JsonMapper.ToJson(state));
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(sw, state);
            sw.Close();
            fs.Close();
            return true;
        }

        public static void CreateDirectoryStructure()
        {
            if (!Directory.Exists(worldsDataPath)) Directory.CreateDirectory(worldsDataPath);
        }

        public static List<MenuWorldData> GetAllSaveFiles()
        {
            List<MenuWorldData> worlds = new List<MenuWorldData>();
            string[] worldFiles = Directory.GetFiles(worldsDataPath);
            foreach (string path in worldFiles)
            {
                if (path.EndsWith(WORLDFILEEXTENSION) && path[path.LastIndexOf('\\')+1]!='.')
                {
                    try
                    {
                        ZipFile zipFile = new ZipFile(path);

                        if (LoadStringFileFromZip(zipFile, "VE_VERSION") == VERSIONNUMBER)
                        {
                            MenuWorldData menuData = new MenuWorldData();
                            //SaveFileObjects.SaveFileWorldState worldData = JsonMapper.ToObject<SaveFileObjects.SaveFileWorldState>(LoadStringFileFromZip(zipFile, "worldData"));
                            SaveFileObjects.SaveFileWorldState worldData = JsonConvert.DeserializeObject<SaveFileObjects.SaveFileWorldState>(LoadStringFileFromZip(zipFile, "worldData"));

                            menuData.worldName = worldData.worldName;

                            int slashIndex = path.LastIndexOf('\\')+1;

                            menuData.fileName = path.Substring(slashIndex, path.LastIndexOf('.')-slashIndex);
                            menuData.lastPlayed = worldData.lastModified;

                            Texture2D tex = new Texture2D(200, 150);
                            if (tex.LoadImage(LoadDataFileFromZip(zipFile, "screenshot")))
                            {
                                menuData.screenshot = tex;
                            }
                            worlds.Add(menuData);
                        }
                        zipFile.Dispose();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log(ex);
                        continue;
                    }
                }
            }
            return worlds;
        }

        public static string MakeStringFileSafe(string str)
        {
            char[] disallowedChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            foreach (char c in disallowedChars)
            {
                str = str.Replace(c.ToString(), "");
            }
            return str;
        }

        static string LoadStringFileFromZip(ZipFile zip, string filename)
        {
            try
            {
                if (zip.ContainsEntry(filename))
                {
                    foreach(ZipEntry entry in zip.Entries)
                    {
                        if (entry.FileName == filename)
                        {
                            Stream s = entry.OpenReader();

                            StreamReader sr = new StreamReader(s);
                            string file = sr.ReadToEnd();
                            sr.Close();
                            s.Close();
                            return file;
                        }
                    }
                }
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
                return null;
            }
        }
        static byte[] LoadDataFileFromZip(ZipFile zip, string filename)
        {
            try
            {
                if (zip.ContainsEntry(filename))
                {
                    foreach (ZipEntry entry in zip.Entries)
                    {
                        if (entry.FileName == filename)
                        {
                            Stream s = entry.OpenReader();
                            byte[] data = new byte[s.Length];
                            s.Read(data, 0, (int)s.Length);
                            s.Close();
                            return data;

                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }


    //INSTANCE STUFF
    string saveFilePath;

    SaveFileObjects.SaveFileWorldState _worldState;
    public SaveFileObjects.SaveFileWorldState worldState
    {
        get { if (_worldState == null) return _worldState = LoadWorldData(); else return _worldState; }
        set { _worldState = value; }
    }

    ZipFile _zf;
    ZipFile zf {
        get { if (_zf == null) return OpenOrCreateZipFile();
            else return _zf; }
        set { _zf = value; } }

    //causes messy stuff when destroying outside of scenes
    /*~SaveFileManager()
    {
        CloseSaveFile();
    }*/

    /// <summary>
    /// Initialises a SaveFileManager for an already existing save file.
    /// </summary>
    public SaveFileManager(string safeName)
    {
        this.saveFilePath = worldsDataPath+"\\" + safeName + WORLDFILEEXTENSION;

        OpenOrCreateZipFile();
    }


    public static SaveFileManager Create(string worldName, int seed)
    {
        //char[] disallowedChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

        string safeName = Statics.MakeStringFileSafe(worldName);

        SaveFileManager save = new SaveFileManager(safeName);
        save.OpenOrCreateZipFile();

        save.worldState = new SaveFileObjects.SaveFileWorldState
        {
            worldName = worldName,
            seed = seed,
            worldTime = 0,
            lastModified = (long)(System.DateTime.Now.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds,
        };

        //save.SaveFileInZip(JsonMapper.ToJson(save.worldState), "worldData"); //using this instead of WriteWorldData() so worldTime is forced to 0.
        save.SaveJSONFileInZip<SaveFileObjects.SaveFileWorldState>(save.worldState, "worldData");

        SaveFileObjects.SaveFilePlayerState playerState = new SaveFileObjects.SaveFilePlayerState
        {
            name = "player",
            posY = 1000,
            camZoom = -30,
            collectedColors = startingColors,
            //collectedColors = new byte[] {10,15,20,255},
            //test = new string[][] {new string[] { "hello", "world"},new string[] {"foo", "bar"} }
        };

        save.SavePlayerData(playerState);

        save.zf.AddDirectoryByName("models/customObjects");
        save.zf.Save();
        //save.ApplyPendingEdits();

        return save;
    }

    public void CloseSaveFile()
    {
        WriteWorldData();
        zf.Save();
        zf.Dispose();
        //if(current==this)current = null;
    }

    public List<string> GetFilesInFolder(string folderName)
    {
        try
        {
            var files = zf.SelectEntries("*", folderName);
            List<string> names = new List<string>();

            int i = 0;
            foreach (ZipEntry zE in files)
            {
                if(!zE.IsDirectory)
                    names.Add(zE.FileName);
                i++;
            }
            return names;
        }
        catch
        {
            return null;
        }
    }

    #region SPECIFICS

    public void SaveVoxelModel(VoxelModel model, string name="")
    {
        string path = name == "" ? model.modelName : name;
        SaveFileInZip(model.ExportToSBVOX().ToArray(), "models/" + path);
    }

    public VoxelModel OpenVoxelModel(string name)
    {
        byte[] data = LoadDataFileFromZip("models/" + name);
        if (data == null) return null;
        MemoryStream m = new MemoryStream(data);
        return VoxelModel.ReadSBVOXBinary(new BinaryReader(m), name);
    }

    public void WriteWorldData()
    {
        worldState.worldTime = Time.time;
        worldState.lastModified = (long)(System.DateTime.Now.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;



        SaveJSONFileInZip<SaveFileObjects.SaveFileWorldState>(worldState, "worldData");
        //SaveFileInZip(JsonMapper.ToJson(worldState), );
    }

    public bool SaveWorldScreenshot(Texture2D screenshot)
    {
        try
        {
            SaveFileInZip(screenshot.EncodeToJPG(), "screenshot");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public SaveFileObjects.SaveFileChunkState LoadChunkData(int x, int y)
    {
        return LoadJSONFileInZip<SaveFileObjects.SaveFileChunkState>("chunks/" + x + "," + y);
    }

    public bool SaveChunkData(SaveFileObjects.SaveFileChunkState state, int x, int y)
    {
        return SaveJSONFileInZip<SaveFileObjects.SaveFileChunkState>(state, "chunks/"+x+","+y);
    }

    private SaveFileObjects.SaveFileWorldState LoadWorldData()
    {
        return worldState = LoadJSONFileInZip<SaveFileObjects.SaveFileWorldState>("worldData");
    }

    public SaveFileObjects.SaveFilePlayerState LoadPlayerData()
    {
        return LoadJSONFileInZip<SaveFileObjects.SaveFilePlayerState>("players/player");
    }

    public bool SavePlayerData(SaveFileObjects.SaveFilePlayerState state)
    {
        return SaveJSONFileInZip<SaveFileObjects.SaveFilePlayerState>(state, "players/player");
    }

    #endregion

    #region GENERAL IO

    ZipFile OpenOrCreateZipFile()
    {
        if (!File.Exists(saveFilePath))
        {
            using(FileStream fs = File.Create(saveFilePath))
            {
                fs.Write(new byte[] { 0x50, 0x4b, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 22);
            }

            SaveFileInZip(VERSIONNUMBER, "VE_VERSION"); //this also opens the zip file since this function gets called from within SaveFileInZip .-.
            //ApplyPendingEdits();
            return _zf;
        }   
        _zf = new ZipFile(saveFilePath);
        _zf.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
        _zf.CompressionMethod = CompressionMethod.None;
        return _zf;
    }

    public bool SaveFileInZip(byte[] data, string fileName)
    {
        try
        {
            if (zf.ContainsEntry(fileName))
            {
                zf.RemoveEntry(fileName);
                zf.Save();
            }
            zf.AddEntry(fileName, data);
            zf.Save();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            return false;
        }
    }

    public bool SaveFileInZip(string data, string fileName)
    {
        try
        {
            if (zf.ContainsEntry(fileName))
            {
                zf.RemoveEntry(fileName);
                zf.Save();
            }
            
            zf.AddEntry(fileName, data);
            zf.Save();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            return false;
        }
    }

    /*public void ApplyPendingEdits()
    {
        zf.Save();
    }*/

    public string LoadStringFileFromZip(string filename)
    {
        try
        {
            if (zf.ContainsEntry(filename))
            {
                foreach (ZipEntry entry in zf.Entries)
                {
                    if (entry.FileName == filename)
                    {
                        Stream s = entry.OpenReader();

                        StreamReader sr = new StreamReader(s);
                        string file = sr.ReadToEnd();
                        sr.Close();
                        s.Close();
                        return file;
                    }
                }
            }
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            return null;
        }
    }
    public byte[] LoadDataFileFromZip(string filename)
    {
        try
        {
            if (zf.ContainsEntry(filename))
            {
                foreach (ZipEntry entry in zf.Entries)
                {
                    if (entry.FileName == filename)
                    {
                        Stream s = entry.OpenReader();
                        byte[] data = new byte[s.Length];
                        s.Read(data, 0, (int)s.Length);
                        s.Close();
                        return data;

                    }
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region JSON IO

    bool SaveJSONFileInZip<T>(T data, string fileName)
    {
        return SaveFileInZip(JsonConvert.SerializeObject(data), fileName);

        //NOTE:this one's messy. Again, using streams instead of strings but maybe a bit OTT here?
        /*try
        {
            if (zf.ContainsEntry(fileName))
            {
                zf.RemoveEntry(fileName);
                zf.Save();
            }
            JsonSerializer jsonSerializer = new JsonSerializer();
            MemoryStream ms = new MemoryStream();
            jsonSerializer.Serialize(new StreamWriter(ms), data);
            ms.Seek(0, SeekOrigin.Begin);
            zf.AddEntry(fileName, ms);
            zf.Save();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            return false;
        }*/
    }

    T LoadJSONFileInZip<T>(string filename)
    {
        string jsonData = LoadStringFileFromZip(filename);
        if (jsonData != null) return JsonConvert.DeserializeObject<T>(jsonData);
        return default(T);

        //NOTE: I'm using a slightly modified version of LoadStringFileFromZip so I can use streams instead of strings when converting JSON.
        /*try
        {
            if (zf.ContainsEntry(filename))
            {
                foreach (ZipEntry entry in zf.Entries)
                {
                    if (entry.FileName == filename)
                    {
                        Stream s = entry.OpenReader();

                        StreamReader sr = new StreamReader(s);
                        //string file = sr.ReadToEnd();
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        T obj = jsonSerializer.Deserialize<T>(new JsonTextReader(sr));
                        sr.Close();
                        s.Close();
                        return obj;
                    }
                }
            }
            return default(T);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            return default(T);
        }*/
    }

    #endregion
}
