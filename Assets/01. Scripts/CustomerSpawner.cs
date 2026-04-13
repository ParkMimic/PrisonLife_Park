using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    [Header("손님 생성")]
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 5f;
    public int maxQueueCount = 5;

    [Header("줄 서기 설정")]
    public Transform queueStartPosition;
    public float queueSpacing = 1.5f;
    public Vector3 queueDirection = new Vector3(0f, 0f, -1f);

    [Header("감옥 이동 경로")]
    public Transform[] waypoints;  // 카운터 → 감옥까지 경유 지점

    [Header("감옥 참조")]
    public Prison prison;

    private List<Customer> queue = new List<Customer>();

    void Start()
    {
        if (prison == null)
            Debug.LogError("[CustomerSpawner] Prison이 연결되지 않았습니다!");

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
            Debug.LogError("[CustomerSpawner] customerPrefab이 없습니다!");
            return;
        }

        Vector3 spawnPos = spawnPoint.position;
        GameObject obj = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
        Customer customer = obj.GetComponent<Customer>();

        if (customer == null)
        {
            Debug.LogError("[CustomerSpawner] Customer.cs가 없습니다!");
            return;
        }

        customer.Init(this);
        queue.Add(customer);
        UpdateQueuePositions();
    }

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

    public Transform[] GetWaypoints() => waypoints;

    public Prison GetPrison() => prison;
}
