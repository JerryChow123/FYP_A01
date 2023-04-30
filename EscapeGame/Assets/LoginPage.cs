using Newtonsoft.Json;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPage : MonoBehaviour
{
    Button login;
    InputField username, password;
    Text result;
	public static string data;
	public static string ac;
	public static string pw;
	
	// Start is called before the first frame update
	void Start()
    {
        login = GetComponentInChildren<Button>();
        login.onClick.AddListener(OnClickLogin);

        username = transform.Find("username").GetComponent<InputField>();
        password = transform.Find("password").GetComponent<InputField>();

        result = transform.Find("result").GetComponent<Text>();
        result.text = "";
    }

    void OnClickLogin()
	{
        var msg = username.text + " + " + password.text;
        Debug.Log(msg);
		result.color = Color.green;
		result.text = "connecting...";
		StartCoroutine(GetQuestions());
	}

	IEnumerator GetQuestions()
	{
		WWWForm form = new WWWForm();
		form.AddField("username", username.text);
		form.AddField("password", password.text);

		//var url = "http://192.168.0.135:5000/";
		var url = "https://asia-east2-industrial-silo-356001.cloudfunctions.net/learning-rpg-game/";
		UnityWebRequest www = UnityWebRequest.Post(url, form);
		//www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
		}
		else
		{
			var data = www.downloadHandler.text;
			Debug.Log(data);

			var resp = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
			if (!Convert.ToBoolean(resp["success"]))
			{
				result.color = Color.red;
				result.text = "wrong username or password!";
			}
			else
			{
				LoginPage.data = data;
				LoginPage.ac = username.text;
				LoginPage.pw = password.text;

				var loginpage = SceneManager.GetSceneByName("LoginPage");
				var objs = loginpage.GetRootGameObjects();
				foreach (GameObject obj in objs)
					obj.SetActive(false);

				var game = SceneManager.GetSceneByName("EscapeGame");
				objs = game.GetRootGameObjects();
				foreach (GameObject obj in objs)
				{
					if (obj.tag != "GamePanel")
						obj.SetActive(true);
				}

				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}
}
