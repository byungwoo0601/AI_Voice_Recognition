using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class NewRecord : MonoBehaviour
{
    [Serializable]
    public class VoiceRecognize
    {
        public string text;
    }
    public bool isRecording = false;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingLengthSec = 30;
    private int _recordingHZ = 44100;
    private const int BlockSize_16Bit = 2;
    public Text text;
    void Start()
    {
        _microphoneID = Microphone.devices[0];
    }
    public void Rec()
    {
        if(isRecording == false)
        {
            startRecording();
        }
        else if(isRecording)
        {
            stopRecording();
        }
        if (isRecording == false)
        {
            isRecording = true;
        }
        else
        {
            isRecording = false;
        }
    }
    public void startRecording()
    {
        Debug.Log("start recording");
        _recording = Microphone.Start(_microphoneID, false, _recordingLengthSec, _recordingHZ);
    }
	public void stopRecording()
	{
		if (Microphone.IsRecording(_microphoneID))
		{
			Microphone.End(_microphoneID);

			Debug.Log("stop recording");
			if (_recording == null)
			{
				Debug.LogError("nothing recorded");
				return;
			}
			// audio clip to byte array
			byte[] byteData = getByteFromAudioClip(_recording);

			// 녹음된 audioclip api 서버로 보냄
			StartCoroutine(PostVoice(url, byteData));
		}
		return;
	}
    private byte[] getByteFromAudioClip(AudioClip audioClip)
    {
        MemoryStream stream = new MemoryStream();
        const int headerSize = 44;
        ushort bitDepth = 16;

        int fileSize = audioClip.samples * BlockSize_16Bit + headerSize;

        // audio clip의 정보들을 file stream에 추가(링크 참고 함수 선언)
        WavtoByte.WriteFileHeader(ref stream, fileSize, audioClip);
        WavtoByte.WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
        WavtoByte.WriteFileData(ref stream, audioClip, bitDepth);

        // stream을 array형태로 바꿈
        byte[] bytes = stream.ToArray();

        return bytes;
    }
    string url = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor";

private IEnumerator PostVoice(string url, byte[] data)
    {
        // request 생성
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // 요청 헤더 설정
        request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "zqhm1iqevf");
        request.SetRequestHeader("X-NCP-APIGW-API-KEY", "fDPo0KaYMaIGRVuZQFr7SHbbK8Iu7vCnrUvsrmGd");
        request.SetRequestHeader("Content-Type", "application/octet-stream");

        // 바디에 처리과정을 거친 Audio Clip data를 실어줌
        request.uploadHandler = new UploadHandlerRaw(data);

        // 요청을 보낸 후 response를 받을 때까지 대기
        yield return request.SendWebRequest();

        // 만약 response가 비어있다면 error
        if (request == null)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // json 형태로 받음 {"text":"인식결과"}
            string message = request.downloadHandler.text;
            VoiceRecognize voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(message);

            Debug.Log("Voice Server responded: " + voiceRecognize.text);
            text.text = voiceRecognize.text;
            // Voice Server responded: 인식결과
        }
    }
}
