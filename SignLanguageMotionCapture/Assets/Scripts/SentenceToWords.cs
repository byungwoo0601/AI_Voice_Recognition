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

    public static List<string> words;
    private void Awake()
    {
        ResultText = GetComponent<Text>();
    }

    public void Extraction()
    {
        //Debug.Log(ResultText.text);

        words = RemoveJosa(SplitIntoWords(ResultText.text));
        foreach (string word in words)
        {
            //Debug.Log(word);
        }
    }


    public List<string> SplitIntoWords(string input)
    {
        List<string> wordList = new List<string>();
        foreach (Match match in Regex.Matches(input, @"\b[\w��-�R']*\b"))
        {
            if (!string.IsNullOrEmpty(match.Value))
            {
                wordList.Add(match.Value);
            }
        }

        return wordList;
    }

    private List<string> RemoveJosa(List<string> words)
    {
        List<string> josaList = new List<string>()
    {
        "��", "��", "��", "��", "��", "��", "��", "��", "��", "����", "��",
        "����", "���Լ�", "����", "��", "����", "���δ�", "�δ�", "�ʹ�", "����"
        // �ʿ��� ���縦 �߰��Ͻ� �� �ֽ��ϴ�.
    };

        for (int i = 0; i < words.Count; i++)
        {
            foreach (var josa in josaList)
            {
                if (words[i].EndsWith(josa))
                {
                    words[i] = words[i].Substring(0, words[i].Length - josa.Length);
                    break;
                }
            }
        }

        return words;
    }




}