using UnityEngine;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;

    [Header("거래 정보")]
    public int itemsRequired = 1;
    public int moneyReward = 1;

    private CustomerSpawner spawner;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isWaypointMoving = false; // MoveToPosition 실행 중 Update 이동 차단
    private Animator anim;

    [Header("진행 상태")]
    public int currentArrivedCount = 0;
    public int pendingCount = 0; // 날아가는 중인 아이템 수 (미도착)
    public bool isSatisfied = false;

    // 줄 서기 완료 + 미만족 + 비행 중 아이템 없을 때만 true
    public bool IsReady => !isMoving && !isSatisfied && pendingCount == 0;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Init(CustomerSpawner spawner)
    {
        this.spawner = spawner;
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
    }

    void Update()
    {
        anim.SetBool("isWalking", isMoving || isWaypointMoving);

        if (!isMoving || isWaypointMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

        Vector3 dir = (targetPosition - transform.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    // 아이템을 하나 받을 때마다 호출될 메서드
    public void AddDeliverCount(int amount)
    {
        if (isSatisfied) return;
        if (isMoving) return;  // 아직 줄 서는 중이면 수갑 수령 불가

        currentArrivedCount += amount;
        Debug.Log($"[Customer] 아이템 받음! 현재: {currentArrivedCount}/{itemsRequired}");

        // 다 채워졌는지 확인
        if (currentArrivedCount >= itemsRequired)
        {
            Satisfy();
        }
    }

    public void Satisfy()
    {
        if (isSatisfied) return;
        isSatisfied = true;

        StartCoroutine(GoToPrisonRoutine());
    }

    IEnumerator GoToPrisonRoutine()
    {
        // 큐를 즉시 갱신해 다음 손님이 바로 앞으로 이동
        spawner.OnCustomerLeave(this);

        yield return new WaitForSeconds(0.5f);

        Prison prison = spawner.GetPrison();
        if (prison == null)
        {
            Debug.LogError("[Customer] Prison이 연결되지 않았습니다!");
            yield break;
        }

        if (prison.IsFull())
        {
            Debug.Log("[Prison] 감옥이 꽉 찼어!");
            yield break;
        }

        // 웨이포인트 순서대로 이동
        foreach (Transform waypoint in spawner.GetWaypoints())
            yield return StartCoroutine(MoveToPosition(waypoint.position));

        // 최종 감옥 위치로 이동
        Vector3 prisonPos = prison.GetNextPosition();
        prison.AddPrisoner(this);
        yield return StartCoroutine(MoveToPosition(prisonPos));

        Debug.Log($"[Prison] 수감 완료! 현재 {prison.GetCount()}명");
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = false;          // Update 이동 차단
        isWaypointMoving = true;

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime);

            Vector3 dir = (target - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            yield return null;
        }

        transform.position = target;
        isWaypointMoving = false;
    }
}
