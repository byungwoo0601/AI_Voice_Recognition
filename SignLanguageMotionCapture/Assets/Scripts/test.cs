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
using System.Text.RegularExpressions;

public class test : MonoBehaviour
{
    private void Start()
    {
        Program pro = new Program();
        pro.Main();
    }
}
class Program
{
    public void Main()
    {
        string inputString = "{\"text\":\"This is a sample \\\"string\\\" with \\\"quoted\\\" data.\"}";

        // ����ǥ������ ����Ͽ� ū ����ǥ�� �ѷ����� ������ ����
        string pattern = "\"([^\"]*)\"";
        MatchCollection matches = Regex.Matches(inputString, pattern);

        // ����� ������ ���
        foreach (Match match in matches)
        {
            string extractedData = match.Groups[1].Value;
            Debug.Log(extractedData);
        }
    }
}
