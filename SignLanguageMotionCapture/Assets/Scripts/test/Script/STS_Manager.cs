using System;
using System.Collections.Generic;
using Google.Cloud.Speech.V1;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition
{
	public class STS_Manager : MonoBehaviour
	{
		public static STS_Manager instance;

		private GCStreamingSpeechRecognition_ _speechRecognition;

		[Header("Buttons")]
		public Button RecordButton_1; //음성 인식 시작 버튼
		public Button RecordButton_2; //음성 인식 종료 버튼
		public Button DeleteButton; //텍스트로 변환된 데이터 제거 버튼
		public Button TestButton; //테스트 버튼 => 추후에 애니메이션 재생 버튼으로 수정 예정
		public Button EditButton; //변환된 텍스트를 직접 수정할 수 있는 수정 모드로 전환되는 버튼
		public Button EditStopButton; //수정 모드 종료 버튼

		[Header("Prefabs")]
		public GameObject Text_prefab; //텍스트로 변환된 데이터가 입력될 오브젝트
		public GameObject InputField_prefab; //수정 모드에 사용될 오브젝트

		[Header("GameObject")]
		public GameObject New_Image; //음성 인식이 종료되었을 때 새롭게 입력될 데이터가 생성되는데 그 데이터가 New_Image에 할당된다
		public GameObject Clicked_Image; //클릭된 오브젝트가 할당된다
		public GameObject Text_parent; //음성 인식이 종료되었을 때 새롭게 입력될 데이터가 생성되는데 이때 Text_parent 오브젝트의 자식으로 생성된다.
		public GameObject InputField_parent; //음성 인식이 종료되었을 때 새롭게 입력될 데이터가 생성되는데 이때 InputField_parent 오브젝트의 자식으로 생성된다.(수정 모드)
											 //실제로 실행 시켜보면 무슨 말인지 알아볼듯

		[Header("Text")]
		public Text _resultText; //변환된 텍스트가 할당 될 변수

		[Header("TMP_InputField")]
		public TMP_InputField inputField;

		[Header("ScrollRect")]
		public ScrollRect scrollRect;
		public ScrollRect scrollRect_Input;

		[Header("Test")]
		public string get_Text; //클릭된 오브젝트에서 리턴 될 텍스트
		public int count; //텍스트가 입력될 오브젝트의 개수

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
		private void EditButtonOnClickHandler() // 수정 시작 버튼 온 클릭 핸들러
		{
			scrollRect.gameObject.SetActive(false);
			scrollRect_Input.gameObject.SetActive(true);

			EditButton.gameObject.SetActive(false);
			EditStopButton.gameObject.SetActive(true);
		}
		private void EditStopButtonOnClickHandler() // 수정 종료 버튼 온 클릭 핸들러
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
		private void DeleteButtonOnClickHandler() // 입력된 텍스트 삭제 버튼 온 클릭 핸들러
        {
			if (Clicked_Image == null)
			{
				Debug.Log("오브젝트 선택 안 됐습니다.");
			}
			Destroy(Clicked_Image);
			count--;
        }
		private void StartRecordButtonOnClickHandler() // stt입력 시작 버튼 온 클릭 핸들러
		{
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
		}
		private async void StopRecordButtonOnClickHandler() // stt입력 종료 버튼 온 클릭 핸들러
		{
			await _speechRecognition.StopStreamingRecognition();

			RecordButton_1.gameObject.SetActive(true);
			RecordButton_2.gameObject.SetActive(false);

			count++;

			pushInputField(_resultText.text);
			DuplicationObject();

            Invoke("StringEmpty", 0.3f);
			Invoke("newImageOn", 0.4f);

		}
		private void StreamingRecognitionStartedEventHandler() // stt입력 시작 이벤트 핸들러
		{
			RecordButton_2.interactable = true;
			RecordButton_1.interactable = false;
		}
		private void StreamingRecognitionEndedEventHandler() // stt입력 종료 이벤트 핸들러
		{
			RecordButton_2.interactable = false;
			RecordButton_1.interactable = true;
		}
		private void TestButtonOnClickEventHandler() // 테스트 버튼 추후에 애니메이션 실행 버튼으로 수정 예정
		{
            Instantiate(Text_prefab, Text_parent.transform);
            count++;
			_resultText = Text_parent.transform.GetChild(count - 1).GetComponentInChildren<Text>();
			_resultText.text = string.Empty;
		}
		private void StreamingRecognitionFailedEventHandler(string error) // stt입력 오류 발생 시 이벤트 핸들러
		{
			_resultText.text = $"<color=red>Start record Failed due to: {error}.</color>";

			RecordButton_2.interactable = false;
			RecordButton_1.interactable = true;
		}
		private void InterimResultDetectedEventHandler(SpeechRecognitionAlternative alternative) // 중간 입력 될 텍스트 (최종 입력과 다를 수 있음)
		{
			_resultText.text = $"{alternative.Transcript}";

			scrollRect.verticalNormalizedPosition = 0f;
		}
		private void FinalResultDetectedEventHandler(SpeechRecognitionAlternative alternative) // 최종 입력 될 텍스트
		{
			_resultText.text = $"{alternative.Transcript}";
			
			scrollRect.verticalNormalizedPosition = 0f;
		}
		private void DuplicationObject() //다음 입력될 오브젝트 생성(복제)
		{
			Instantiate(Text_prefab, Text_parent.transform);
			New_Image = Text_parent.transform.GetChild(count - 1).gameObject;
			_resultText = Text_parent.transform.GetChild(count - 1).GetComponentInChildren<Text>();
		}
		private void StringEmpty() //text 비우기
        {
			_resultText.text = string.Empty;
		}
		private void pushInputField(string text)
        {
			inputField.text = text;
			Instantiate(InputField_prefab, InputField_parent.transform);
			inputField = InputField_parent.transform.GetChild(count - 1).GetComponentInChildren<TMP_InputField>();
		}
		private void newImageOn()
        {
			New_Image.SetActive(true);
        }
	}
}
