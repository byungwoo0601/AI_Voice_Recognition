using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Google.Protobuf;
using Google.Cloud.Speech.V1;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using FrostweepGames.Plugins.Native;
using FrostweepGames.Plugins.Editor;
using System.Collections;
using UnityEngine.Assertions;
using Grpc.Core;

namespace FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition
{
    public class GCStreamingSpeechRecognition_ : MonoBehaviour
    {
		private const bool LogExceptions
#if UNITY_EDITOR
			= true;
#else
			= false;
#endif

        private const int SampleRate = 16000;
		//������ ���� ���� ���ļ� ������ ǰ���� ��������
		private const int StreamingRecognitionTimeLimit = 110;
		//�ִ� ���� ���� 110��
		private const int AudioChunkSize = SampleRate;
		//���� ũ��

		public static GCStreamingSpeechRecognition_ Instance { get; private set; }
		//�ٸ� ��ũ��Ʈ���� �ش� ��ũ��Ʈ�� ������ �� ����ϴ� ����

		public event Action StreamingRecognitionStartedEvent;
		//�����ν� ���� �̺�Ʈ ����
		public event Action<string> StreamingRecognitionFailedEvent;
		//�����ν� ���� �̺�Ʈ ����
		public event Action StreamingRecognitionEndedEvent;
		//�����ν� ���� �̺�Ʈ ����
		public event Action<SpeechRecognitionAlternative> InterimResultDetectedEvent;
		//�����ν� �߰� ����� ����ؼ� ������Ʈ ���ִ� �̺�Ʈ ����
		public event Action<SpeechRecognitionAlternative> FinalResultDetectedEvent;
		//�����ν� ���� ����� ������Ʈ ���ִ� �̺�Ʈ ����

		private SpeechClient _speechClient;

		private SpeechClient.StreamingRecognizeStream _streamingRecognizeStream;
		//���� api�� ������ �ϴ� Ŭ������ �ν��Ͻ� ����

		private AudioClip _workingClip;
		//���� ���� ���� ����� Ŭ��

		private Coroutine _checkOnMicAndRunStreamRoutine;
		//����ũ üũ �� ���� �ν� �ڷ�ƾ�� �Ҵ� �� ����

		private CancellationTokenSource _cancellationToken;

		private float _recordingTime;
		//���� ������ �ð��� ����Ǵ� ����

		private int _currentSamplePosition;
		//���� ����ũ ���ø� ��ġ

		private int _previousSamplePosition;
		//���� ����ũ ���ø� ��ġ

		private float[] _currentAudioSamples;
		//���� ������ ����� ����

		private List<byte> _currentRecordedSamples;
		//���� ���� �� �����Ͱ� ����Ǵ� ����

		private Config.RecognitionSettings _currentRecognitionSettings;
		//���� �ν� ������ ���õ� ����

		private bool _initialized;
		//�ʱ�ȭ ���� Ȯ�� ����

		private bool _recognition;
		//���� �ν��� Ȱ��ȭ �Ǿ� �ִ��� ���� Ȯ�� ����

		public Config config;
		//���� �ν� ���� ���� ����Ǿ� �ִ� ConfigŸ���� ����

		[ReadOnly]
		public bool isRecording;
		//���� ������ ����ǰ� �ִ��� �ƴ��� Ȯ���� �� �ִ� ����

		[ReadOnly]
		public string microphoneDevice;
		//���� ����� ����ũ ����̽� �̸��� ����Ǵ� ����

		private void Awake() //����Ƽ�� ó�� ����Ǹ� ����Ǵ� �����ֱ� �޼ҵ�(Awake �޼ҵ�)
		{
			if(Instance != null) //�ش� ��ũ��Ʈ�� �ν���Ʈ�� �̹� �����ϴ��� Ȯ��
			{
				Destroy(gameObject); //���� �̹� ���� �Ѵٸ� �ش� ��ũ��Ʈ�� ���Ե� ������Ʈ�� �ı��ϰ� ��ũ��Ʈ ������ �ߴ�(����) ��
				return;
            }

            Instance = this; //�ν��Ͻ��� �ش� ��ũ��Ʈ�� �Ҵ�

            Assert.IsNotNull(config, "Config is requried to be added."); //config ������ null���� Assert.IsNotNull�� ���� Ȯ���ϰ�, ���� config�� null�̶�� ������ �����(������� ����) config�ڿ� ������ �ܼ�â�� ��Ÿ��

			Initialize(); //Initialize �޼ҵ�(�ʱ�ȭ) ȣ��
		}
		private async void OnDestroy() //�ش� ��ũ��Ʈ�� ���Ե� ������Ʈ�� �ı��� �� ����Ǵ� �޼ҵ�(OnDestroy �޼ҵ�), async�� Ȱ���� �񵿱� ó��
		{
			if (Instance != this || !_initialized)
				return;
			//�ν��Ͻ��� ���� ������Ʈ �̰ų� �ʱ�ȭ ���θ� Ȯ���ϰ� �ʱ�ȭ ���� �ʾҴٸ� return

			await StopStreamingRecognition(); //StopStreamingRecognition�� �񵿱�� ����, �����Ͱ� ��� ���۵� �� ���� ��ٸ���

			Instance = null;
			//������Ʈ�� �ı��Ǿ����Ƿ� �ν��Ͻ��� null�� ������ �ٸ� ��ũ��Ʈ���� �������� ���ϰ� ��
		}
		private async void Update() //�� �����Ӹ��� ����Ǵ� �޼ҵ�(Update �޼ҵ�), async�� Ȱ���� �񵿱� ó��
		{
			if (Instance != this || !_initialized) 
				return;
			//�ν��Ͻ��� ���� ������Ʈ �̰ų� �ʱ�ȭ ���θ� Ȯ���ϰ� �ʱ�ȭ ���� �ʾҴٸ� return

			if (!isRecording)
				return;
			//���� ������ �ʴٸ� return

			_recordingTime += Time.unscaledDeltaTime;
			//���� �����ǰ� �ִ� �ð��� ����ؼ� ������Ų��.

			if(_recordingTime >= StreamingRecognitionTimeLimit) //���� �����ǰ� �ִ� �ð��� �ִ� �����ð��� ���� ���
			{
				await RestartStreamingRecognitionAfterLimit(); //���� �ν� ����� �޼ҵ�
				_recordingTime = 0; //������ �ð��� 0���� �ʱ�ȭ
			}

            HandleRecordingData();
        }
		private void FixedUpdate()
		{
			if (Instance != this || !_initialized)
				return;

			WriteDataToStream();
		}
		public void RequestMicrophonePermission()
		{
			if (!CustomMicrophone.HasMicrophonePermission())
			{
				CustomMicrophone.RequestMicrophonePermission();
			}
		}
		public void SetMicrophoneDevice(string deviceName)//������ ���� ����ũ�� �����ϴ� �޼ҵ�
		{
			if (isRecording)//���� ���� ���̶�� return
				return;

			microphoneDevice = deviceName;
		}
		public string[] GetMicrophoneDevices()
		{
			return CustomMicrophone.devices;
		}
		public bool HasConnectedMicrophoneDevices()//���� �� ����ũ�� �ִ��� Ȯ���ϴ� �޼ҵ�
		{
			return CustomMicrophone.HasConnectedMicrophoneDevices();
		}
		public void StartStreamingRecognition(Config.RecognitionSettings recognitionSettings)//���� �ν� ���� �޼ҵ�
		{
			if(!_initialized)//�ʱ�ȭ ���� Ȯ��
			{
				StreamingRecognitionFailedEvent?.Invoke("Failed to start recogntion due to: 'Not initialized'");
				return;
			}
			//�ʱ�ȭ ���� �ʾҴٸ� return

            _currentRecognitionSettings = recognitionSettings; //�Է����� ���� recognitionSettings(���� ��)�� ���� ���� �ν� ���� ������ �Է�
			_checkOnMicAndRunStreamRoutine = StartCoroutine(CheckOnMicrophoneAndRunStream());
		}
		public async Task StopStreamingRecognition()//���� �ν� �ߴ� �޼ҵ�
		{
			if (!isRecording || !_recognition) //����, ���� �ν� Ȱ��ȭ ���� Ȯ��
				return; //Ȱ��ȭ �Ǿ� ���� ������ return

			_recognition = false; //���� �ν� ���� false

			StopRecording(); //���� ���� �޼ҵ� ����

			if (_streamingRecognizeStream != null)
			{
				await _streamingRecognizeStream.WriteCompleteAsync(); //���� api���� ���� �ν� �Ϸ�ǵ��� �ϴ� �޼ҵ尡 ����� �� ���� ��ٸ���.
			}
		}
		private void FinishCleaningAfterStreamingRecognition() //���� �ν��� ���� �� ����Ǵ� �޼ҵ�
        {
            _streamingRecognizeStream = null; //���� api ���� null�� ������ ���� ����
            _currentRecordedSamples = null; //���� ���� �� ������ null�� ������ ���� ����

            if (_checkOnMicAndRunStreamRoutine != null)
            {
                StopCoroutine(_checkOnMicAndRunStreamRoutine);
                _checkOnMicAndRunStreamRoutine = null;
            }

            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();
                _cancellationToken.Dispose();
                _cancellationToken = null;
            }

            StreamingRecognitionEndedEvent?.Invoke();
        }
		private async Task RestartStreamingRecognitionAfterLimit() //���� �ν� ����� �޼ҵ�
		{
			await StopStreamingRecognition(); //���� �ν� �ߴ� �޼ҵ尡 ����� �� ���� ��ٸ�

			_checkOnMicAndRunStreamRoutine = StartCoroutine(CheckOnMicrophoneAndRunStream()); //����ũ üũ �� ���� �ν� �ڷ�ƾ�� �Ҵ� ��
		}
		private IEnumerator CheckOnMicrophoneAndRunStream()
		{
			while (!HasConnectedMicrophoneDevices()) //����� ����ũ�� �ִ��� Ȯ��
			{
				RequestMicrophonePermission(); //����ũ ������ ��û
				yield return null; //�� ������ ��ٸ� �� ���� �ݺ�
			}

			RunStreamingRecognition(); //���� �ν� ���� �޼ҵ� ����(����ũ ������ �Ǿ��� ��)

			_checkOnMicAndRunStreamRoutine = null; //����ũ üũ �� ���� �ν� �ڷ�ƾ null�� �ʱ�ȭ
		}
		private async void RunStreamingRecognition() //���� �ν� ����
		{
			if (isRecording) //���� ���̸� return
			{
				StreamingRecognitionFailedEvent?.Invoke("Already recording");
				return;
			}

			if (!StartRecording()) //���� ���� bool Ÿ�� �޼ҵ忡�� ������ ����ũ�� ���� false�� ��ȯ�Ǿ��ٸ� return
			{
				StreamingRecognitionFailedEvent?.Invoke("Cannot start recording");
				return;
			}

			_streamingRecognizeStream = _speechClient.StreamingRecognize();

			var recognitionConfig = new RecognitionConfig() //���� �ν� ����(_currentRecognitionSettings�� StartStreamingRecognition���� �ʱ�ȭ �Ǿ���)
			{
				Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
				SampleRateHertz = SampleRate,
				LanguageCode = _currentRecognitionSettings.languageCode.Parse(),
				MaxAlternatives = _currentRecognitionSettings.maxAlternatives,
				EnableSeparateRecognitionPerChannel = _currentRecognitionSettings.enableSeparateRecognitionPerChannel,
                AudioChannelCount = _currentRecognitionSettings.audioChannelCount,
                ProfanityFilter = _currentRecognitionSettings.profanityFilter,
                Adaptation = _currentRecognitionSettings.adaptation,
                EnableWordTimeOffsets = _currentRecognitionSettings.enableWordTimeOffsets,
                EnableWordConfidence = _currentRecognitionSettings.enableWordConfidence,
                EnableAutomaticPunctuation = _currentRecognitionSettings.enableAutomaticPunctuation,
                EnableSpokenPunctuation = _currentRecognitionSettings.enableSpokenPunctuation,
                EnableSpokenEmojis = _currentRecognitionSettings.enableSpokenEmojis,
                DiarizationConfig = _currentRecognitionSettings.diarizationConfig,
                Model = _currentRecognitionSettings.model.Parse(),
                UseEnhanced = _currentRecognitionSettings.useEnhanced
			};

			if (_currentRecognitionSettings.speechContexts != null)
			{
				foreach (var context in _currentRecognitionSettings.speechContexts)
				{
                    recognitionConfig.SpeechContexts.Add(context);
                }
			}

			StreamingRecognitionConfig streamingConfig = new StreamingRecognitionConfig()
			{
				Config = recognitionConfig,
				InterimResults = config.interimResults,
			};

			try
			{
				await _streamingRecognizeStream.WriteAsync(new StreamingRecognizeRequest()
				{
					StreamingConfig = streamingConfig
				});
			}
			catch(RpcException ex)
			{
				StopRecording();

				_streamingRecognizeStream = null;

				StreamingRecognitionFailedEvent?.Invoke($"Cannot start recognition due to: {ex.Message}");
				return;
			}

			_recognition = true;

			StreamingRecognitionStartedEvent.Invoke();

			_cancellationToken = new CancellationTokenSource();

			HandleStreamingRecognitionResponsesTask();
		}
		private bool StartRecording() //���� ���� bool Ÿ�� �޼ҵ�
		{
			if (string.IsNullOrEmpty(microphoneDevice)) //������ ����ũ�� ���ٸ� return false
				return false;

			_workingClip = CustomMicrophone.Start(microphoneDevice, true, 3, SampleRate); //���� ����

			_currentAudioSamples = new float[_workingClip.samples]; //float�� �迭 ���� [_workingClip.samples(������ ����� Ŭ���� ũ�Ⱑ �迭�� ũ�Ⱑ �ȴ�)], ������ float�� �迭�� ���� ������ ����� ���� �迭�� �Ҵ�

			_currentRecordedSamples = new List<byte>(); //���� �����͸� text�� ��ȯ��Ű���� ���� �����͸� byte�� ��ȯ�ؾ� �ϴµ� �� �������� ����� byte�� ����� ������

			isRecording = true;

			return true; //���� ���� �Ǿ��ٴ� true return
		}
		private void StopRecording() //���� ���� �޼ҵ�
		{
			if (!isRecording) //���� ���� �ƴϸ� return
				return;

			if (string.IsNullOrEmpty(microphoneDevice)) //����ũ�� �Ҵ���� �ʾҴٸ� return
				return;

			CustomMicrophone.End(microphoneDevice);

			MonoBehaviour.Destroy(_workingClip);

			_currentRecordedSamples.Clear(); //���� ���� �� ������ �ʱ�ȭ

			isRecording = false; //���� ���� false
		}
		private async void HandleStreamingRecognitionResponsesTask()
		{
			try
			{
				while (await _streamingRecognizeStream.GetResponseStream().MoveNextAsync(_cancellationToken.Token))
				{
					var current = _streamingRecognizeStream.GetResponseStream().Current;

					if (current == null)
						return;

					var results = _streamingRecognizeStream.GetResponseStream().Current.Results;

					if (results.Count <= 0)
						continue;

					StreamingRecognitionResult result = results[0];
					if (result.Alternatives.Count <= 0)
						continue;

					if (result.IsFinal)
					{
                        FinalResultDetectedEvent.Invoke(result.Alternatives[0]);
					}
					else
					{
						if (config.interimResults)
						{
							for (int i = 0; i < _currentRecognitionSettings.maxAlternatives; i++)
							{
								if (i >= result.Alternatives.Count)
									break;

								InterimResultDetectedEvent.Invoke(result.Alternatives[i]);
                            }
                        }
					}
				}

				if(!_recognition)
                {
					FinishCleaningAfterStreamingRecognition();
                }
			}
			catch (Exception ex) 
			{
				if (LogExceptions)
				{
					Debug.LogException(ex);
				}
			}
		}
		private void HandleRecordingData()
		{
			if (!isRecording)
				return;

			_currentSamplePosition = CustomMicrophone.GetPosition(microphoneDevice); //���� �����ǰ� �ִ� ��ġ �Ҵ�

			if (CustomMicrophone.GetRawData(ref _currentAudioSamples, _workingClip)) //_workingClip(���� ���� ���� ����� Ŭ��), _currentAudioSamples(���� ������ ����� ���� float�� �迭)
			{
				if (_previousSamplePosition > _currentSamplePosition) //���� ����ũ ���ø� ��ġ�� ���� ����ũ ���ø� ��ġ���� ũ�ٸ�
				{
					for (int i = _previousSamplePosition; i < _currentAudioSamples.Length; i++) //���� ����ũ ���ø� ��ġ�� �� ũ�ٸ� ���� ��ġ(_previousSamplePosition) ���� _currentAudioSamples.Length����
					{
						_currentRecordedSamples.AddRange(FloatToBytes(_currentAudioSamples[i])); //_currentAudioSamples(���� ������ ����� ���� float�� �迭)���� for������ i�� �ش��ϴ� ��ġ�� �����͸� byte�� ��ȯ�ϰ�
																								 //_currentRecordedSamples(���� ������ �����Ͱ� ����Ǵ� ����)�� �߰�
					}
					_previousSamplePosition = 0; //���� ����ũ ���ø� ��ġ�� 0���� �ʱ�ȭ �� ��
				}

				for (int i = _previousSamplePosition; i < _currentSamplePosition; i++) //0(ó��)���� ���� ����ũ ���ø� ��ġ���� 
				{
					_currentRecordedSamples.AddRange(FloatToBytes(_currentAudioSamples[i]));
				}

				_previousSamplePosition = _currentSamplePosition; //���� ���ø� ��ġ�� ���� ���ø� ��ġ�� �Ҵ�
			}
			//�ش� �޼ҵ��� ������ ������ ����� ����(_currentAudioSamples, float��)�� _currentRecordedSamples(byte�� ����Ʈ)�� ������ ��ȯ �� �߰� �ϴ� ������ �Ѵ�.
			//Update �޼ҵ忡�� �� ������ ���� ���� ��
		}
		private async void WriteDataToStream() //�����͸� ���� api ������ �����ϴ� ����
		{
			if (_streamingRecognizeStream == null) //���� api�� ���� �Ǿ����� ����
				return;

			ByteString chunk;
			List<byte> samplesChunk = null;

			if (isRecording || (_currentRecordedSamples != null && _currentRecordedSamples.Count > 0)) //���� ���̰ų� _currentRecordedSamples(byte�� ����Ʈ)�� ��� ���� �ʴٸ� ����Ǵ� �ڵ�
			{
				if (_currentRecordedSamples.Count >= AudioChunkSize * 2)
				{
					samplesChunk = _currentRecordedSamples.GetRange(0, AudioChunkSize * 2);
					_currentRecordedSamples.RemoveRange(0, AudioChunkSize * 2);
				}
				else if(!isRecording)
				{
					samplesChunk = _currentRecordedSamples.GetRange(0, _currentRecordedSamples.Count);
					_currentRecordedSamples.Clear();
				}

				if (samplesChunk != null && samplesChunk.Count > 0)
				{
					chunk = ByteString.CopyFrom(samplesChunk.ToArray(), 0, samplesChunk.Count);

					try
					{
						await _streamingRecognizeStream.WriteAsync(new StreamingRecognizeRequest() { AudioContent = chunk });
					}
					catch(RpcException ex)
					{
						StreamingRecognitionFailedEvent?.Invoke($"Cannot proceed recognition due to: {ex.Message}");

						_streamingRecognizeStream = null;

						await StopStreamingRecognition();
					}
				}
			}
		}
		private void Initialize() //�ʱ�ȭ �Լ�
		{
			string credentialJson;

            if (config.googleCredentialLoadFromResources)
            {
                if (string.IsNullOrEmpty(config.googleCredentialFilePath) || string.IsNullOrWhiteSpace(config.googleCredentialFilePath)) //���� api ����� �� �ʿ��� key ���� ������ �����Ͱ� ���ٸ� ����Ǵ� �ڵ�
                {
					Debug.LogException(new Exception("The googleCredentialFilePath is empty. Please fill path to file.")); //�ش� ������ ��������� ��θ� ä���� ���� �ֿܼ� ���
					return;
                }

				TextAsset textAsset = Resources.Load<TextAsset>(config.googleCredentialFilePath); //key ����(json������)���� api key ���� 

				if(textAsset == null) //api key�� ������ �ȵǾ��ٸ� ����Ǵ� �ڵ�
                {
					Debug.LogException(new Exception($"Couldn't load file: {config.googleCredentialFilePath} .")); //������ ���� �ȵǾ��ٴ� ���� �ֿܼ� ���
					return;
				}

				credentialJson = textAsset.text; //textAsset�����Ϳ��� text ���
			}
            else
            {
				credentialJson = config.googleCredentialJson;
			}

			if (string.IsNullOrEmpty(credentialJson) || string.IsNullOrWhiteSpace(credentialJson)) //���� api key�� ����ִٸ� ����Ǵ� �ڵ�
			{
				Debug.LogException(new Exception("The Google service account credential is empty."));
				return;
			}

			try
			{
#pragma warning disable CS1701
				_speechClient = new SpeechClientBuilder
				{
					ChannelCredentials = GoogleCredential.FromJson(credentialJson).ToChannelCredentials()
				}.Build();
#pragma warning restore CS1701
				_initialized = true;
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private byte[] FloatToBytes(float sample) //���� �����͸� byte�������� ��ȯ�ϴ� �ڵ�
		{
			return System.BitConverter.GetBytes((short)(sample * 32767));
		}
	}
}