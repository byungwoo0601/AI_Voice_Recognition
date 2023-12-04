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

			// ������ audioclip api ������ ����
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

        // audio clip�� �������� file stream�� �߰�(��ũ ���� �Լ� ����)
        WavtoByte.WriteFileHeader(ref stream, fileSize, audioClip);
        WavtoByte.WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
        WavtoByte.WriteFileData(ref stream, audioClip, bitDepth);

        // stream�� array���·� �ٲ�
        byte[] bytes = stream.ToArray();

        return bytes;
    }
    string url = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor";

private IEnumerator PostVoice(string url, byte[] data)
    {
        // request ����
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // ��û ��� ����
        request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "zqhm1iqevf");
        request.SetRequestHeader("X-NCP-APIGW-API-KEY", "fDPo0KaYMaIGRVuZQFr7SHbbK8Iu7vCnrUvsrmGd");
        request.SetRequestHeader("Content-Type", "application/octet-stream");

        // �ٵ� ó�������� ��ģ Audio Clip data�� �Ǿ���
        request.uploadHandler = new UploadHandlerRaw(data);

        // ��û�� ���� �� response�� ���� ������ ���
        yield return request.SendWebRequest();

        // ���� response�� ����ִٸ� error
        if (request == null)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // json ���·� ���� {"text":"�νİ��"}
            string message = request.downloadHandler.text;
            VoiceRecognize voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(message);

            Debug.Log("Voice Server responded: " + voiceRecognize.text);
            text.text = voiceRecognize.text;
            // Voice Server responded: �νİ��
        }
    }
}
