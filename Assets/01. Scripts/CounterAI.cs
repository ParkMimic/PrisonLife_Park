using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 카운터 AI: 수갑 스폰 지점 → 카운터 → 죄수에게 전달을 반복한다.
/// </summary>
public class CounterAI : MonoBehaviour
{
    [Header("참조 (소환 시 자동 설정 or Inspector 직접 연결)")]
    public ConverterProcessor converterProcessor;
    public CustomerSpawner customerSpawner;
    public MoneyPickupZone moneyPickupZone;

    [Header("이동 위치")]
    public Transform pickupPoint;   // 수갑 스폰 지점 근처
    public Transform counterPoint;  // 카운터 앞 대기 위치

    [Header("설정")]
    public int   maxCarry        = 5;    // 한 번에 최대 운반 개수
    public float deliverInterval = 0.3f; // 아이템 전달 간격
    public float searchInterval  = 0.5f; // 아이템 없을 때 재탐색 간격
    public float arrivalDistance = 1.0f; // 목적지 도달 판정 거리

    private NavMeshAgent agent;
    private List<ResultItem> carriedItems = new List<ResultItem>();

    // ── 초기화 ────────────────────────────────────────────────

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Init(Transform pickup, Transform counter)
    {
        pickupPoint  = pickup;
        counterPoint = counter;
    }

    private void Start()
    {
        if (converterProcessor == null) converterProcessor = FindFirstObjectByType<ConverterProcessor>();
        if (customerSpawner    == null) customerSpawner    = FindFirstObjectByType<CustomerSpawner>();
        if (moneyPickupZone    == null) moneyPickupZone    = FindFirstObjectByType<MoneyPickupZone>();

        StartCoroutine(CounterLoop());
    }

    // ── 메인 루프 ─────────────────────────────────────────────

    IEnumerator CounterLoop()
    {
        while (true)
        {
            // 1. 수갑이 생길 때까지 대기
            if (!converterProcessor.HasResultItems())
            {
                yield return new WaitForSeconds(searchInterval);
                continue;
            }

            // 2. 픽업 지점으로 이동
            yield return StartCoroutine(MoveTo(pickupPoint.position));

            // 3. 아이템 수령 (최대 maxCarry개)
            int count = 0;
            while (converterProcessor.HasResultItems() && count < maxCarry)
            {
                ResultItem item = converterProcessor.TakeItem();
                if (item == null) break;

                // 운반 중에는 AI에 붙여서 숨김
                item.transform.SetParent(transform);
                item.gameObject.SetActive(false);
                carriedItems.Add(item);
                count++;
            }

            if (carriedItems.Count == 0)
            {
                yield return new WaitForSeconds(searchInterval);
                continue;
            }

            // 4. 카운터로 이동
            yield return StartCoroutine(MoveTo(counterPoint.position));

            // 5. 죄수에게 전달
            yield return StartCoroutine(DeliverRoutine());
        }
    }

    IEnumerator DeliverRoutine()
    {
        while (carriedItems.Count > 0)
        {
            Customer customer = customerSpawner.GetFirstCustomer();

            if (customer == null || customer.isSatisfied)
            {
                yield return new WaitForSeconds(searchInterval);
                continue;
            }

            ResultItem item = carriedItems[carriedItems.Count - 1];
            carriedItems.RemoveAt(carriedItems.Count - 1);

            item.transform.SetParent(null);
            item.gameObject.SetActive(true);

            Vector3 targetPos = customer.transform.position + Vector3.up * 1.5f;

            item.FlyTo(targetPos, () =>
            {
                if (item != null) Destroy(item.gameObject);

                customer.AddDeliverCount(1);

                if (customer.currentArrivedCount >= customer.itemsRequired)
                    moneyPickupZone?.SpawnMoney(customer.moneyReward);
            });

            yield return new WaitForSeconds(deliverInterval);
        }
    }

    // ── 이동 유틸 ─────────────────────────────────────────────

    IEnumerator MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);

        while (true)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= arrivalDistance)
                break;

            yield return null;
        }

        agent.isStopped = true;
    }
}
