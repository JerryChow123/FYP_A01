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

	private void Start()
	{
		button = GetComponent<Button>();
		panel = GameObject.Find("ReadWriteEnabledImageToDrawOn");

		if (button != null)
			button.onClick.AddListener(ButtonOnClick);
	}

	IEnumerator SendPng(string pngPath)
	{
		WWWForm form = new WWWForm();
		byte[] myData = System.IO.File.ReadAllBytes(pngPath);
		form.AddBinaryData("file", myData, "test", "");
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
			Debug.Log(data);
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

			GameObject.Find("Player").GetComponent<FirstPersonController>().enabled = true;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
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
				var path = @"C:\_Test\test.png";
				File.WriteAllBytes(path, bytes);
				StartCoroutine(SendPng(path));

				break;

			case btn_listen:
				break;
		}
	}
}
