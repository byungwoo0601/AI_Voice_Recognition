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

    // words 변수는 public으로 변경되어 다른 스크립트에서 직접 접근 가능
    public static List<string> words;

    private void Awake()
    {
        ResultText = GetComponent<Text>();
    }

    public void Extraction()
    {
        // Debug.Log(ResultText.text);
        // Extraction 메서드에서 SplitIntoWords와 RemoveJosa를 호출하여 단어 추출
        words = RemoveJosa(SplitIntoWords(ResultText.text));
    }

    // 정규식을 사용하여 문장을 단어로 나누는 메서드
    private List<string> SplitIntoWords(string input)
    {
        List<string> wordList = new List<string>();
        foreach (Match match in Regex.Matches(input, @"\b[\w가-힣']*\b"))
        {
            if (!string.IsNullOrEmpty(match.Value))
            {
                wordList.Add(match.Value);
            }
        }
        return wordList;
    }

    // 조사를 제거하는 메서드
    private List<string> RemoveJosa(List<string> words)
    {
        List<string> josaList = new List<string>()
        {
            "은", "는", "이", "가", "의", "을", "를", "와", "과", "으로", "로",
            "에게", "에게서", "에서", "에", "에는", "으로는", "로는", "와는", "과는"
            // 필요한 조사를 추가하실 수 있습니다.
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