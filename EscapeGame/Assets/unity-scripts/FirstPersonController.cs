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
			SceneManager.LoadSceneAsync("DrawBox", LoadSceneMode.Additive);
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

		public static List<Question> all_questions;

		IEnumerator GetQuestions()
		{
			WWWForm form = new WWWForm();
			form.AddField("username", "teacher");
			form.AddField("password", "teacher");

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
				Debug.Log(resp["success"]);
				Debug.Log(resp["questions"]);
				var json = JsonConvert.SerializeObject(resp["questions"]);
				var questions = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
				foreach (var q in questions)
				{
					var q_data = JsonConvert.DeserializeObject<Question>(q.Value.ToString());
					Debug.Log(q_data);
					all_questions.Add(q_data);
				}
			}
		}

		SubtitleManager SubTitleRequest, SubTitleResponse;
		VoiceRecorder recorder;
		float recorder_nexttime;

		IEnumerator AccessDialogflow(byte[] audio_bytes)
		{
			WWWForm form = new WWWForm();
			form.AddBinaryData("file", audio_bytes, "test", "");
			form.AddField("username", "teacher");
			form.AddField("password", "teacher");
			var url = "http://127.0.0.1:5000/";
			//var url = "https://asia-east2-industrial-silo-356001.cloudfunctions.net/speech";
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
			StartCoroutine(GetQuestions());

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

			if (Input.GetKeyDown(KeyCode.G))
			{
				Scene s = SceneManager.GetSceneByName("EscapeGame");
				GameObject[] objs = s.GetRootGameObjects();
				foreach (GameObject obj in objs)
					obj.SetActive(false);

				s = SceneManager.GetSceneByName("DrawBox");
				objs = s.GetRootGameObjects();
				foreach (GameObject obj in objs)
					obj.SetActive(true);

				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				this.enabled = false;
			}
			else if (Input.GetKeyDown(KeyCode.R))
			{
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
				panel.SetActive(true);
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				this.enabled = false;
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				Debug.Log("按鍵 E");

				// Bit shift the index of the layer (8) to get a bit mask
				int layerMask = 1 << 8;
				layerMask = ~layerMask;

				RaycastHit hit;
				Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

				if (Physics.Raycast(ray, out hit, 2.5f, layerMask))
				{
					if (hit.transform.tag == "TriggerDoor")
					{
						Debug.Log("觸發門 " + hit.collider.name);
						Animator anim = hit.collider.gameObject.transform.parent.GetComponent<Animator>();

						if (!anim.GetBool("opened"))
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