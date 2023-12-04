using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Text_Count : MonoBehaviour
{
    public static Text_Count instance;
    void textCountUp()
    {
        FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.count++;
    }
}
