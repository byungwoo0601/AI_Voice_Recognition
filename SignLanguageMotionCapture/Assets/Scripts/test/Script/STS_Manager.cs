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
		public Button TestButton;
		public Button EditButton;
		public Button EditStopButton;

		[Header("Prefabs")]
		public GameObject Text_prefab;
		public GameObject InputField_prefab;

		[Header("GameObject")]
		public GameObject Clicked_Image;
		public GameObject Text_parent;
		public GameObject InputField_parent;

		[Header("Text")]
		public Text _resultText;

		[Header("TMP_InputField")]
		public TMP_InputField inputField;

		[Header("ScrollRect")]
		public ScrollRect scrollRect;
		public ScrollRect scrollRect_Input;

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
			TestButton.onClick.AddListener(TestButtonOnClickEventHandler);
			EditButton.onClick.AddListener(EditButtonOnClickHandler);
			EditStopButton.onClick.AddListener(EditStopButtonOnClickHandler);

			_speechRecognition.SetMicrophoneDevice(_speechRecognition.GetMicrophoneDevices()[0]);
		}
        private void OnDestroy()
		{
			_speechRecognition.InterimResultDetectedEvent -= InterimResultDetectedEventHandler;
			_speechRecognition.FinalResultDetectedEvent -= FinalResultDetectedEventHandler;
		}
		private void EditButtonOnClickHandler() // ���� ���� ��ư �� Ŭ�� �ڵ鷯
		{
			scrollRect.gameObject.SetActive(false);
			scrollRect_Input.gameObject.SetActive(true);

			EditButton.gameObject.SetActive(false);
			EditStopButton.gameObject.SetActive(true);
		}
		private void EditStopButtonOnClickHandler() // ���� ���� ��ư �� Ŭ�� �ڵ鷯
		{
			int index = Text_parent.transform.childCount;
			for (int i = 0; i < index; i++) 
            {
				Text_parent.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = InputField_parent.transform.GetChild(i).GetComponent<TMP_InputField>().text; 
            }

			scrollRect_Input.gameObject.SetActive(false);
			scrollRect.gameObject.SetActive(true);

			EditStopButton.gameObject.SetActive(false);
			EditButton.gameObject.SetActive(true);
		}
		private void DeleteButtonOnClickHandler() // �Էµ� �ؽ�Ʈ ���� ��ư �� Ŭ�� �ڵ鷯
        {
			if (Clicked_Image == null)
			{
				Debug.Log("������Ʈ ���� �� �ƽ��ϴ�.");
			}
			Destroy(Clicked_Image);
			count--;
        }
		private void StartRecordButtonOnClickHandler() // stt�Է� ���� ��ư �� Ŭ�� �ڵ鷯
		{
			if (_resultText == null)
			{
				Instantiate(Text_prefab, Text_parent.transform);
				Debug.Log("�Էµ� �ؽ�Ʈ ������Ʈ�� ��� ���ο� ������Ʈ�� �����Ͽ����ϴ�.");
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
		private async void StopRecordButtonOnClickHandler() // stt�Է� ���� ��ư �� Ŭ�� �ڵ鷯
		{
			await _speechRecognition.StopStreamingRecognition();

			RecordButton_1.gameObject.SetActive(true);
			RecordButton_2.gameObject.SetActive(false);

			count++;

			pushInputField(_resultText.text);
			DuplicationObject();
			
			Invoke("StringEmpty", 0.3f);
		}
		private void StreamingRecognitionStartedEventHandler() // stt�Է� ���� �̺�Ʈ �ڵ鷯
		{
			RecordButton_2.interactable = true;
			RecordButton_1.interactable = false;
		}
		private void StreamingRecognitionEndedEventHandler() // stt�Է� ���� �̺�Ʈ �ڵ鷯
		{
			RecordButton_2.interactable = false;
			RecordButton_1.interactable = true;
		}
		private void TestButtonOnClickEventHandler() // �׽�Ʈ ��ư ���Ŀ� �ִϸ��̼� ���� ��ư���� ���� ����
		{
            Instantiate(Text_prefab, Text_parent.transform);
            count++;
			_resultText = Text_parent.transform.GetChild(count - 1).GetComponentInChildren<Text>();
			_resultText.text = string.Empty;
		}
		private void StreamingRecognitionFailedEventHandler(string error) // stt�Է� ���� �߻� �� �̺�Ʈ �ڵ鷯
		{
			_resultText.text = $"<color=red>Start record Failed due to: {error}.</color>";

			RecordButton_2.interactable = false;
			RecordButton_1.interactable = true;
		}
		private void InterimResultDetectedEventHandler(SpeechRecognitionAlternative alternative) // �߰� �Է� �� �ؽ�Ʈ (���� �Է°� �ٸ� �� ����)
		{
			_resultText.text = $"{alternative.Transcript}";

			scrollRect.verticalNormalizedPosition = 0f;
		}
		private void FinalResultDetectedEventHandler(SpeechRecognitionAlternative alternative) // ���� �Է� �� �ؽ�Ʈ
		{
			_resultText.text = $"{alternative.Transcript}";
			
			scrollRect.verticalNormalizedPosition = 0f;
		}
		private void DuplicationObject() //���� �Էµ� ������Ʈ ����(����)
		{
			Instantiate(Text_prefab, Text_parent.transform);
			_resultText = Text_parent.transform.GetChild(count - 1).GetComponentInChildren<Text>();
		}
		private void StringEmpty() //text ����
        {
			_resultText.text = string.Empty;
		}
		private void pushInputField(string text)
        {
			inputField.text = text;
			Instantiate(InputField_prefab, InputField_parent.transform);
			inputField = InputField_parent.transform.GetChild(count - 1).GetComponentInChildren<TMP_InputField>();
		}
	}
}
