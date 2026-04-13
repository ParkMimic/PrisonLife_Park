using UnityEngine;
using System.Collections.Generic;

public class Prison : MonoBehaviour
{
    [Header("감옥 설정")]
    public Transform[] prisonPositions;  // 감옥 내 수용 위치 배열
    public int maxCapacity = 20;         // 최대 수용 인원

    private List<Customer> prisoners = new List<Customer>();

    public bool IsFull() => prisoners.Count >= maxCapacity;

    public int GetCount() => prisoners.Count;

    // 죄수 수용
    public Vector3 GetNextPosition()
    {
        int index = prisoners.Count;

        if (index >= prisonPositions.Length)
        {
            Debug.LogError("[Prison] 수용 위치가 부족해요!");
            return transform.position;
        }

        return prisonPositions[index].position;
    }

    public void AddPrisoner(Customer customer)
    {
        if (IsFull())
        {
            Debug.Log("[Prison] 감옥이 가득 찼어요!");
            return;
        }

        prisoners.Add(customer);
        Debug.Log($"[Prison] 죄수 수용! 현재 {prisoners.Count}명");
    }
}