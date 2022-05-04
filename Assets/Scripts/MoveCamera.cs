using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPostion;

    private void Update()
    {
        transform.position = cameraPostion.position;
    }
}
