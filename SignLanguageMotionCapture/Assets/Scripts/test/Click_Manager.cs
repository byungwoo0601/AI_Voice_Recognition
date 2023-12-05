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
        //FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance._resultText = childText;
    }
    private void Start()
    {
        childText.text = string.Empty;
        //Debug.Log("새로운 오브젝트 생성됨");
    }
    public void OnPointerClick(PointerEventData data) //마우스 버튼 클릭이 완료 되었을 때(눌렀다 땠을 때)
    {
        if(FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.Clicked_Image != null)
        {
            FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.Clicked_Image.GetComponent<Image>().color = new Color32(143, 143, 143, 255);
        }
        FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.Clicked_Image = gameObject.GetComponent<Transform>().gameObject;
        FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.get_Text = FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.Clicked_Image.GetComponentInChildren<Text>().text;
        gameObject.GetComponent<Image>().color = new Color32(180, 180, 180, 255);
    }
    public void OnPointerEnter(PointerEventData data) //마우스 버튼이 해당 오브젝트 위에 올라갔을 때
    {
        if (gameObject.GetComponentInChildren<Text>().text == string.Empty)
        {
            return;
        }
        gameObject.GetComponent<Image>().color = new Color32(180, 180, 180, 255);
    }
    public void OnPointerExit(PointerEventData data) //마우스 버튼이 해당 오브젝트 위에서 나갈 때 작동
    {
        if (gameObject.GetComponentInChildren<Text>().text == string.Empty)
        {
            return;
        }
        if(FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition.Examples.STS_Manager.instance.Clicked_Image == gameObject.GetComponent<Transform>().gameObject)
        {
            return;
        }
        gameObject.GetComponent<Image>().color = new Color32(143, 143, 143, 255);
    }
}
