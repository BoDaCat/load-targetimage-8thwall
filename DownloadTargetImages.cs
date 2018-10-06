using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using SimpleJSON;
using System;

public class DownloadTargetImages : MonoBehaviour {

    private XRController xr_;
    public InputField ID;
    string IDInput;
    string URL;
    private List<string> Targets;
    private List<Texture2D> TI;
    private List<XRDetectionTexture> detectionTextures;
    public List<XRDetectionTexture> texHandler;
    public GameObject DetectionController;



    // Use this for initialization
    void Start () {

        URL = ""; // NEEDS URL
        List<XRDetectionTexture> detectionTextures = DetectionController.GetComponent<XRImageDetectionController>().detectionTextures;
    }


    // User submits a string
    public void OnSubmit()
    {
        IDInput = ID.text;
        Debug.Log(IDInput);
        StartCoroutine(SubmitID(IDInput));

    }

    // Parse downloaded JSON text file that has URLs to all target images
    // Start Coroutine to download each image
    public void Parse(string text)
    {
        Debug.Log(text);
        Debug.Log("Parse");
        var metadata = JSON.Parse(text);
        Debug.Log(metadata[0]);
        Targets = new List<string>(metadata.Count);
        TI = new List<Texture2D>(metadata.Count);
        Debug.Log(metadata.Count);
        Debug.Log(Targets.Capacity);
        int i = 0;
        for (i = 0; i < Targets.Capacity; i++)
        {
            Targets.Add(metadata[i]);
            Debug.Log(metadata[i]);
        }

        foreach (string target in Targets)
        {
            StartCoroutine(TargetDL(target));
        }
    }

    // Save each target image into XRDetectionTexture list
    public void SaveImage(XRDetectionTexture tex)
    {
        Debug.Log("Saving Image");
        texHandler = new List<XRDetectionTexture>();
        int x = 0;
        texHandler.Capacity = Targets.Capacity;
        Debug.Log(texHandler.Capacity);
        for (x = 0; x < texHandler.Capacity; x++)
        {
            texHandler.Insert(x, tex);
        }

        if (texHandler.Count == TI.Capacity)
        {
            detectionTextures = new List<XRDetectionTexture>(texHandler.Count);


            int i = 0;
            for (i = 0; i < detectionTextures.Capacity; i++)
            {
                detectionTextures.Add(texHandler[i]);
                Debug.Log(texHandler[i]);
            }


            xr_ = GameObject.FindWithTag("XRController").GetComponent<XRController>();

            if (detectionTextures.Count > 0)
            {
                Dictionary<string, XRDetectionImage> detectionImages =
                  new Dictionary<string, XRDetectionImage>();
                foreach (XRDetectionTexture detectionTexture in detectionTextures)
                {
                    detectionImages.Add(
                      detectionTexture.tex.name, XRDetectionImage.FromDetectionTexture(detectionTexture));
                }
                xr_.SetDetectionImages(detectionImages);
                Debug.Log("DetectionImages Count is: " + detectionImages.Count);

            }
        }
        Debug.Log("Saved");
        Debug.Log(texHandler[0]);
        Debug.Log(texHandler[0].tex.name);
    }

    // Downlaods each target image
    IEnumerator TargetDL(string target)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL + IDInput + "/" + target);
        yield return www.SendWebRequest();

        if (www.isDone)
        {
            
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Debug.Log(texture);
            texture.name = target;
            Debug.Log(texture.name);
            XRDetectionTexture XRDI = new XRDetectionTexture(texture, .15f);
            SaveImage(XRDI);
        }
        www.Dispose();
    }

    // Downlaods JSON text file
    IEnumerator SubmitID(string IDInput)
    {
        WWW www = new WWW(URL+IDInput+"/"+IDInput+".txt");
        while (!www.isDone)
            yield return null;
        Debug.Log(www);

        if (www.isDone)
        {
            Parse(www.text);
            Debug.Log("Calling Parse");
        }
        
        www.Dispose();
    }

}
