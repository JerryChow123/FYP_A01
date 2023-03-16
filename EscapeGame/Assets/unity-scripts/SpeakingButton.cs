using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SpeakingButton : MonoBehaviour
{
    public Button button;
    GameObject panel;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        panel = transform.parent.gameObject;

        if (button != null)
            button.onClick.AddListener(ButtonOnClick);
    }

    IEnumerator SendVoiceRecord(string wavPath)
    {
        WWWForm form = new WWWForm();
        byte[] myData = System.IO.File.ReadAllBytes(wavPath);
        form.AddBinaryData("file", myData, "test", "");
        //var url = "http://192.168.0.135:5000/";
        var url = "https://asia-east2-industrial-silo-356001.cloudfunctions.net/speech";
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Upload complete!");
            var data = www.downloadHandler.text;
            Debug.Log(data);
			panel.transform.Find("SayText").GetComponent<Text>().text = data.Replace("Transcript: ", "");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            panel.SetActive(false);
            GameObject.Find("Player").GetComponent<FirstPersonController>().enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void ButtonOnClick()
    {
        var recorder = GameObject.Find("VoiceRecorder").GetComponent<VoiceRecorder>();

        if (Microphone.IsRecording(null))
        {
            recorder.Stop();
            recorder.fileName = @"C:\_Test\test.wav";
            recorder.Save();
            StartCoroutine(SendVoiceRecord(recorder.fileName));
            GetComponentInChildren<Text>().text = "Record";
        }
        else
        {
            recorder.Begin();
            GetComponentInChildren<Text>().text = "Stop";
        }

        Debug.Log(recorder.Infotxt);
    }
}