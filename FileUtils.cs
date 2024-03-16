using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FileUtils 
{
   
    public static string GetTextFromStreamAssets(string path)
    {
        string localpath = "";

        if (Application.platform == RuntimePlatform.Android)
        {
            localpath = Application.streamingAssetsPath + "/" + path;
        }
        else
        {
            localpath = "file:///" + Application.streamingAssetsPath + "/" + path;
        }

        UnityWebRequest request = UnityWebRequest.Get(localpath);

        var ops = request.SendWebRequest();

        while (!ops.isDone)
        {

        }

        if(request.isNetworkError || request.isHttpError)
        {
            return "";
        }
        else
        {
            return request.downloadHandler.text;
        }


    }

}
