using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JSONParser{
    public static void SaveToJSON(object objToSaved, string fileName)
    {
        string fileText;

        fileText = JsonUtility.ToJson(objToSaved, true);
        // JSON.net doesn't support Vector3 and Color
        //fileText = JsonConvert.SerializeObject(obj, Formatting.Indented); 
        File.WriteAllText(Application.dataPath + "/JSON/" + fileName + ".json", fileText);
        Debug.Log("(" + fileName + ") JSON file is saved!");
    }

    public static T LoadFromJSON<T>(string fileName)
    {
        //Debug.Log("(" + fileName + ") JSON file is loaded!");
        TextAsset path = (TextAsset)Resources.Load(fileName, typeof(TextAsset));
        
        //string fileText = File.ReadAllText(Application.streamingAssetsPath + "/" + fileName + ".json");
        //string fileText = File.ReadAllText(path.text);
        //string fileText = File.ReadAllText(Application.dataPath + "/JSON/" + fileName + ".json");
        Debug.Log("(" + fileName + ") JSON file is loaded!");

        return JsonConvert.DeserializeObject<T>(path.text);
    }  
}
