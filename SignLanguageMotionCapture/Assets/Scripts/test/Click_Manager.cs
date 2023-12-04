using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Click_Manager : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler,IPointerEnterHandler,IPointerMoveHandler
{
    public void OnPointerDown(PointerEventData data) //���콺 ��ư ������ �� �� ����
    {
        Debug.Log("Down");
    }
    public void OnPointerUp(PointerEventData data) //���콺 ��ư ���� �� �� ����
    {
        Debug.Log("Up");
    }
    public void OnPointerClick(PointerEventData data) //���콺 ��ư Ŭ���� �Ϸ� �Ǿ��� ��(������ ���� ��)
    {
        Debug.Log("Click");
    }
    public void OnPointerEnter(PointerEventData data) //���콺 ��ư�� �ش� ������Ʈ ���� �ö��� ��
    {
        Debug.Log("Enter");
    }
    public void OnPointerMove(PointerEventData data) //���콺 ��ư�� �ش� ������Ʈ ������ ������ �� ��� �۵�
    {
        Debug.Log("Move");
    }
}
