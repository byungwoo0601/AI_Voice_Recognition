using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Mic : MonoBehaviour
{
    AudioSource aud;
    private string microphoneID = null;
    private int recordingLengthSec = 100;
    private int recordingHZ = 44100;
    System.Diagnostics.Stopwatch watch;

    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
        microphoneID = Microphone.devices[0];
        Debug.Log(microphoneID);
    }
    public void StartRec()
    {
        watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        Debug.Log("start recording");
        aud.clip = Microphone.Start(microphoneID, false, recordingLengthSec, recordingHZ);
    }

    public void StopRec()
    {
        if (Microphone.IsRecording(microphoneID))
        {
            Microphone.End(microphoneID);
            watch.Stop();

            float time = watch.ElapsedMilliseconds / 1000f;
            Debug.Log(time + "second");

            Debug.Log("stop recording");
            TrimSilence(aud.clip, time);
            Debug.Log(aud.clip.length);
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
    private AudioClip TrimSilence(AudioClip clip, float endTime)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        int endIndex = (int)(endTime * clip.frequency) * clip.channels;

        int i;
        for (i = endIndex - 1; i>=0;i--)
        {
            if (Mathf.Abs(samples[i]) > 0.1f)
            {
                break;
            }
        }
        float[] trimmedSamples = new float[i + 1];
        Array.Copy(samples, trimmedSamples, i + 1);

        AudioClip trimmedClip = AudioClip.Create("trimmed", trimmedSamples.Length / clip.channels, clip.channels, clip.frequency, false);
        trimmedClip.SetData(trimmedSamples, 0);

        return trimmedClip;
    }
}
