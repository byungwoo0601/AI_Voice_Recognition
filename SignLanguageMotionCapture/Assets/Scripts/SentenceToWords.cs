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
        ResultText = GetComponent<Text>();  // ÇöÀç ½ºÅ©¸³Æ®°¡ ¿¬°áµÈ °´Ã¼ÀÇ Text ÄÄÆ÷³ÍÆ®¿¡ ´ëÇÑ ÂüÁ¶¸¦ È¹µæ
    }

    public void Extraction()
    {
        // Extraction ¸Þ¼­µå¿¡¼­ SplitIntoWords¿Í RemoveJosa¸¦ È£ÃâÇÏ¿© ´Ü¾î ÃßÃâ
        words = RemoveJosa(SplitIntoWords(ResultText.text));
    }

    // Á¤±Ô½ÄÀ» »ç¿ëÇÏ¿© ¹®ÀåÀ» ´Ü¾î·Î ³ª´©´Â ¸Þ¼­µå
    private List<string> SplitIntoWords(string input)
    {
        List<string> wordList = new List<string>();

        // Á¤±Ô½Ä ÆÐÅÏÀ» »ç¿ëÇÏ¿© ¹®ÀåÀ» ´Ü¾î·Î ³ª´®
        foreach (Match match in Regex.Matches(input, @"\b[\w°¡-ÆR']*\b"))
        {
            if (!string.IsNullOrEmpty(match.Value))
            {
                wordList.Add(match.Value);
            }
        }
        return wordList;  // ´Ü¾î ¸ñ·Ï ¹ÝÈ¯
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

        // ¸ðµç ´Ü¾î¿¡ ´ëÇØ ¹Ýº¹
        for (int i = 0; i < words.Count; i++)
        {
            // °¢ ´Ü¾î¿¡ ´ëÇØ Á¤ÀÇµÈ Á¶»ç ¸®½ºÆ®¸¦ È®ÀÎ
            foreach (var josa in josaList)
            {
                // ´Ü¾î°¡ ÇØ´ç Á¶»ç·Î ³¡³ª¸é Á¶»ç¸¦ Á¦°ÅÇÏ°í ¹Ýº¹¹® Á¾·á
                if (words[i].EndsWith(josa))
                {
                    words[i] = words[i].Substring(0, words[i].Length - josa.Length);
                    break;
                }
            }
        }

        return words;  // Á¶»ç°¡ Á¦°ÅµÈ ´Ü¾î ¸ñ·Ï ¹ÝÈ¯
    }
}