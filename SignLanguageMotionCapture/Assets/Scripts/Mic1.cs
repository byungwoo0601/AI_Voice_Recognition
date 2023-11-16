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

public class Mic1 : MonoBehaviour
{
    private string microphoneID1 = null;
    private AudioClip _recording1 = null;
    private int recordingLengthSec1 = 20;
    private int recordingHZ1 = 44100;
    private byte[] byteData1;
    private string kor = "Kor";
    private string _text1;
    void Start()
    {
        microphoneID1 = Microphone.devices[0];
    }
    public void StartRec1()
    {
        Debug.Log("start recording");
        _recording1 = Microphone.Start(microphoneID1, false, recordingLengthSec1, recordingHZ1);
    }
    public void StopRec1()
    {
        if (Microphone.IsRecording(microphoneID1))
        {
            Microphone.End(microphoneID1);

            Debug.Log("stop recording");

            if (_recording1 == null)
            {
                Debug.Log("nothing recorded");
                return;
            }
        }
        Debug.Log(_recording1.length);

        byteData1 = AudioClipToByteArray(_recording1);

        PrintByteArraySize(byteData1);
    }
    public void showSTT1()
    {
        //Debug.Log("Start showSTT1");
        //Debug.Log(SpeechToText(byteData1, kor));
        //Debug.Log("End showSTT1");
        StartCoroutine(SpeechToTextCoroutine(byteData1, kor));
    }
    private byte[] AudioClipToByteArray(AudioClip audioClip)
    {
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        byte[] byteArray = new byte[samples.Length * 4];

        Buffer.BlockCopy(samples, 0, byteArray, 0, byteArray.Length);

        return byteArray;
    }
    private string SpeechToText(byte[] fileData, string lang)
    {
        try
        {
            string url = $"https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang={lang}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-NCP-APIGW-API-KEY-ID", "zqhm1iqevf");
            request.Headers.Add("X-NCP-APIGW-API-KEY", "fDPo0KaYMaIGRVuZQFr7SHbbK8Iu7vCnrUvsrmGd");
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = fileData.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileData, 0, fileData.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return _text1 = reader.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SpeechToText function: {e.Message}");
            return null;
        }
    }
    private IEnumerator SpeechToTextCoroutine(byte[] fileData, string lang)
    {
        string url = $"https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang={lang}";
        UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(fileData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/octet-stream");
        request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "zqhm1iqevf");
        request.SetRequestHeader("X-NCP-APIGW-API-KEY", "fDPo0KaYMaIGRVuZQFr7SHbbK8Iu7vCnrUvsrmGd");

        yield return request.SendWebRequest();

        if (request == null)
        {
            Debug.LogError($"Error in SpeechToText function: {request.error}");
        }
        else
        {
            _text1 = request.downloadHandler.text;
            Debug.Log($"Result: {_text1}");
        }
    }
    private void PrintByteArraySize(byte[] byteArray)
    {
        Debug.Log($"Byte Array Size: {byteArray.Length} bytes");
    }
}