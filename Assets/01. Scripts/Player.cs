using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    [Header("에셋 조이스틱 연결")]
    public Joystick joystick;
    public float moveSpeed;

    Rigidbody rigid;
    Camera mainCam;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        mainCam = Camera.main;
    }

    private void FixedUpdate()
    {
        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f) return;

        // 카메라의 forward/right를 기준으로 방향 계산
        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;

        // Y축 성분 제거
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 조이스틱 입력을 카메라 기준 월드 방향으로 전환
        Vector3 moveDir = camForward * v + camRight * h;

        // 이동
        rigid.MovePosition(rigid.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // 이동 방향으로 캐릭터 회전
        transform.rotation = Quaternion.LookRotation(moveDir);
    }
}
