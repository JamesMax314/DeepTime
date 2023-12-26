using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlls : MonoBehaviour
{
    private Vector3 camPos;
    private Vector2 camRot;
    public float cameraSpeed = 0.01F;
    public float cameraSensitivity = 10F;

    void Start()
    {
        camPos = this.transform.position;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
           this.transform.position += new Vector3(this.transform.forward.x, 0, this.transform.forward.z)*cameraSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
           this.transform.position -= new Vector3(this.transform.forward.x, 0, this.transform.forward.z)*cameraSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
           this.transform.position -= this.transform.right*cameraSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
           this.transform.position += this.transform.right*cameraSpeed;
        }
        if (Input.GetKey(KeyCode.Space))
        {
           this.transform.position += this.transform.up*cameraSpeed;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
           this.transform.position -= this.transform.up*cameraSpeed;
        }

        // this.transform.position = camPos;

        camRot.x += Input.GetAxis("Mouse X") * cameraSensitivity;
        camRot.y += Input.GetAxis("Mouse Y") * cameraSensitivity;

        this.transform.localRotation = Quaternion.Euler(-camRot.y, camRot.x, 0);
    }
}