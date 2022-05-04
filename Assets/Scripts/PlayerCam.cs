using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    [Header ("Camera Sensitivity")]
    // Allows you to adjust the look sensitivity of the player camera
    public float sensX;
    public float sensY;

    public Transform orientation;
    //public Transform camHolder;

    // Gets references to the x and y rotations of the player
    float xRotation;
    float yRotation;

    private void Start()
    {
        // Locks the cursor to the center of the screee and disables the camera icon
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Takes in mouse input and sets it with delta time to avoid framerate buggyness.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamps the rotation so that the player can't look over 90 degress up or down
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        //camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    // External tool used to help with camera tilting during wallrunning
    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    // Small asset to control camera tilt when wall-running
    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}