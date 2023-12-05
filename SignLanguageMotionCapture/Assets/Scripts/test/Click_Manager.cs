using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Click_Manager : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public static Click_Manager instance;

    public Text childText;
    private void Awake()
    {
        instance = this;
        childText.text = string.Empty;
        FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance._resultText = childText;
    }
    public void OnPointerClick(PointerEventData data) //���콺 ��ư Ŭ���� �Ϸ� �Ǿ��� ��(������ ���� ��)
    {
        FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.Clicked_Image = gameObject.GetComponent<Transform>().gameObject;
        FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.get_Text = FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.Clicked_Image.GetComponentInChildren<Text>().text;
    }
    public void OnPointerEnter(PointerEventData data) //���콺 ��ư�� �ش� ������Ʈ ���� �ö��� ��
    {
        if (gameObject.GetComponentInChildren<Text>().text == string.Empty)
        {
            return;
        }
        gameObject.GetComponent<Image>().color = new Color32(180, 180, 180, 255);
    }
    public void OnPointerExit(PointerEventData data) //���콺 ��ư�� �ش� ������Ʈ ������ ���� �� �۵�
    {
        if (gameObject.GetComponentInChildren<Text>().text == string.Empty)
        {
            return;
        }
        gameObject.GetComponent<Image>().color = new Color32(143, 143, 143, 255);
    }
}
