using System;
using System.Collections.Generic;
using Google.Cloud.Speech.V1;
using UnityEngine.UI;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples
{
	public class STS_Manager : MonoBehaviour
	{
		private GCStreamingSpeechRecognition_ _speechRecognition;

		public Button RecordButton_1,
						RecordButton_2;

		public Text _resultText;

		public ScrollRect scrollRect;

		public float voiceDetectionThreshold = 0.02f;

		public int record_count = 0;
		public string temp_text;		

		private void Start()
		{
			_speechRecognition = GCStreamingSpeechRecognition_.Instance;
			_speechRecognition.StreamingRecognitionStartedEvent += StreamingRecognitionStartedEventHandler;
			_speechRecognition.StreamingRecognitionFailedEvent += StreamingRecognitionFailedEventHandler;
			_speechRecognition.StreamingRecognitionEndedEvent += StreamingRecognitionEndedEventHandler;
			_speechRecognition.InterimResultDetectedEvent += InterimResultDetectedEventHandler;
			_speechRecognition.FinalResultDetectedEvent += FinalResultDetectedEventHandler;

			RecordButton_1.onClick.AddListener(StartRecordButtonOnClickHandler);
			RecordButton_2.onClick.AddListener(StopRecordButtonOnClickHandler);

			_speechRecognition.SetMicrophoneDevice(_speechRecognition.GetMicrophoneDevices()[0]);
		}

		private void OnDestroy()
		{
			_speechRecognition.InterimResultDetectedEvent -= InterimResultDetectedEventHandler;
			_speechRecognition.FinalResultDetectedEvent -= FinalResultDetectedEventHandler;
		}

		private void StartRecordButtonOnClickHandler()
		{
			if (record_count == 0) 
            {
				_resultText.text = string.Empty;
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

			_speechRecognition.config.recognitionSettings.speechContexts = contexts;

			_speechRecognition.StartStreamingRecognition(_speechRecognition.config.recognitionSettings);

			RecordButton_2.gameObject.SetActive(true);
			RecordButton_1.gameObject.SetActive(false);

			temp_text = _resultText.text;
		}

		private async void StopRecordButtonOnClickHandler()
		{
			await _speechRecognition.StopStreamingRecognition();

			RecordButton_1.gameObject.SetActive(true);
			RecordButton_2.gameObject.SetActive(false);

			record_count++;
		}
		private void StreamingRecognitionStartedEventHandler()
		{
			RecordButton_2.interactable = true;
			RecordButton_1.interactable = false;
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
			if(record_count == 0)
            {
				_resultText.text = $"{alternative.Transcript}";
			}
            else
			{
				_resultText.text = $"{temp_text}\n{alternative.Transcript}";
			}

			scrollRect.verticalNormalizedPosition = 0f;
		}

		private void FinalResultDetectedEventHandler(SpeechRecognitionAlternative alternative)
		{
			if (record_count == 0 || record_count == 1)
			{
				_resultText.text = $"{alternative.Transcript}";
			}
			else
			{
				_resultText.text = $"{temp_text}\n{alternative.Transcript}";
			}

			scrollRect.verticalNormalizedPosition = 0f;
		}
	}
}
