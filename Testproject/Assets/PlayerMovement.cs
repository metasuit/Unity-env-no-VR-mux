using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform teleportHuman;
    public Transform teleportModel;

    public float teleportDelay = 1.0f;
    private float lastTeleportTime;
    bool atHuman = true;
   
    public float speed = 12f;

    private void Start()
    {
        lastTeleportTime = 0.0f;
    }
    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right *  x + transform.forward * z;
        Vector3 upAndDown = new Vector3(0, speed/2 * Time.deltaTime);
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space))
        {
            controller.Move(upAndDown);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            controller.Move(-upAndDown);
        }

        if(Input.GetKey(KeyCode.Tab))
        {
            if(Time.time - lastTeleportTime >= teleportDelay)
            {
                if (atHuman == false)
                {
                    controller.Move(teleportHuman.position - transform.position);
                    atHuman = true;
                }
                else
                {
                    controller.Move(teleportModel.position - transform.position);
                    atHuman = false;
                }
                lastTeleportTime = Time.time;
            }
            

        }
        
    }
}
