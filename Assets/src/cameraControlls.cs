using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlls : MonoBehaviour
{
   private Vector3 camPos;
   private Vector2 camRot;
   private Vector3 velocity;
   private bool freeMove = true;
   private bool grounded = false;
   public float cameraSpeed = 1F;
   public float cameraSensitivity = 10F;
   public float jumpVelocity = 10;
   public float gravity = -10;

   void Start()
   {
      camPos = this.transform.position;
   }

   private void OnTriggerEnter(Collider other)
   {
      //Check for a match with the specified name on any GameObject that collides with your GameObject
      if (other.gameObject.name == "BaseMesh")
      {
         grounded = true;
         Debug.Log("grunded");
      }
   }

   private void OnTriggerExit(Collider other)
   {
      //Check for a match with the specified name on any GameObject that collides with your GameObject
      if (other.gameObject.name == "BaseMesh")
      {
         grounded = false;
         Debug.Log("un grunded");
      }
   }

   private void OnTriggerStay(Collider other)
   {
      //Check for a match with the specified name on any GameObject that collides with your GameObject
      if (other.gameObject.name == "BaseMesh")
      {
         grounded = false;
         Debug.Log("un grunded");
      }
   }

   void Update()
   {
      if (Input.GetKey(KeyCode.W))
      {
         this.transform.position += new Vector3(this.transform.forward.x, 0, this.transform.forward.z)*cameraSpeed*Time.deltaTime;
      }
      if (Input.GetKey(KeyCode.S))
      {
         this.transform.position -= new Vector3(this.transform.forward.x, 0, this.transform.forward.z)*cameraSpeed*Time.deltaTime;
      }
      if (Input.GetKey(KeyCode.A))
      {
         this.transform.position -= this.transform.right*cameraSpeed*Time.deltaTime;
      }
      if (Input.GetKey(KeyCode.D))
      {
         this.transform.position += this.transform.right*cameraSpeed*Time.deltaTime;
      }

      if (Input.GetKeyUp(KeyCode.F))
      {
         freeMove = !freeMove;
      }

      if (freeMove)
      {
         if (Input.GetKey(KeyCode.Space))
         {
            this.transform.position += new Vector3(0, this.transform.up.y, 0)*cameraSpeed*Time.deltaTime;
         }
         if (Input.GetKey(KeyCode.LeftShift))
         {
            this.transform.position -= new Vector3(0, this.transform.up.y, 0)*cameraSpeed*Time.deltaTime;
         }
      } else {
         if (Input.GetKey(KeyCode.Space) && grounded)
         {
            velocity = new Vector3(0, jumpVelocity, 0);
         }
      }

      if (!grounded && !freeMove)
      {
         velocity += new Vector3(0, gravity*Time.deltaTime, 0);
         this.transform.position += velocity*Time.deltaTime;
      }

      camRot.x += Input.GetAxis("Mouse X") * cameraSensitivity;
      camRot.y += Input.GetAxis("Mouse Y") * cameraSensitivity;

      this.transform.localRotation = Quaternion.Euler(-camRot.y, camRot.x, 0);
   }
}