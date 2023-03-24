using FreeDraw;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DrawBox : MonoBehaviour
{
	const int btn_reset = 0;
	const int btn_submit = 1;
	const int btn_listen = 2;

	public int button_type = 0;

	Button button;
	GameObject panel;
	public static string text;

	private void Start()
	{
		button = GetComponent<Button>();
		panel = GameObject.Find("ReadWriteEnabledImageToDrawOn");

		if (button != null)
			button.onClick.AddListener(ButtonOnClick);
	}

	IEnumerator SendPng(string pngPath, byte[] byteData=null)
	{
		WWWForm form = new WWWForm();
		if (byteData != null)
		{
			form.AddBinaryData("file", byteData, "test", "");
		}
		else
		{
			byte[] myData = System.IO.File.ReadAllBytes(pngPath);
			form.AddBinaryData("file", myData, "test", "");
		}
		//var url = "http://192.168.0.135:5000/";
		var url = "https://asia-east2-industrial-silo-356001.cloudfunctions.net/visionai";
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
			var start = data.IndexOf('\"');
			var end = data.IndexOf('\"', start + 1);
			//var result = (data == null) ? data : data.Split('b')[0].Replace("\"", "").Trim();
			//Debug.Log(start + " to " + end);
			var result = data.Substring(start + 1, end - start - 1);
			Debug.Log(result);
			Debug.Log("Correct : " + text);
			//panel.transform.Find("SayText").GetComponent<Text>().text = data.Replace("Transcript: ", "");

			// ªð¦^
			Scene s = SceneManager.GetSceneByName("DrawBox");
			GameObject[] objs = s.GetRootGameObjects();
			foreach (GameObject obj in objs)
				obj.SetActive(false);

			s = SceneManager.GetSceneByName("EscapeGame");
			objs = s.GetRootGameObjects();
			foreach (GameObject obj in objs)
			{
				if (!obj.name.EndsWith("Panel"))
					obj.SetActive(true);
			}

			var player = GameObject.Find("Player").GetComponent<FirstPersonController>();
			player.enabled = true;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			
			bool bCorrect = (result == text);
			player.CheckAnswerResult(bCorrect);
		}
	}

	void ButtonOnClick()
	{
		Drawable draw = panel.GetComponent<Drawable>();

		switch (button_type)
		{
			case btn_reset:
				draw.ResetCanvas();
				break;

			case btn_submit:
				Texture2D photo = draw.GetComponentsInParent<SpriteRenderer>()[0].sprite.texture;
				//Debug.Log(photo);
				byte[] bytes = photo.EncodeToPNG();
				//var path = @"C:\_Test\test.png";
				//File.WriteAllBytes(path, bytes);
				StartCoroutine(SendPng(null, bytes));

				break;

			case btn_listen:
				GetComponent<AudioSource>().Play();
				break;
		}
	}
}
