using System;
using System.Collections.Generic;
using Google.Cloud.Speech.V1;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples
{
	public class STS_Manager : MonoBehaviour
	{
		public static STS_Manager instance;

		private GCStreamingSpeechRecognition_ _speechRecognition;

		[Header("Buttons")]
		public Button RecordButton_1;
		public Button RecordButton_2;
		public Button DeleteButton;
		public Button DuplicationButton;
		public Button EditButton;

		[Header("Prefabs")]
		public GameObject Text_prefab;
		public GameObject InputField_prefab;

		[Header("GameObject")]
		public GameObject Clicked_Image;
		public GameObject Text_parent;
		public GameObject InputField_parent;

		[Header("Text")]
		public Text _resultText;

		[Header("ScrollRect")]
		public ScrollRect scrollRect;

		[Header("TMP_InputField")]
		public TMP_InputField inputField;

		private float voiceDetectionThreshold = 0.02f;

		[Header("Test")]
		public string temp_Text;
		public string get_Text;
		public int count;
		private void Awake()
        {
			instance = this;
		}
        private void Start()
		{
			count = 1;
			_speechRecognition = GCStreamingSpeechRecognition_.Instance;
			_speechRecognition.StreamingRecognitionStartedEvent += StreamingRecognitionStartedEventHandler;
			_speechRecognition.StreamingRecognitionFailedEvent += StreamingRecognitionFailedEventHandler;
			_speechRecognition.StreamingRecognitionEndedEvent += StreamingRecognitionEndedEventHandler;
			_speechRecognition.InterimResultDetectedEvent += InterimResultDetectedEventHandler;
			_speechRecognition.FinalResultDetectedEvent += FinalResultDetectedEventHandler;

			RecordButton_1.onClick.AddListener(StartRecordButtonOnClickHandler);
			RecordButton_2.onClick.AddListener(StopRecordButtonOnClickHandler);
			DeleteButton.onClick.AddListener(DeleteButtonOnClickHandler);
			DuplicationButton.onClick.AddListener(DuplicationButtonOnClickEventHandler);
			EditButton.onClick.AddListener(EditButtonOnClickHandler);

			_speechRecognition.SetMicrophoneDevice(_speechRecognition.GetMicrophoneDevices()[0]);
		}
        private void OnDestroy()
		{
			_speechRecognition.InterimResultDetectedEvent -= InterimResultDetectedEventHandler;
			_speechRecognition.FinalResultDetectedEvent -= FinalResultDetectedEventHandler;
		}
		private void EditButtonOnClickHandler()
        {

        }
		private void DeleteButtonOnClickHandler()
        {
			if(Clicked_Image==null)
			{
				Debug.Log("오브젝트 선택 안 됐습니다.");
			}
			Destroy(Clicked_Image);
			count--;
        }
		private void StartRecordButtonOnClickHandler()
		{
			if (_resultText == null)
			{
				Instantiate(Text_prefab, Text_parent.GetComponent<Transform>());
				Debug.Log("입력될 텍스트 오브젝트가 없어서 새로운 오브젝트를 생성하였습니다.");
			}

			List<List<string>> context = new List<List<string>>();

			Google.Cloud.Speech.V1.SpeechContext[] contexts = new Google.Cloud.Speech.V1.SpeechContext[context.Count];

			for (int i = 0; i < context.Count; i++)
			{
				contexts[i] = new Google.Cloud.Speech.V1.SpeechContext();

				foreach (var phrase in context[i])
				{
					contexts[i].Phrases.Add(phrase);
				}
			}

			if (count == 0)
			{
				_resultText.text = string.Empty;
			}

			_speechRecognition.config.recognitionSettings.speechContexts = contexts;

			_speechRecognition.StartStreamingRecognition(_speechRecognition.config.recognitionSettings);

			RecordButton_2.gameObject.SetActive(true);
			RecordButton_1.gameObject.SetActive(false);

			temp_Text = _resultText.text;
		}
		private async void StopRecordButtonOnClickHandler()
		{
			await _speechRecognition.StopStreamingRecognition();

			RecordButton_1.gameObject.SetActive(true);
			RecordButton_2.gameObject.SetActive(false);

			count++;

			pushInputField(_resultText.text);
			DuplicationObject();
			StringEmpty();
		}
		private void StreamingRecognitionStartedEventHandler()
		{
			RecordButton_2.interactable = true;
			RecordButton_1.interactable = false;
		}
		private void DuplicationButtonOnClickEventHandler()
		{
			Instantiate(Text_prefab, Text_parent.GetComponent<Transform>());
			count++;
			_resultText = Text_parent.GetComponent<Transform>().GetChild(count - 1).GetComponentInChildren<Text>();
			_resultText.text = string.Empty;
		}
		private void StreamingRecognitionFailedEventHandler(string error)
		{
			_resultText.text = $"<color=red>Start record Failed due to: {error}.</color>";

			RecordButton_2.interactable = false;
			RecordButton_1.interactable = true;
		}
		private void StreamingRecognitionEndedEventHandler()
		{
			RecordButton_2.interactable = false;
			RecordButton_1.interactable = true;
		}
		private void InterimResultDetectedEventHandler(SpeechRecognitionAlternative alternative)
		{
			_resultText.text = $"{alternative.Transcript}";

			scrollRect.verticalNormalizedPosition = 0f;
		}
		private void FinalResultDetectedEventHandler(SpeechRecognitionAlternative alternative)
		{
			_resultText.text = $"{alternative.Transcript}";
			
			scrollRect.verticalNormalizedPosition = 0f;
		}
		private void DuplicationObject()
		{
			Instantiate(Text_prefab, Text_parent.GetComponent<Transform>());
			_resultText = Text_parent.GetComponent<Transform>().GetChild(count - 1).GetComponentInChildren<Text>();			
		}
		private void StringEmpty()
        {
			_resultText.text = string.Empty;
		}
		private void pushInputField(string text)
        {
			inputField.text = text;
			Instantiate(InputField_prefab, InputField_parent.GetComponent<Transform>());
		}
	}
}
