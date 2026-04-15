using UnityEngine;

public class IdleUI : MonoBehaviour
{
    [Header("참조")]
    public Joystick joystick;
    public GameObject idleUIRoot; // 비활성화 상태로 배치된 UI 부모

    [Header("설정")]
    public float idleThreshold = 3.0f; // 이 시간(초) 동안 조작 없으면 UI 활성화

    private float idleTimer = 0f;
    private bool isIdle = false;

    void Start()
    {
        SetIdle(true);
    }

    void Update()
    {
        bool hasInput = joystick != null &&
                        (Mathf.Abs(joystick.Horizontal) > 0.01f ||
                         Mathf.Abs(joystick.Vertical)   > 0.01f);

        if (hasInput)
        {
            idleTimer = 0f;
            SetIdle(false);
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleThreshold)
                SetIdle(true);
        }
    }

    void SetIdle(bool idle)
    {
        if (isIdle == idle) return;
        isIdle = idle;
        if (idleUIRoot != null) idleUIRoot.SetActive(idle);
    }
}
