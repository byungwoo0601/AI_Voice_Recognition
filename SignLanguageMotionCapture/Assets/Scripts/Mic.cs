using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using UnityEngine.Networking;
using System.Text;
using System.IO;

public class Mic : MonoBehaviour
{
    [Serializable]
    public class VoiceRecognize
    {
        public string text;
    }
    string url = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor";

    AudioSource aud;
    private string microphoneID = null;
    private AudioClip _recording = null;
    private int recordingLengthSec = 60;
    private int recordingHZ = 44100;
    private byte[] byteData;
    public string _text;
    public Text text;

    void Start()
    {
        //APIExamSTT aPIExam = new APIExamSTT();
        //aPIExam.ExamSTT();
        aud = GetComponent<AudioSource>();
        microphoneID = Microphone.devices[0];
        Debug.Log(microphoneID);
    }
    public void StartRec()
    {
        Debug.Log("start recording");
        _recording = Microphone.Start(microphoneID, false, recordingLengthSec, recordingHZ);
    }

    public void StopRec()
    {
        if (Microphone.IsRecording(microphoneID))
        {
            int lastTime = Microphone.GetPosition(null);

            Microphone.End(microphoneID);

            float[] samples = new float[_recording.samples];

            _recording.GetData(samples, 0);

            float[] cutSamples = new float[lastTime];

            Array.Copy(samples, cutSamples, cutSamples.Length);

            _recording = AudioClip.Create("Recording", cutSamples.Length, 1, recordingHZ, false);

            _recording.SetData(cutSamples, 0);

            Debug.Log("stop recording");

            if (_recording == null)
            {
                Debug.Log("nothing recorded");
                return;
            }

            aud.clip = _recording;
        }
        Debug.Log(_recording.length);
        byteData = AudioClipToByte(_recording);
        return;
    }
    public void PlayRec()
    {
        Debug.Log("play recording");
        aud.Play();
    }
    private byte[] AudioClipToByte(AudioClip audioClip)
    {
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        byte[] byteArray = new byte[samples.Length * 4];

        for (int i = 0; i < samples.Length; i++)
        {
            float sample = Mathf.Clamp01(samples[i]);
            int sampleValue = Mathf.RoundToInt(sample * 65535);

            byteArray[i * 2] = (byte)(sampleValue & 0xFF);
            byteArray[i * 2 + 1] = (byte)((sampleValue >> 8) & 0xFF);
        }
        return byteArray;
    }
    public void showSTT()
    {
        if (_recording == null)
        {
            Debug.Log("nothing recorded");
            return;
        }
        StartCoroutine(PostVoice(url, byteData));
    }
    public void reClip()
    {
        AudioClip newClip = ByteArrayToAudioClip(byteData);
        aud.clip = newClip;
    }
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
            Debug.Log(message);
            VoiceRecognize voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(message);

            Debug.Log("Voice Server responded: " + voiceRecognize.text);
        }
    }
    private AudioClip ByteArrayToAudioClip(byte[] audioData)
    {
        float[] floatData = new float[audioData.Length / 2];

        for (int i = 0; i < floatData.Length; i++) 
        {
            floatData[i] = (float)BitConverter.ToInt16(audioData, i * 2) / 32768.0f;
        }

        AudioClip audioClip = AudioClip.Create("New_Recording", floatData.Length, 1, recordingHZ, false);
        audioClip.SetData(floatData, 0);

        return audioClip;
    }
}