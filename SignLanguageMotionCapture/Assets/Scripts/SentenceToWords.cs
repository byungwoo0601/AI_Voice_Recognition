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

    // words ������ public���� ����Ǿ� �ٸ� ��ũ��Ʈ���� ���� ���� ����
    public static List<string> words;

    private void Awake()
    {
        ResultText = GetComponent<Text>();  // ���� ��ũ��Ʈ�� ����� ��ü�� Text ������Ʈ�� ���� ������ ȹ��
    }

    public void Extraction()
    {
        // Extraction �޼��忡�� SplitIntoWords�� RemoveJosa�� ȣ���Ͽ� �ܾ� ����
        words = RemoveJosa(SplitIntoWords(ResultText.text));
    }

    // ���Խ��� ����Ͽ� ������ �ܾ�� ������ �޼���
    private List<string> SplitIntoWords(string input)
    {
        List<string> wordList = new List<string>();

        // ���Խ� ������ ����Ͽ� ������ �ܾ�� ����
        foreach (Match match in Regex.Matches(input, @"\b[\w��-�R']*\b"))
        {
            if (!string.IsNullOrEmpty(match.Value))
            {
                wordList.Add(match.Value);
            }
        }
        return wordList;  // �ܾ� ��� ��ȯ
    }

    // ���縦 �����ϴ� �޼���
    private List<string> RemoveJosa(List<string> words)
    {
        List<string> josaList = new List<string>()
        {
            "��", "��", "��", "��", "��", "��", "��", "��", "��", "����", "��",
            "����", "���Լ�", "����", "��", "����", "���δ�", "�δ�", "�ʹ�", "����"
            // �ʿ��� ���縦 �߰��Ͻ� �� �ֽ��ϴ�.
        };

        // ��� �ܾ ���� �ݺ�
        for (int i = 0; i < words.Count; i++)
        {
            // �� �ܾ ���� ���ǵ� ���� ����Ʈ�� Ȯ��
            foreach (var josa in josaList)
            {
                // �ܾ �ش� ����� ������ ���縦 �����ϰ� �ݺ��� ����
                if (words[i].EndsWith(josa))
                {
                    words[i] = words[i].Substring(0, words[i].Length - josa.Length);
                    break;
                }
            }
        }

        return words;  // ���簡 ���ŵ� �ܾ� ��� ��ȯ
    }
}