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

    // words º¯¼ö´Â publicÀ¸·Î º¯°æµÇ¾î ´Ù¸¥ ½ºÅ©¸³Æ®¿¡¼­ Á÷Á¢ Á¢±Ù °¡´É
    public static List<string> words;

    private void Awake()
    {
        ResultText = GetComponent<Text>();
    }

    public void Extraction()
    {
        // Debug.Log(ResultText.text);
        // Extraction ¸Þ¼­µå¿¡¼­ SplitIntoWords¿Í RemoveJosa¸¦ È£ÃâÇÏ¿© ´Ü¾î ÃßÃâ
        words = RemoveJosa(SplitIntoWords(ResultText.text));
    }

    // Á¤±Ô½ÄÀ» »ç¿ëÇÏ¿© ¹®ÀåÀ» ´Ü¾î·Î ³ª´©´Â ¸Þ¼­µå
    private List<string> SplitIntoWords(string input)
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

    // Á¶»ç¸¦ Á¦°ÅÇÏ´Â ¸Þ¼­µå
    private List<string> RemoveJosa(List<string> words)
    {
        List<string> josaList = new List<string>()
        {
            "Àº", "´Â", "ÀÌ", "°¡", "ÀÇ", "À»", "¸¦", "¿Í", "°ú", "À¸·Î", "·Î",
            "¿¡°Ô", "¿¡°Ô¼­", "¿¡¼­", "¿¡", "¿¡´Â", "À¸·Î´Â", "·Î´Â", "¿Í´Â", "°ú´Â"
            // ÇÊ¿äÇÑ Á¶»ç¸¦ Ãß°¡ÇÏ½Ç ¼ö ÀÖ½À´Ï´Ù.
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