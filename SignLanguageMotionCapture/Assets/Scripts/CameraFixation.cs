using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFixation : MonoBehaviour
{
    public Transform target; 
    public float distance = 2.3f;
    public float x = 0;
    public float y = 0.1f;
    public float z = -2.8f;

    void Update()
    {
        Vector3 newPosition = target.position - (target.forward * distance);
        transform.position = new Vector3(newPosition.x + x, newPosition.y + y, newPosition.z + z);

        // 캐릭터를 바라보도록 카메라를 회전시킵니다.
        transform.LookAt(target);
    }

}
