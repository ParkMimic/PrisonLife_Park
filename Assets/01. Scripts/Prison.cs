using UnityEngine;
using System.Collections.Generic;

public class Prison : MonoBehaviour
{
    [Header("감옥 설정")]
    public int maxCapacity = 20;

    [Header("배치 설정")]
    public Transform originPoint;   // 첫 번째 죄수 위치 기준점 (미설정 시 Prison 위치 사용)
    public int   columnsPerRow = 4; // 한 줄에 세울 인원 수
    public float spacingX = 1.5f;   // 가로 간격
    public float spacingZ = 1.5f;   // 세로 간격

    [Header("정렬 방향")]
    public Vector3 prisonerFacing = Vector3.forward; // 수감된 죄수가 바라볼 방향

    private List<Customer> prisoners = new List<Customer>();

    public bool IsFull() => prisoners.Count >= maxCapacity;

    public int GetCount() => prisoners.Count;

    public Vector3 GetNextPosition()
    {
        int index = prisoners.Count;
        int col = index % columnsPerRow;
        int row = index / columnsPerRow;

        Vector3 origin = originPoint != null ? originPoint.position : transform.position;
        return origin + new Vector3(col * spacingX, 0f, row * spacingZ);
    }

    public void AddPrisoner(Customer customer)
    {
        if (IsFull())
        {
            Debug.Log("[Prison] 감옥이 꽉 찼어!");
            return;
        }

        prisoners.Add(customer);
        GameManager.instance.AddPrisoner();

        // 도착 후 지정된 방향으로 회전
        if (prisonerFacing != Vector3.zero)
            customer.transform.rotation = Quaternion.LookRotation(prisonerFacing);

        Debug.Log($"[Prison] 수감 완료! 현재 {prisoners.Count}명");
    }
}
