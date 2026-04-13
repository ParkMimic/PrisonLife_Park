using UnityEngine;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    public float prisonMoveSpeed = 4f;

    private CustomerSpawner spawner;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isSatisfied = false;

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

    public void Satisfy()
    {
        if (isSatisfied) return;
        isSatisfied = true;

        StartCoroutine(GoToPrisonRoutine());
    }

    IEnumerator GoToPrisonRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        // 줄에서 제거 → 뒷 죄수들 앞으로 이동
        spawner.OnCustomerLeave(this);

        // 감옥이 가득 찼는지 확인
        Prison prison = spawner.GetPrison();
        if (prison == null)
        {
            Debug.LogError("[Customer] Prison이 연결되지 않았어요!");
            yield break;
        }

        if (prison.IsFull())
        {
            Debug.Log("[Prison] 감옥이 가득 찼어요!");
            yield break;
        }

        // 감옥 내 다음 위치로 이동
        Vector3 prisonPos = prison.GetNextPosition();
        prison.AddPrisoner(this);

        while (Vector3.Distance(transform.position, prisonPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                prisonPos,
                prisonMoveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = prisonPos;

        Debug.Log($"[Prison] 죄수 입감 완료! 현재 {prison.GetCount()}명");
    }
}