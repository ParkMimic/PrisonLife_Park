using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    [Header("손님 설정")]
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 5f;
    public int maxQueueCount = 5;

    [Header("줄 서기 설정")]
    public Transform queueStartPosition;  // 기준 위치 하나만 지정
    public float queueSpacing = 1.5f;     // 손님 간 간격
    public Vector3 queueDirection = new Vector3(0f, 0f, -1f); // 줄 서는 방향

    [Header("감옥 설정")]
    public Prison prison;

    private List<Customer> queue = new List<Customer>();

    void Start()
    {
        if (prison == null)
            Debug.LogError("[CustomerSpawner] Prison이 연결되지 않았어요!");

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (queue.Count < maxQueueCount && !prison.IsFull())
                SpawnCustomer();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnCustomer()
    {
        if (customerPrefab == null)
        {
            Debug.LogError("[CustomerSpawner] customerPrefab이 없어요!");
            return;
        }

        Vector3 spawnPos = spawnPoint.position + Vector3.up * 1f;

        GameObject obj = Instantiate(
            customerPrefab, spawnPos, Quaternion.identity);

        Customer customer = obj.GetComponent<Customer>();

        if (customer == null)
        {
            Debug.LogError("[CustomerSpawner] Customer.cs가 없어요!");
            return;
        }

        customer.Init(this);
        queue.Add(customer);
        UpdateQueuePositions();
    }

    // 기준 위치에서 queueDirection 방향으로 자동 배치
    public void UpdateQueuePositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            Vector3 targetPos = queueStartPosition.position
                + queueDirection.normalized * (queueSpacing * i);

            queue[i].MoveTo(targetPos);
        }
    }

    public Customer GetFirstCustomer()
    {
        if (queue.Count == 0) return null;
        return queue[0];
    }

    public void OnCustomerLeave(Customer customer)
    {
        queue.Remove(customer);
        UpdateQueuePositions();
    }

    public Prison GetPrison() => prison;
}