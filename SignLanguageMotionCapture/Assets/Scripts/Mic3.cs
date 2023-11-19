using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text.Json;

public class Mic3 : MonoBehaviour
{
    AudioClip record;
    AudioSource aud;
    private string microphoneID2 = null;
    private string path;
    private int recordingLengthSec1 = 30;
    private int recordingHZ1 = 44100;
    public Text text;

    void Start()
    {
        aud = GetComponent<AudioSource>();
        microphoneID2 = Microphone.devices[0];
        Debug.Log(microphoneID2);
    }

    public void Rec()
    {
        Debug.Log("start recording");
        record = Microphone.Start(microphoneID2, false, recordingLengthSec1, recordingHZ1);
        aud.clip = record;
    }
    public void stopRec()
    {
        if (Microphone.IsRecording(microphoneID2))
        {
            Microphone.End(microphoneID2);

            Debug.Log("stop recording");

            if (record == null)
            {
                Debug.Log("nothing recorded");
                return;
            }
            SavWav.Save("C:\\Users\\Hojin\\Documents\\GitHub\\AI_Voice_Recognition\\SignLanguageMotionCapture\\Assets\\record", aud.clip);

            path = "Assets/record.wav";
            APIExamSTT apiExam = new APIExamSTT();
            text.text = apiExam.ExamSTT(path);
        }
    }
    public void toWav()
    {
        SavWav.Save("C:\\Users\\Hojin\\Documents\\GitHub\\AI_Voice_Recognition\\SignLanguageMotionCapture\\Assets\\record", aud.clip);
    }
    public void toSTT()
    {
        path = "Assets/record.wav";
        APIExamSTT apiExam = new APIExamSTT();
        text.text = apiExam.ExamSTT(path);
    }
}
public class APIExamSTT
{
    public string ExamSTT(string filePath)
    {
        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        byte[] fileData = new byte[fs.Length];
        fs.Read(fileData, 0, fileData.Length);
        fs.Close();

        string lang = "Kor";    // 언어 코드 ( Kor, Jpn, Eng, Chn )
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
            requestStream.Close();
        }
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream stream = response.GetResponseStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        string _text = reader.ReadToEnd();
        stream.Close();
        response.Close();
        reader.Close();

        string pattern = "\"(\\\\\"|[^\"])*\"";
        MatchCollection matches = Regex.Matches(_text, pattern);
        string text = matches[1].Value.Trim('"');
        return text;
    }
}
