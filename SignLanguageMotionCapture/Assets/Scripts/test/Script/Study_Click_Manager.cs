using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor.Animations;

namespace FrostweepGames.Plugins.GoogleCloud.StreamingSpeechRecognition
{
    public class Study_Click_Manager : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static Study_Click_Manager instance;
        new AnimationClip animation;
        string[] splited_text;

        public Text childText;

        public void OnPointerClick(PointerEventData data) //���콺 ��ư Ŭ���� �Ϸ� �Ǿ��� ��(������ ���� ��)
        {
            if (gameObject.GetComponentInChildren<Text>().text == string.Empty)
            {
                return;
            }
            if (STS_Manager.instance.Clicked_Image != null)
            {
                STS_Manager.instance.Clicked_Image.GetComponent<Image>().color = new Color32(143, 143, 143, 255);
            }
            STS_Manager.instance.Clicked_Image = gameObject.transform.gameObject;
            splited_text = STS_Manager.instance.Clicked_Image.GetComponentInChildren<Text>().text.Split(',');
            for(int i = 0; i < splited_text.Length; i++)
            {
                if (Resources.Load<AnimationClip>(splited_text[i]))
                {
                    animation = Resources.Load<AnimationClip>(splited_text[i]);
                    Study_ClipCreation.instance.newState.motion = animation;
                    break;
                }
            }
            STS_Manager.instance.get_Text = STS_Manager.instance.Clicked_Image.GetComponentInChildren<Text>().text;
            gameObject.GetComponent<Image>().color = new Color32(180, 180, 180, 255);
            Study_ClipCreation.instance.AnimStart();
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
            if (STS_Manager.instance.Clicked_Image == transform.gameObject)
            {
                return;
            }
            gameObject.GetComponent<Image>().color = new Color32(143, 143, 143, 255);
        }
    }
}
