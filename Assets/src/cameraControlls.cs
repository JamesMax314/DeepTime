using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
   private CharacterController controller;
   private Vector3 playerVelocity;
   private bool groundedPlayer;
   private Vector2 camRot;
   private bool freeMove = false;

   public float playerSpeed = 2.0f;
   public float jumpHeight = 1.0f;
   public float gravityValue = -9.81f;
   public float cameraSensitivity = 10F;
   public float freeMoveSpeed = 10F;

   private void Start()
   {
      controller = gameObject.AddComponent<CharacterController>();
      controller.minMoveDistance = 1e-5F;
   }

   void Update()
   {
      groundedPlayer = controller.isGrounded;

      if (Input.GetKeyUp(KeyCode.F))
      {
         freeMove = !freeMove;
      }

      camRot.x += Input.GetAxis("Mouse X") * cameraSensitivity;
      camRot.y += Input.GetAxis("Mouse Y") * cameraSensitivity;

      this.transform.localRotation = Quaternion.Euler(-camRot.y, camRot.x, 0);
      Quaternion lookMat = Quaternion.Euler(0, camRot.x, 0);

      if (freeMove)
      {
         Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
         if (Input.GetKey(KeyCode.LeftShift))
         {
            move.y = -1;
         } else if (Input.GetKey(KeyCode.Space))
         {
            move.y = 1;
         }
         controller.Move(lookMat * move * Time.deltaTime * freeMoveSpeed);
      } else
      {
         Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
         controller.Move(lookMat * move * Time.deltaTime * playerSpeed);

         if (Input.GetButtonDown("Jump") && groundedPlayer)
         {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
         }

         playerVelocity.y += gravityValue * Time.deltaTime;
         controller.Move(playerVelocity * Time.deltaTime);
      }
   }
}
