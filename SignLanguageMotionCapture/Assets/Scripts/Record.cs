using UnityEngine;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Collections;

public class Record : MonoBehaviour
{
    AudioClip record;
    AudioSource aud;
    private string microphoneID = null;
    private string path;
    private string speechToText;
    private int recordingLengthSec1 = 30;
    private int recordingHZ1 = 44100;
    private float timer = 0f;
    private bool timerRunning = false;
    public Text text;
    public Text timerText;
    //public GameObject textScroll;

    void Start()
    {
        aud = GetComponent<AudioSource>();
        microphoneID = Microphone.devices[0];
        Debug.Log(microphoneID);
    }

    void Update()
    {
        if(timerRunning)
        {
            timer += Time.deltaTime;
            timerText.text = $"{timer:N2}";
        }
    }
    public void Rec()
    {
        if(timerRunning == false)
        {
            timer = 0f;
            Debug.Log("start recording");
            record = Microphone.Start(microphoneID, false, recordingLengthSec1, recordingHZ1);
            aud.clip = record;
        }
        else
        {
            if (Microphone.IsRecording(microphoneID))
            {
                Microphone.End(microphoneID);

                Debug.Log("stop recording");

                if (record == null)
                {
                    Debug.Log("nothing recorded");
                    return;
                }
                //SavWav.Save("C:\\Users\\Hojin\\Documents\\GitHub\\AI_Voice_Recognition\\SignLanguageMotionCapture\\Assets\\record", aud.clip);

                path = "Assets/Sample.wav";
                StartCoroutine(STT(path));
                text.text = speechToText;
                //textScroll.SetActive(true);
            }
        }
        if(timerRunning == false)
        {
            timerRunning = true;
        }
        else
        {
            timerRunning = false;
        }
    }

    public void startRec()
    {
        timer = 0f;
        timerRunning = true;
        Debug.Log("start recording");
        record = Microphone.Start(microphoneID, false, recordingLengthSec1, recordingHZ1);
        aud.clip = record;
    }
    public void stopRec()
    {
        timerRunning = false;
        if (Microphone.IsRecording(microphoneID))
        {
            Microphone.End(microphoneID);

            Debug.Log("stop recording");

            if (record == null)
            {
                Debug.Log("nothing recorded");
                return;
            }
            SavWav.Save("Assets/record", aud.clip);

            path = "Assets/record.wav";
            StartCoroutine(STT(path));
        }
    }
    public void testRec()
    {
        path = "Assets/record.wav";
        StartCoroutine(STT(path));
    }
    private IEnumerator STT(string filePath)
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
        speechToText = matches[1].Value.Trim('"');

        yield return null;
    }

}