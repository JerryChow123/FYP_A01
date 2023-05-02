using System;
using System.Collections;
using System.Collections.Generic;
using FreeDraw;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;


#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
			//SceneManager.LoadSceneAsync("DrawBox", LoadSceneMode.Additive);
		}

		public class Question
		{
			public string question;
			public string optionA;
			public string optionB;
			public string optionC;
			public string optionD;
			public string answer;
		}

		public class Dictation { public string text; }
		public class Sentence { public string text; }

		public static List<Question> all_questions;
		public static List<Dictation> all_dictations;
		public static List<Sentence> all_sentences;

		public static string username, password;

		SubtitleManager SubTitleRequest, SubTitleResponse;
		VoiceRecorder recorder;
		float recorder_nexttime;
		Animator current_box_anim;

		public static void AddData()
		{
			string data;
			try
			{
				data = LoginPage.data;
			}
			catch (Exception) 
			{
				data = null;
			}
			if (data == null)
				data = "{\"dictation\":{\"-NUAsF_fQkZM-5vpW1v2\":{\"text\":\"agenda\"},\"-NUAsIP3INaTnSJKpkT5\":{\"text\":\"acknowledge\"},\"-NUAsL1rWuA04p_NOEj6\":{\"text\":\"bonus\"},\"-NUAsMi1q1Zd546AIxdr\":{\"text\":\"browse\"},\"-NUAsOZMjZk8bYBguJPJ\":{\"text\":\"chairman\"},\"-NUAsQv5edg-Azlj_pCf\":{\"text\":\"despair\"},\"-NUAsTEAW3B02idjCrir\":{\"text\":\"destination\"},\"-NUAsVD-7mLT58mF2n9M\":{\"text\":\"execute\"},\"-NUAsXZtodWHWqQNURGR\":{\"text\":\"external\"},\"-NUAsZjZfDZ6XHK1aVVm\":{\"text\":\"insure\"}},\"marks\":null,\"questions\":{\"-NSymGqdmaG6xbHPQWM6\":{\"answer\":\"2\",\"optionA\":\"1\",\"optionB\":\"2\",\"optionC\":\"3\",\"optionD\":\"4\",\"question\":\"1 add 1 is ?\"},\"-NUI46rthdxZzxUaIvKh\":{\"answer\":\"4\",\"optionA\":\"4\",\"optionB\":\"6\",\"optionC\":\"8\",\"optionD\":\"5\",\"question\":\"2 Multiplication 2 is ?\"},\"-NUI6Ch-ocHiOL5jBAdq\":{\"answer\":\"9\",\"optionA\":\"6\",\"optionB\":\"3\",\"optionC\":\"8\",\"optionD\":\"9\",\"question\":\"3 Division 3 is ?\"},\"-NUIJBxkbqshfkOJw6sx\":{\"answer\":\"-1\",\"optionA\":\"4\",\"optionB\":\"5\",\"optionC\":\"1\",\"optionD\":\"-1\",\"question\":\"4 Subtraction 5 is ?\"}},\"sentence\":{\"-NUAsdZw9s85OQxkao7_\":{\"text\":\"Action speak louder than words.\"},\"-NUAsfIBBBCXpGWdsGhO\":{\"text\":\"Wasting time is robbing oneself.\"},\"-NUAsgozhuzDNEZMFzMP\":{\"text\":\"Never say die.\"},\"-NUAsirdFDJCfrR5MeI8\":{\"text\":\"Keep on going never give up.\"},\"-NUAskeWS3j1W1l0adhb\":{\"text\":\"Never put off what you can do today until tomorrow.\"},\"-NUAspIyBMLiMBj7B_ih\":{\"text\":\"Believe in yourself.\"},\"-NUAsqssZOL-NK2mBLPI\":{\"text\":\"You think you can, you can.\"},\"-NUAssR8A_iO9rgc8ZJr\":{\"text\":\"I can because i think i can.\"},\"-NUAsttsxaKEJHW7fLoA\":{\"text\":\"Winners do what losers don't want to do.\"},\"-NUAswQTZRiiHizT-tSI\":{\"text\":\"Jack of all trades and master of none.\"}},\"success\":true}";
			var resp = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

			var questions_json = JsonConvert.SerializeObject(resp["questions"]);
			var questions = JsonConvert.DeserializeObject<Dictionary<string, object>>(questions_json);
			foreach (var i in questions)
			{
				var j_data = JsonConvert.DeserializeObject<Question>(i.Value.ToString());
				//Debug.Log(j_data);
				all_questions.Add(j_data);
			}

			var dictations_json = JsonConvert.SerializeObject(resp["dictation"]);
			var dictations = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictations_json);
			foreach (var i in dictations)
			{
				var j_data = JsonConvert.DeserializeObject<Dictation>(i.Value.ToString());
				//Debug.Log(j_data);
				all_dictations.Add(j_data);
			}

			var sentences_json = JsonConvert.SerializeObject(resp["sentence"]);
			var sentences = JsonConvert.DeserializeObject<Dictionary<string, object>>(sentences_json);
			foreach (var i in sentences)
			{
				var j_data = JsonConvert.DeserializeObject<Sentence>(i.Value.ToString());
				//Debug.Log(j_data);
				all_sentences.Add(j_data);
			}
		}

		IEnumerator AccessDialogflow(byte[] audio_bytes)
		{
			WWWForm form = new WWWForm();
			form.AddBinaryData("file", audio_bytes, "test", "");
			form.AddField("username", "teacher");
			form.AddField("password", "teacher");
			//var url = "http://127.0.0.1:5000/";
			//var url = "https://asia-east2-industrial-silo-356001.cloudfunctions.net/dialogflowbot";
			var url = LoginPage.DIALOGFLOW_URL;
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
				Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
				if (dict["content"].Length > 0)
				{
					var audio = Convert.FromBase64String(dict["content"]);
					Debug.Log("==================== SIZE -> " + audio.Length);
					float seconds = (float)audio.Length / 40000f;
					Debug.Log("==================== Seconds -> " + seconds.ToString());
					recorder_nexttime = Time.fixedTime + seconds;
					SubTitleRequest.DisplaySubtitle(dict["query_text"], seconds);
					SubTitleResponse.DisplaySubtitle(dict["response_text"], seconds);
					var audio_player = GameObject.Find("Muryotaisu").GetComponent<AudioSource>();
					audio_player.clip = ConvertBytesToClip(audio);
					audio_player.Play();
				}
			}
		}

		IEnumerator TextToSpeech(string text)
		{
			WWWForm form = new WWWForm();
			form.AddField("username", "teacher");
			form.AddField("password", "teacher");
			form.AddField("text", text);
			//var url = "http://127.0.0.1:5000/";
			//var url = "https://asia-east2-industrial-silo-356001.cloudfunctions.net/text_speech";
			var url = LoginPage.TEXT_SPEECH_URL;
			UnityWebRequest www = UnityWebRequest.Post(url, form);
			yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(www.error);
			}
			else
			{
				Debug.Log("Send POST complete!");
				var audio = www.downloadHandler.data;
				var objects = SceneManager.GetSceneByName("DrawBox").GetRootGameObjects();
				GameObject button = null;
				foreach (var obj in objects)
				{
					//Debug.Log(obj);
					if (obj.name == "Canvas")
					{
						var audio_player = obj.GetComponentInChildren<AudioSource>();
						audio_player.clip = ConvertBytesToClip(audio);
						button = obj.transform.Find("ListenButton").gameObject;
						break;
					}
				}

				Scene s = SceneManager.GetSceneByName("EscapeGame");
				GameObject[] objs = s.GetRootGameObjects();
				foreach (GameObject obj in objs)
					obj.SetActive(false);

				s = SceneManager.GetSceneByName("DrawBox");
				objs = s.GetRootGameObjects();
				foreach (GameObject obj in objs)
				{
					obj.SetActive(true);
					if (obj.name == "Canvas")
						obj.GetComponentInChildren<DrawBox>().ResetCanvas();
				}

				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				this.enabled = false;
				Debug.Log(button);
				if (button != null)
					DrawBox.text = text;
			}
		}

		AudioClip ConvertBytesToClip(byte[] rawData)
		{
			float[] samples = new float[rawData.Length / 2];
			float rescaleFactor = 32767;
			short st = 0;
			float ft = 0;

			for (int i = 0; i < rawData.Length; i+=2)
			{
				st = BitConverter.ToInt16(rawData, i);
				ft = st / rescaleFactor;
				samples[i / 2] = ft;
			}

			AudioClip audioClip = AudioClip.Create("mySound", samples.Length, 1, 24000, false);
			audioClip.SetData(samples, 0);
			return audioClip;
		}

		private void Start()
		{
			// Test
			SubTitleRequest = GameObject.Find("SubTitle").transform.Find("Request").GetComponent<SubtitleManager>();
			SubTitleResponse = GameObject.Find("SubTitle").transform.Find("Response").GetComponent<SubtitleManager>();
			recorder = GameObject.Find("VoiceRecorder").GetComponent<VoiceRecorder>();

			SubTitleRequest.DisplaySubtitle("", 1f);
			SubTitleResponse.DisplaySubtitle("", 1f);
			recorder_nexttime = 0f;

			// MC問題 data
			all_questions = new List<Question>();
			all_sentences = new List<Sentence>();
			all_dictations = new List<Dictation>();
			try
			{
				AddData();
			}
			catch (Exception e)
			{
				Debug.Log(e);
			}

			var doors = GameObject.FindGameObjectsWithTag("TriggerDoor");
			foreach (var door in doors)
			{
				var anim = door.GetComponent<Animator>();
				if (anim != null)
					anim.keepAnimatorControllerStateOnDisable = true;
			}

			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void StartCoroutine(IEnumerable enumerable)
		{
			throw new System.NotImplementedException();
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
			// 人物控制
			KeyPress_Think();
		}

		public void CheckAnswerResult(bool bCorrect, string result=null)
        {
			if (bCorrect)
			{
				//正確
				SubTitleRequest.DisplaySubtitle("正確!!", 2f);
				current_box_anim?.SetBool("opened", true);
			}
			else
            {
				//錯誤
				SubTitleResponse.DisplaySubtitle("錯誤!!", 2f);
				if (result != null)
					SubTitleRequest.DisplaySubtitle(result, 2f);
            }
        }

		void KeyPress_Think()
		{
			if (Time.fixedTime > recorder_nexttime)
			{
				if (Input.GetKey(KeyCode.V))
				{
					if (!Microphone.IsRecording(null))
					{
						SubTitleRequest.DisplaySubtitle("[Recording]", 9999f);
						recorder.Begin();
					}

					return;
				}
				else if (Microphone.IsRecording(null))
				{
					SubTitleRequest.DisplaySubtitle("", 0.1f);
					recorder.Stop();
					StartCoroutine(AccessDialogflow(recorder.data));
					recorder_nexttime = Time.fixedTime + 2f;
				}
			}

			if (Input.GetKeyDown(KeyCode.E))
			{
				Debug.Log("按鍵 E");

				// Bit shift the index of the layer (8) to get a bit mask
				int layerMask = 1 << 8;
				layerMask = ~layerMask;

				RaycastHit hit;
				Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

				if (Physics.Raycast(ray, out hit, 2.5f, layerMask))
				{
					if (hit.transform.tag == "TriggerDoor" || hit.transform.tag == "TriggerBox")
					{
						Debug.Log("觸發門 " + hit.collider.name);
						Animator anim;

						if (hit.transform.tag == "TriggerDoor")
							anim = hit.collider.gameObject.transform.parent.GetComponent<Animator>();
						else
							anim = hit.collider.gameObject.transform.GetComponent<Animator>();

						if (!anim.GetBool("opened"))
						{
							var random = UnityEngine.Random.Range(0, 3);

							if (random == 0 && all_questions.Count > 0)
							{
								Canvas[] panels = Resources.FindObjectsOfTypeAll<Canvas>();
								GameObject panel = null;
								foreach (Canvas p in panels)
								{
									if (p.name == "AnswerPanel")
									{
										panel = p.gameObject;
										break;
									}
								}
								if (panel == null)
								{
									Debug.Log("AnswerPanel not found!");
									return;
								}
								panel.SetActive(true);
								var buttons = panel.GetComponentsInChildren<AnswerButton>();
								int index = new System.Random().Next(all_questions.Count);
								foreach (AnswerButton button in buttons)
								{
									//Debug.Log(button.GetComponentInChildren<Text>().text);
									//Debug.Log(button.transform.gameObject.name);
									switch (button.transform.gameObject.name)
									{
										case "ButtonA":
											button.GetComponentInChildren<Text>().text = all_questions[index].optionA;
											break;

										case "ButtonB":
											button.GetComponentInChildren<Text>().text = all_questions[index].optionB;
											break;

										case "ButtonC":
											button.GetComponentInChildren<Text>().text = all_questions[index].optionC;
											break;

										case "ButtonD":
											button.GetComponentInChildren<Text>().text = all_questions[index].optionD;
											break;
									}
								}
								panel.transform.Find("Text").GetComponent<Text>().text = all_questions[index].question;
								AnswerButton.answer = all_questions[index].answer;
								//Debug.Log("[Answer] " + AnswerButton.answer);
								AnswerButton.door = hit.collider.gameObject;
								Cursor.visible = true;
								Cursor.lockState = CursorLockMode.None;
								this.enabled = false;
							}
							else if (random == 1)
							{
								current_box_anim = hit.collider.gameObject.GetComponent<Animator>();
								if (current_box_anim == null)
									current_box_anim = hit.collider.gameObject.GetComponentInParent<Animator>();
								int index = new System.Random().Next(all_dictations.Count);
								StartCoroutine(TextToSpeech(all_dictations[index].text));
							}
							else if (random == 2)
							{
								current_box_anim = hit.collider.gameObject.GetComponent<Animator>();
								if (current_box_anim == null)
									current_box_anim = hit.collider.gameObject.GetComponentInParent<Animator>();
								Canvas[] panels = Resources.FindObjectsOfTypeAll<Canvas>();
								GameObject panel = null;
								foreach (Canvas p in panels)
								{
									if (p.name == "SpeakingPanel")
									{
										panel = p.gameObject;
										break;
									}
								}
								if (panel == null)
								{
									Debug.Log("SpeakingPanel not found!");
									return;
								}
								var text_obj = panel.GetComponentInChildren<Text>();
								int index = new System.Random().Next(all_sentences.Count);
								text_obj.text = all_sentences[index].text;
								panel.SetActive(true);
								Cursor.visible = true;
								Cursor.lockState = CursorLockMode.None;
								this.enabled = false;
							}
						}
						else
							anim.SetBool("opened", !anim.GetBool("opened"));
					}
				}
			}
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}