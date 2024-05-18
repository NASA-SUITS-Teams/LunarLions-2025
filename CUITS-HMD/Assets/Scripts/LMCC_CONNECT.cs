using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Unity.Profiling;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Newtonsoft.Json;


using Unity.VisualScripting;
//using static TSS_Serialized.UIAData;
using System;
using System.Xml;

using TMPro;

public class LMCC_CONNECT : MonoBehaviour
{

    public class Step
    {
        public string step { get; set; }
        public string role { get; set; }
        public string description { get; set; }
    }

    public class ProcObject
    {
        public int id { get; set; }
        public string title { get; set; }
        public List<Step> steps { get; set; }
    }

    public class MessObject
    {

    }

    public class Configuration
    {
    public string TSS_IP { get; set; }
    public string MAPBOX_KEY { get; set; }
    public string HOLO_IP { get; set; }
    public string SERVER_IP { get; set; }
    }

    // Connection
    string host;
    string port;
    string url;
    int    team_number;
    public bool   connected;
    float  time_since_last_update;

    // Database Jsons
    bool ProcUpdated;
    string ProcJsonString;
    bool MessUpdated;
    string MessJsonString;
    bool ConfigUpdated;
    string ConfigJsonString;

    // Procedure and Message Objects 
    public ProcObject GoldenProcedure;
    public MessObject GoldenMessage;
    public Configuration the_config;

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    this.connected = true;
                    break;
            }

        }
    }

    // Connect to TSSc with a specific team number
    public void ConnectToHost(string host)
    {
        this.host = host;
        this.port = "8000";
        this.url = "http://" + this.host + ":" + this.port;
        Debug.Log(this.url);

        // Test Connection
        StartCoroutine(GetRequest(this.url));
    }

    public void DisconnectFromHost()
    {
        this.connected = false;
    }

   ///////////////////////////////////////////// Procedures

    IEnumerator GetProcState()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(this.url + "/get_sent_procedure"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    if (this.ProcJsonString != webRequest.downloadHandler.text)
                    {
                        this.ProcUpdated = true;
                        this.ProcJsonString = webRequest.downloadHandler.text;
                    }
                    break;
            }
        }
    }

    public string GetProcJsonString()
    {
        ProcUpdated = false;
        return this.ProcJsonString;
    }

    public bool isProcUpdated()
    {
        return ProcUpdated;
    }

    ///////////////////////////////////////////// Messages

    IEnumerator GetMessState()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(this.url + "/get_message"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    if (this.MessJsonString != webRequest.downloadHandler.text)
                    {
                        this.MessUpdated = true;
                        this.MessJsonString = webRequest.downloadHandler.text;
                    }
                    break;
            }

        }
    }

    public string GetMessJsonString()
    {
        MessUpdated = false;
        return this.MessJsonString;
    }

    public bool isMessUpdated()
    {
        return MessUpdated;
    }
    
    ///////////////////////////////////////////// Config

    IEnumerator GetConfigState()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(this.url + "/get_config"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    if (this.ConfigJsonString != webRequest.downloadHandler.text)
                    {
                        this.ConfigUpdated = true;
                        this.ConfigJsonString = webRequest.downloadHandler.text;
                    }
                    break;
            }

        }
    }

    public string GetConfigJsonString()
    {
        ConfigUpdated = false;
        return this.ConfigJsonString;
    }

    public bool isConfigUpdated()
    {
        return ConfigUpdated;
    }
    
    ///////////////////////////////////////////// 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
     // If you are connected to TSSc
        if (this.connected)
        {
            // Each Second
            time_since_last_update += Time.deltaTime;
            if (time_since_last_update > 1.0f)
            {
                // Pull TSSc Updates
                StartCoroutine(GetProcState());
                StartCoroutine(GetMessState()); 
                StartCoroutine(GetConfigState()); 
                time_since_last_update = 0.0f;
            }
        }


        // Check if the UIA data has been updated
        if (isProcUpdated())
        {
            Debug.Log("Procedure Updated");
            string ProcJsonString = GetProcJsonString();
            GoldenProcedure = JsonConvert.DeserializeObject<ProcObject>(ProcJsonString);
        }

        // Check if the DCU data has been updated
        if (isMessUpdated())
        {
            Debug.Log("Message Updated");
            string MessJsonString = GetMessJsonString();
            GoldenMessage = JsonConvert.DeserializeObject<MessObject>(MessJsonString);
        }

        // Check if the DCU data has been updated
        if (isConfigUpdated())
        {
            Debug.Log("Configuration Updated");
            string ConfigJsonString = GetConfigJsonString();
            the_config = JsonConvert.DeserializeObject<Configuration>(ConfigJsonString);
        }

    }

}