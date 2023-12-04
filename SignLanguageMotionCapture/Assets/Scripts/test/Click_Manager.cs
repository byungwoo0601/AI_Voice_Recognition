using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Click_Manager : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler,IPointerEnterHandler,IPointerMoveHandler
{
    public void OnPointerDown(PointerEventData data) //마우스 버튼 눌렀을 때 그 시점
    {
        Debug.Log("Down");
    }
    public void OnPointerUp(PointerEventData data) //마우스 버튼 땠을 때 그 시점
    {
        Debug.Log("Up");
    }
    public void OnPointerClick(PointerEventData data) //마우스 버튼 클릭이 완료 되었을 때(눌렀다 땠을 때)
    {
        Debug.Log("Click");
    }
    public void OnPointerEnter(PointerEventData data) //마우스 버튼이 해당 오브젝트 위에 올라갔을 때
    {
        Debug.Log("Enter");
    }
    public void OnPointerMove(PointerEventData data) //마우스 버튼이 해당 오브젝트 위에서 움직일 때 계속 작동
    {
        Debug.Log("Move");
    }
}
