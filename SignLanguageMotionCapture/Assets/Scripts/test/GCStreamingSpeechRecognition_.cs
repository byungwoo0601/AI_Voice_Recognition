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
		//녹음할 음성 샘플 주파수 음성의 품질과 관련있음
		private const int StreamingRecognitionTimeLimit = 110;
		//최대 녹음 길이 110초
		private const int AudioChunkSize = SampleRate;
		//샘플 크기

		public static GCStreamingSpeechRecognition_ Instance { get; private set; }
		//다른 스크립트에서 해당 스크립트를 참조할 때 사용하는 변수

		public event Action StreamingRecognitionStartedEvent;
		//음성인식 시작 이벤트 변수
		public event Action<string> StreamingRecognitionFailedEvent;
		//음성인식 실패 이벤트 변수
		public event Action StreamingRecognitionEndedEvent;
		//음성인식 종료 이벤트 변수
		public event Action<SpeechRecognitionAlternative> InterimResultDetectedEvent;
		//음성인식 중간 결과를 계속해서 업데이트 해주는 이벤트 변수
		public event Action<SpeechRecognitionAlternative> FinalResultDetectedEvent;
		//음성인식 최종 결과를 업데이트 해주는 이벤트 변수

		private SpeechClient _speechClient;

		private SpeechClient.StreamingRecognizeStream _streamingRecognizeStream;
		//구글 api와 연결을 하는 클래스의 인스턴스 변수

		private AudioClip _workingClip;
		//현재 녹음 중인 오디오 클립

		private Coroutine _checkOnMicAndRunStreamRoutine;
		//마이크 체크 및 음성 인식 코루틴이 할당 될 변수

		private CancellationTokenSource _cancellationToken;

		private float _recordingTime;
		//현재 녹음된 시간이 저장되는 변수

		private int _currentSamplePosition;
		//현재 마이크 샘플링 위치

		private int _previousSamplePosition;
		//이전 마이크 샘플링 위치

		private float[] _currentAudioSamples;
		//현재 녹음된 오디오 샘플

		private List<byte> _currentRecordedSamples;
		//현재 녹음 된 데이터가 저장되는 변수

		private Config.RecognitionSettings _currentRecognitionSettings;
		//음성 인식 설정과 관련된 변수

		private bool _initialized;
		//초기화 여부 확인 변수

		private bool _recognition;
		//음성 인식이 활성화 되어 있는지 여부 확인 변수

		public Config config;
		//음성 인식 설정 값이 저장되어 있는 Config타입의 변수

		[ReadOnly]
		public bool isRecording;
		//현재 녹음이 진행되고 있는지 아닌지 확인할 수 있는 변수

		[ReadOnly]
		public string microphoneDevice;
		//현재 연결된 마이크 디바이스 이름이 저장되는 변수

		private void Awake() //유니티가 처음 실행되면 실행되는 생명주기 메소드(Awake 메소드)
		{
			if(Instance != null) //해당 스크립트의 인스턴트가 이미 존재하는지 확인
			{
				Destroy(gameObject); //만약 이미 존재 한다면 해당 스크립트가 포함된 오브젝트를 파괴하고 스크립트 실행이 중단(종료) 됨
				return;
            }

            Instance = this; //인스턴스에 해당 스크립트를 할당

            Assert.IsNotNull(config, "Config is requried to be added."); //config 변수가 null인지 Assert.IsNotNull을 통해 확인하고, 만약 config가 null이라면 게임이 종료됨(실행되지 않음) config뒤에 문구가 콘솔창에 나타남

			Initialize(); //Initialize 메소드(초기화) 호출
		}
		private async void OnDestroy() //해당 스크립트가 포함된 오브젝트가 파괴될 때 실행되는 메소드(OnDestroy 메소드), async를 활용해 비동기 처리
		{
			if (Instance != this || !_initialized)
				return;
			//인스턴스가 현재 오브젝트 이거나 초기화 여부를 확인하고 초기화 되지 않았다면 return

			await StopStreamingRecognition(); //StopStreamingRecognition을 비동기로 실행, 데이터가 모두 전송될 때 까지 기다린다

			Instance = null;
			//오브젝트가 파괴되었으므로 인스턴스를 null로 설정해 다른 스크립트에서 접근하지 못하게 함
		}
		private async void Update() //매 프레임마다 실행되는 메소드(Update 메소드), async를 활용해 비동기 처리
		{
			if (Instance != this || !_initialized) 
				return;
			//인스턴스가 현재 오브젝트 이거나 초기화 여부를 확인하고 초기화 되지 않았다면 return

			if (!isRecording)
				return;
			//녹음 중이지 않다면 return

			_recordingTime += Time.unscaledDeltaTime;
			//현재 녹음되고 있는 시간을 계속해서 증가시킨다.

			if(_recordingTime >= StreamingRecognitionTimeLimit) //현재 녹음되고 있는 시간이 최대 녹음시간을 넘을 경우
			{
				await RestartStreamingRecognitionAfterLimit(); //음성 인식 재시작 메소드
				_recordingTime = 0; //녹음된 시간을 0으로 초기화
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
		public void SetMicrophoneDevice(string deviceName)//녹음에 사용될 마이크를 선택하는 메소드
		{
			if (isRecording)//현재 녹음 중이라면 return
				return;

			microphoneDevice = deviceName;
		}
		public string[] GetMicrophoneDevices()
		{
			return CustomMicrophone.devices;
		}
		public bool HasConnectedMicrophoneDevices()//연결 된 마이크가 있는지 확인하는 메소드
		{
			return CustomMicrophone.HasConnectedMicrophoneDevices();
		}
		public void StartStreamingRecognition(Config.RecognitionSettings recognitionSettings)//음성 인식 시작 메소드
		{
			if(!_initialized)//초기화 여부 확인
			{
				StreamingRecognitionFailedEvent?.Invoke("Failed to start recogntion due to: 'Not initialized'");
				return;
			}
			//초기화 되지 않았다면 return

            _currentRecognitionSettings = recognitionSettings; //입력으로 받은 recognitionSettings(세팅 값)을 현재 음성 인식 세팅 값으로 입력
			_checkOnMicAndRunStreamRoutine = StartCoroutine(CheckOnMicrophoneAndRunStream());
		}
		public async Task StopStreamingRecognition()//음성 인식 중단 메소드
		{
			if (!isRecording || !_recognition) //녹음, 음성 인식 활성화 여부 확인
				return; //활성화 되어 있지 않으면 return

			_recognition = false; //음성 인식 여부 false

			StopRecording(); //녹음 중지 메소드 실행

			if (_streamingRecognizeStream != null)
			{
				await _streamingRecognizeStream.WriteCompleteAsync(); //구글 api에서 음성 인식 완료되도록 하는 메소드가 실행될 때 까지 기다린다.
			}
		}
		private void FinishCleaningAfterStreamingRecognition() //음성 인식이 끝난 후 실행되는 메소드
        {
            _streamingRecognizeStream = null; //구글 api 변수 null로 설정해 연결 해제
            _currentRecordedSamples = null; //현재 녹음 된 데이터 null로 설정해 연결 해제

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
		private async Task RestartStreamingRecognitionAfterLimit() //음성 인식 재시작 메소드
		{
			await StopStreamingRecognition(); //음성 인식 중단 메소드가 실행될 때 까지 기다림

			_checkOnMicAndRunStreamRoutine = StartCoroutine(CheckOnMicrophoneAndRunStream()); //마이크 체크 및 음성 인식 코루틴이 할당 됨
		}
		private IEnumerator CheckOnMicrophoneAndRunStream()
		{
			while (!HasConnectedMicrophoneDevices()) //연결된 마이크가 있는지 확인
			{
				RequestMicrophonePermission(); //마이크 권한을 요청
				yield return null; //한 프레임 기다린 후 루프 반복
			}

			RunStreamingRecognition(); //음성 인식 시작 메소드 실행(마이크 연결이 되었을 때)

			_checkOnMicAndRunStreamRoutine = null; //마이크 체크 및 음성 인식 코루틴 null로 초기화
		}
		private async void RunStreamingRecognition() //음성 인식 시작
		{
			if (isRecording) //녹음 중이면 return
			{
				StreamingRecognitionFailedEvent?.Invoke("Already recording");
				return;
			}

			if (!StartRecording()) //녹음 시작 bool 타입 메소드에서 설정된 마이크가 없어 false가 반환되었다면 return
			{
				StreamingRecognitionFailedEvent?.Invoke("Cannot start recording");
				return;
			}

			_streamingRecognizeStream = _speechClient.StreamingRecognize();

			var recognitionConfig = new RecognitionConfig() //음성 인식 설정(_currentRecognitionSettings가 StartStreamingRecognition에서 초기화 되었음)
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
		private bool StartRecording() //녹음 시작 bool 타입 메소드
		{
			if (string.IsNullOrEmpty(microphoneDevice)) //설정된 마이크가 없다면 return false
				return false;

			_workingClip = CustomMicrophone.Start(microphoneDevice, true, 3, SampleRate); //녹음 시작

			_currentAudioSamples = new float[_workingClip.samples]; //float형 배열 생성 [_workingClip.samples(녹음된 오디오 클립의 크기가 배열의 크기가 된다)], 생성된 float형 배열이 현재 녹음된 오디오 샘플 배열에 할당

			_currentRecordedSamples = new List<byte>(); //음성 데이터를 text로 변환시키려면 음성 데이터를 byte로 변환해야 하는데 그 과정에서 사용할 byte형 오디오 데이터

			isRecording = true;

			return true; //녹음 시작 되었다는 true return
		}
		private void StopRecording() //녹음 중지 메소드
		{
			if (!isRecording) //녹음 중이 아니면 return
				return;

			if (string.IsNullOrEmpty(microphoneDevice)) //마이크가 할당되지 않았다면 return
				return;

			CustomMicrophone.End(microphoneDevice);

			MonoBehaviour.Destroy(_workingClip);

			_currentRecordedSamples.Clear(); //현재 녹음 된 데이터 초기화

			isRecording = false; //녹음 여부 false
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

			_currentSamplePosition = CustomMicrophone.GetPosition(microphoneDevice); //현재 녹음되고 있는 위치 할당

			if (CustomMicrophone.GetRawData(ref _currentAudioSamples, _workingClip)) //_workingClip(현재 녹음 중인 오디오 클립), _currentAudioSamples(현재 녹음된 오디오 샘플 float형 배열)
			{
				if (_previousSamplePosition > _currentSamplePosition) //이전 마이크 샘플링 위치가 현재 마이크 샘플링 위치보다 크다면
				{
					for (int i = _previousSamplePosition; i < _currentAudioSamples.Length; i++) //이전 마이크 샘플링 위치가 더 크다면 이전 위치(_previousSamplePosition) 부터 _currentAudioSamples.Length까지
					{
						_currentRecordedSamples.AddRange(FloatToBytes(_currentAudioSamples[i])); //_currentAudioSamples(현재 녹음된 오디오 샘플 float형 배열)에서 for문에서 i에 해당하는 위치의 데이터를 byte로 변환하고
																								 //_currentRecordedSamples(현재 녹음된 데이터가 저장되는 변수)에 추가
					}
					_previousSamplePosition = 0; //이전 마이크 샘플링 위치를 0으로 초기화 한 후
				}

				for (int i = _previousSamplePosition; i < _currentSamplePosition; i++) //0(처음)부터 현재 마이크 샘플링 위치까지 
				{
					_currentRecordedSamples.AddRange(FloatToBytes(_currentAudioSamples[i]));
				}

				_previousSamplePosition = _currentSamplePosition; //이전 샘플링 위치에 현재 샘플링 위치를 할당
			}
			//해당 메소드의 역할은 기존의 오디오 샘플(_currentAudioSamples, float형)을 _currentRecordedSamples(byte형 리스트)에 데이터 변환 후 추가 하는 역할을 한다.
			//Update 메소드에서 매 프레임 마다 실행 됨
		}
		private async void WriteDataToStream() //데이터를 구글 api 서버로 전송하는 역할
		{
			if (_streamingRecognizeStream == null) //구글 api와 연결 되었는지 여부
				return;

			ByteString chunk;
			List<byte> samplesChunk = null;

			if (isRecording || (_currentRecordedSamples != null && _currentRecordedSamples.Count > 0)) //녹음 중이거나 _currentRecordedSamples(byte형 리스트)가 비어 있지 않다면 실행되는 코드
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
		private void Initialize() //초기화 함수
		{
			string credentialJson;

            if (config.googleCredentialLoadFromResources)
            {
                if (string.IsNullOrEmpty(config.googleCredentialFilePath) || string.IsNullOrWhiteSpace(config.googleCredentialFilePath)) //구글 api 사용할 때 필요한 key 파일 내에서 데이터가 없다면 실행되는 코드
                {
					Debug.LogException(new Exception("The googleCredentialFilePath is empty. Please fill path to file.")); //해당 파일이 비어있으니 경로를 채우라는 문구 콘솔에 출력
					return;
                }

				TextAsset textAsset = Resources.Load<TextAsset>(config.googleCredentialFilePath); //key 파일(json형식의)에서 api key 추출 

				if(textAsset == null) //api key가 추출이 안되었다면 실행되는 코드
                {
					Debug.LogException(new Exception($"Couldn't load file: {config.googleCredentialFilePath} .")); //데이터 추출 안되었다는 문구 콘솔에 출력
					return;
				}

				credentialJson = textAsset.text; //textAsset데이터에서 text 출력
			}
            else
            {
				credentialJson = config.googleCredentialJson;
			}

			if (string.IsNullOrEmpty(credentialJson) || string.IsNullOrWhiteSpace(credentialJson)) //구글 api key가 비어있다면 실행되는 코드
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

		private byte[] FloatToBytes(float sample) //음성 데이터를 byte형식으로 변환하는 코드
		{
			return System.BitConverter.GetBytes((short)(sample * 32767));
		}
	}
}