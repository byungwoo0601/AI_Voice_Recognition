using System;
using System.Collections.Generic;
using Google.Cloud.Speech.V1;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples
{
	public class GCSSR_Example : MonoBehaviour
	{
		private GCStreamingSpeechRecognition _speechRecognition;

		public Button _startRecordButton,
					   _stopRecordButton,
					   _refreshMicrophonesButton;

		public Image _speechRecognitionState;

		public Text _resultText;

		public Dropdown _languageDropdown,
						 _microphoneDevicesDropdown;

		public InputField _contextPhrasesInputField;

		public ScrollRect scrollRect;

		public Image voiceLevelImage;

		public float voiceDetectionThreshold = 0.02f;

		private void Start()
		{
			_speechRecognition = GCStreamingSpeechRecognition.Instance;
			_speechRecognition.StreamingRecognitionStartedEvent += StreamingRecognitionStartedEventHandler;
			_speechRecognition.StreamingRecognitionFailedEvent += StreamingRecognitionFailedEventHandler;
			_speechRecognition.StreamingRecognitionEndedEvent += StreamingRecognitionEndedEventHandler;
			_speechRecognition.InterimResultDetectedEvent += InterimResultDetectedEventHandler;
			_speechRecognition.FinalResultDetectedEvent += FinalResultDetectedEventHandler;

			_startRecordButton.onClick.AddListener(StartRecordButtonOnClickHandler);
			_stopRecordButton.onClick.AddListener(StopRecordButtonOnClickHandler);
			_refreshMicrophonesButton.onClick.AddListener(RefreshMicsButtonOnClickHandler);

			_microphoneDevicesDropdown.onValueChanged.AddListener(MicrophoneDevicesDropdownOnValueChangedEventHandler);

			_startRecordButton.interactable = true;
			_stopRecordButton.interactable = false;
			_speechRecognitionState.color = Color.yellow;

			_languageDropdown.ClearOptions();

			for (int i = 0; i < Enum.GetNames(typeof(Enumerators.LanguageCode)).Length; i++)
			{
				_languageDropdown.options.Add(new Dropdown.OptionData(((Enumerators.LanguageCode)i).Parse()));
			}

			_languageDropdown.value = _languageDropdown.options.IndexOf(_languageDropdown.options.Find(x => x.text == Enumerators.LanguageCode.en_GB.Parse()));

			RefreshMicsButtonOnClickHandler();
		}

		private void OnDestroy()
		{
			_speechRecognition.StreamingRecognitionStartedEvent -= StreamingRecognitionStartedEventHandler;
			_speechRecognition.StreamingRecognitionFailedEvent -= StreamingRecognitionFailedEventHandler;
			_speechRecognition.StreamingRecognitionEndedEvent -= StreamingRecognitionEndedEventHandler;
			_speechRecognition.InterimResultDetectedEvent -= InterimResultDetectedEventHandler;
			_speechRecognition.FinalResultDetectedEvent -= FinalResultDetectedEventHandler;
		}

        private void Update()
        {
			if (_speechRecognition.isRecording)
			{
				if (_speechRecognition.GetMaxFrame() > 0)
				{
					float max = voiceDetectionThreshold;
					float current = _speechRecognition.GetLastFrame() / max;

					if (current >= 1f)
					{
						voiceLevelImage.fillAmount = Mathf.Lerp(voiceLevelImage.fillAmount, Mathf.Clamp(current / 2f, 0, 1f), 30 * Time.deltaTime);
					}
					else
					{
						voiceLevelImage.fillAmount = Mathf.Lerp(voiceLevelImage.fillAmount, Mathf.Clamp(current / 2f, 0, 0.5f), 30 * Time.deltaTime);
					}

					voiceLevelImage.color = current >= 1f ? Color.green : Color.red;
				}
			}
			else
			{
				voiceLevelImage.fillAmount = 0f;
			}
		}

        private void RefreshMicsButtonOnClickHandler()
		{
			_speechRecognition.RequestMicrophonePermission();

			_microphoneDevicesDropdown.ClearOptions();

			for (int i = 0; i < _speechRecognition.GetMicrophoneDevices().Length; i++)
			{
				_microphoneDevicesDropdown.options.Add(new Dropdown.OptionData(_speechRecognition.GetMicrophoneDevices()[i]));
			}

			//smart fix of dropdowns
			_microphoneDevicesDropdown.value = 1;
			_microphoneDevicesDropdown.value = 0;
		}

		private void MicrophoneDevicesDropdownOnValueChangedEventHandler(int value)
		{
			if (!_speechRecognition.HasConnectedMicrophoneDevices())
				return;
			_speechRecognition.SetMicrophoneDevice(_speechRecognition.GetMicrophoneDevices()[value]);
		}

		private void StartRecordButtonOnClickHandler()
		{
			_resultText.text = string.Empty;

			List<List<string>> context = new List<List<string>>();

			if(_contextPhrasesInputField.text.Length > 0)
			{
				string[] split = _contextPhrasesInputField.text.Split(',');

				List<string> context1 = new List<string>();
				foreach(var item in split)
				{
					context1.Add(item.TrimStart(' ').TrimEnd(' '));
				}

				context.Add(context1);
			}

			Google.Cloud.Speech.V1.SpeechContext[] contexts = new Google.Cloud.Speech.V1.SpeechContext[context.Count];

			for(int i = 0; i < context.Count; i++)
            {
				contexts[i] = new Google.Cloud.Speech.V1.SpeechContext();

				foreach(var phrase in context[i])
                {
					contexts[i].Phrases.Add(phrase);
                }
            }

            _speechRecognition.config.recognitionSettings.languageCode = (Enumerators.LanguageCode)_languageDropdown.value;
            _speechRecognition.config.recognitionSettings.speechContexts = contexts;

            _speechRecognition.StartStreamingRecognition(_speechRecognition.config.recognitionSettings);
		}

		private async void StopRecordButtonOnClickHandler()
		{
			await _speechRecognition.StopStreamingRecognition();
		}

		private void StreamingRecognitionStartedEventHandler()
		{
			_speechRecognitionState.color = Color.red;

			_stopRecordButton.interactable = true;
			_startRecordButton.interactable = false;
		}

		private void StreamingRecognitionFailedEventHandler(string error)
		{
			_speechRecognitionState.color = Color.yellow;
			_resultText.text = $"<color=red>Start record Failed due to: {error}.</color>";

			_stopRecordButton.interactable = false;
			_startRecordButton.interactable = true;
		}

		private void StreamingRecognitionEndedEventHandler()
        {
			_speechRecognitionState.color = Color.green;

			_stopRecordButton.interactable = false;
			_startRecordButton.interactable = true;
		}

		private void InterimResultDetectedEventHandler(SpeechRecognitionAlternative alternative)
        {
			if(_resultText.text.Length > 1000)
				_resultText.text = string.Empty;

			_resultText.text += $"<b>Alternative:</b> {alternative.Transcript}\n";

			scrollRect.verticalNormalizedPosition = 0f;
		}

		private void FinalResultDetectedEventHandler(SpeechRecognitionAlternative alternative)
		{
			if (_resultText.text.Length > 1000)
				_resultText.text = string.Empty;

			_resultText.text += $"<b>Final:</b> {alternative.Transcript}\n";

			scrollRect.verticalNormalizedPosition = 0f;
		}
    }
}