using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 조이스틱 설정")]
    public Joystick joystick;
    public float moveSpeed;

    Animator anim;
    Rigidbody rigid;
    Camera mainCam;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        mainCam = Camera.main;
    }

    public bool controllable = true;

    private void FixedUpdate()
    {
        if (!controllable)
        {
            anim.SetFloat("speed", 0f);
            return;
        }

        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f)
        {
            anim.SetFloat("speed", 0f);
            return;
        }
        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;


        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * v + camRight * h;

        rigid.MovePosition(rigid.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        transform.rotation = Quaternion.LookRotation(moveDir);

        anim.SetFloat("speed", moveDir.magnitude);
    }
}
