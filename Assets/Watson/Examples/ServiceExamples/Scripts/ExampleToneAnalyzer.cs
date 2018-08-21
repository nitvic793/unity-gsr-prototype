
using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.ToneAnalyzer.v3;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;
using System.Collections;
using IBM.Watson.DeveloperCloud.Connection;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;

public class ExampleToneAnalyzer : MonoBehaviour
{
    private string _username = "e8712738-75d7-462a-876a-ec201856d96b";
    private string _password = "4wIogtoYkZIK";
    private string _url = "https://gateway.watsonplatform.net/tone-analyzer/api";

    private GSRReader gsrReader;
    private ToneAnalyzer _toneAnalyzer;
    private string _toneAnalyzerVersionDate = "2017-05-26";

    private string _stringToTestTone = "This service enables people to discover and understand, and revise the impact of tone in their content. It uses linguistic analysis to detect and interpret emotional, social, and language cues found in text.";
    private bool _analyzeToneTested = false;

    public string dominantTone = "";
    public InputField field;
    public Text text;
    public Light dirLight;
    public Dictionary<string, float> emotionMap = null;
    public string mode = "video";

    void Start()
    {
        gsrReader = GetComponent<GSRReader>();
        LogSystem.InstallDefaultReactors();

        //  Create credential and instantiate service
        Credentials credentials = new Credentials(_username, _password, _url);

        _toneAnalyzer = new ToneAnalyzer(credentials);
        _toneAnalyzer.VersionDate = _toneAnalyzerVersionDate;

        Runnable.Run(Examples());
        StartCoroutine(GetEmotionData());
        try
        {
            gsrReader.Run();
        }
        catch (Exception e)
        {

        }
    }

    public IEnumerator GetEmotionData()
    {
        
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/get_emotions"))
            {
                yield return www.Send();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    emotionMap = JsonConvert.DeserializeObject<Dictionary<string, float>>(www.downloadHandler.text);
                    CalculateDominantTone();
                }
            }

            yield return new WaitForSeconds(1F);
        }

    }

    void CalculateDominantTone()
    {
        dominantTone = "";
        float maxValue = 0F;
        foreach (var item in emotionMap)
        {
            if (item.Key == "neutral") continue;
            if (item.Value > maxValue)
            {
                dominantTone = item.Key;
                maxValue = item.Value;
            }
        }
        
    }

    public void OnEditFinish()
    {
        Debug.Log("OnEditFinish()");
        if (!_toneAnalyzer.GetToneAnalyze(OnGetToneAnalyze, OnFail, field.text))
            Log.Debug("ExampleToneAnalyzer.OnEditFinish()", "Failed to analyze!");
    }

    private IEnumerator Examples()
    {
        //  Analyze tone
        if (!_toneAnalyzer.GetToneAnalyze(OnGetToneAnalyze, OnFail, _stringToTestTone))
            Log.Debug("ExampleToneAnalyzer.Examples()", "Failed to analyze!");

        while (!_analyzeToneTested)
            yield return null;

        Log.Debug("ExampleToneAnalyzer.Examples()", "Tone analyzer examples complete.");
    }

    private void OnGetToneAnalyze(ToneAnalyzerResponse resp, Dictionary<string, object> customData)
    {
        //Log.Debug("ExampleToneAnalyzer.OnGetToneAnalyze()", "{0}", customData["json"].ToString());
        //Debug.Log(resp.document_tone.tone_categories[0]);
        double maxToneScore = 0;
        dominantTone = "";
        foreach (var item in resp.document_tone.tone_categories)
        {
            if (item.category_id == "emotion_tone")
            {
                foreach (var tone in item.tones)
                {
                    Debug.Log(tone);
                    if (tone.score > maxToneScore)
                    {
                        maxToneScore = tone.score;
                        dominantTone = tone.tone_id;
                    }
                }
            }
        }
        text.text = dominantTone;
        _analyzeToneTested = true;
    }

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleRetrieveAndRank.OnFail()", "Error received: {0}", error.ToString());
    }

    private void Update()
    {
        if(mode == "video")
        {
            text.text = dominantTone;
            if (dominantTone == "joy" || dominantTone == "happiness")
            {
                dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(28, 152.975F, 90F), 0.1F);
                dirLight.color = Color.Lerp(dirLight.color, new Color(246F / 255F, 203F / 255F, 110F / 255F, 1F), 0.1F);
                dirLight.intensity = Mathf.Lerp(dirLight.intensity, 2.31F, 0.1F);
            }
            else if (dominantTone == "sadness")
            {
                dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(4, 152.975F, 90F), 0.1F);
                dirLight.color = Color.Lerp(dirLight.color, new Color(89F / 255F, 75F / 255F, 1, 1), 0.1F);
                dirLight.intensity = Mathf.Lerp(dirLight.intensity, 2.31F, 0.1F);
            }
            else if (dominantTone == "anger")
            {
                dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(20, 152.975F, 90F), 0.1F);
                dirLight.color = Color.Lerp(dirLight.color, new Color(230F / 255F, 70F / 255F, 70F / 255F, 1F), 0.1F);
                dirLight.intensity = Mathf.Lerp(dirLight.intensity, 2.31F, 0.1F);
            }
            else if (dominantTone == "disgust")
            {
                dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(12F, 152.975F, 90F), 0.1F);
                dirLight.color = Color.Lerp(dirLight.color, new Color(165F / 255F, 42F / 190F, 42F / 255F, 1F), 0.1F);
                dirLight.intensity = Mathf.Lerp(dirLight.intensity, 1.7F, 0.1F);
            }
            else if (dominantTone == "fear")
            {
                dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(1F, 152.975F, 90F), 0.1F);
                dirLight.color = Color.Lerp(dirLight.color, new Color(100F / 255F, 42F / 190F, 240F / 255F, 1F), 0.1F);
                dirLight.intensity = Mathf.Lerp(dirLight.intensity, 0.2F, 0.1F);
            }
            else if (dominantTone == "surprise")
            {
                dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(24, 152.975F, 90F), 0.1F);
                dirLight.color = Color.Lerp(dirLight.color, new Color(100F / 255F, 255F / 255F, 110F / 255F, 1F), 0.1F);
                dirLight.intensity = Mathf.Lerp(dirLight.intensity, 1.31F, 0.1F);
            }
            else
            {
                dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(16, 152.975F, 90F), 0.1F);
                dirLight.color = Color.Lerp(dirLight.color, new Color(200F / 255F, 195F / 255F, 200F / 255F, 1F), 0.1F);
                dirLight.intensity = Mathf.Lerp(dirLight.intensity, 3.31F, 0.1F);
            }
        }
        else
        {
            if (gsrReader.IsReady)
            {
                text.text = gsrReader.Output + " " + gsrReader.Average;
                if (gsrReader.Output == "down")
                {
                    dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(28, 152.975F, 90F), 0.01F);
                    dirLight.color = Color.Lerp(dirLight.color, new Color(246F / 255F, 203F / 255F, 110F / 255F, 1F), 0.1F);
                    dirLight.intensity = Mathf.Lerp(dirLight.intensity, 2.31F, 0.1F);
                }
                else
                {
                    dirLight.transform.rotation = Quaternion.Lerp(dirLight.transform.rotation, Quaternion.Euler(4, 152.975F, 90F), 0.01F);
                    dirLight.color = Color.Lerp(dirLight.color, new Color(89F / 255F, 75F / 255F, 1, 1), 0.1F);
                    dirLight.intensity = Mathf.Lerp(dirLight.intensity, 2.31F, 0.1F);
                }
            }
        }
       
        //field.ActivateInputField();
    }

    void OnApplicationQuit()
    {
        Debug.Log("Done");
        //gsrReader.Stop();
    }
}
