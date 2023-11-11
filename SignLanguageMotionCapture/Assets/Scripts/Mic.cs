using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mic : MonoBehaviour
{
    AudioSource aud;
    private string microphoneID = null;
    private int recordingLengthSec = 20;
    private int recordingHZ = 44100;

    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
        microphoneID = Microphone.devices[0];
        Debug.Log(microphoneID);
    }
    public void StartRec()
    {
        Debug.Log("start recording");
        aud.clip = Microphone.Start(microphoneID, false, recordingLengthSec, recordingHZ);
    }

    public void StopRec()
    {
        if(Microphone.IsRecording(microphoneID))
        {
            Microphone.End(microphoneID);

            Debug.Log("stop recording");
            Debug.Log(aud.clip.length);
            float[] samples = new float[aud.clip.samples];
            aud.clip.GetData(samples, 0);
            float[] cutSamples = new float[0];
            //Array.Copy(samples, cutSamples, cutSamples.Length - 1);
            if (aud.clip == null)
            {
                Debug.Log("nothing recorded");
                return;
            }
        }
        return;
    }
    public void PlayRec()
    {
        Debug.Log("play recording");
        aud.Play();
    }
}
