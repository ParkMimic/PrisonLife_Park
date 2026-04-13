using UnityEngine;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    public float prisonMoveSpeed = 4f;

    [Header("거래 정보")]
    public int itemsRequired = 1;
    public int moneyReward = 1;

    private CustomerSpawner spawner;
    private Vector3 targetPosition;
    private bool isMoving = false;

    [Header("진행 상태")]
    public int currentArrivedCount = 0; // 현재까지 받은 아이템 수
    public bool isSatisfied = false; // 거래 만족 여부


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
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

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
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                prisonMoveSpeed * Time.deltaTime);

            // 이동 방향으로 회전
            Vector3 dir = (target - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            yield return null;
        }

        transform.position = target;
    }
}
