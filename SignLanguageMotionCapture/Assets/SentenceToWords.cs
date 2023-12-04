using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class SentenceToWords : MonoBehaviour
{
    Text ResultText;

    private void Awake()
    {
        ResultText = GetComponent<Text>();
    }

    public void Extraction()
    {

        Debug.Log(ResultText.text);

        List<string> words = SplitIntoWords(ResultText.text);
        foreach (string word in words)
        {
            Debug.Log(word);
        }
    }


    public static List<string> SplitIntoWords(string input)
    {
        List<string> wordList = new List<string>();
        foreach (Match match in Regex.Matches(input, @"\b[\w°¡-ÆR']*\b"))
        {
            if (!string.IsNullOrEmpty(match.Value))
            {
                wordList.Add(match.Value);
            }
        }
        return wordList;
    }

}