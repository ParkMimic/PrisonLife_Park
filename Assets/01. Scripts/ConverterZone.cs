using UnityEngine;
using System.Collections;

public class ConverterZone : MonoBehaviour, IInteractable
{
    public enum ZoneMode { Converter, Counter }

    [Header("모드 선택")]
    public ZoneMode zoneMode;

    [Header("공통 설정")]
    public float insertInterval = 0.3f;

    [Header("Converter 모드 설정")]
    public ConverterDisplay display;
    public ConverterProcessor processor;

    [Header("Counter 모드 설정")]
    public CustomerSpawner customerSpawner;
    public MoneyPickupZone moneyPickupZone;
    public float nextCustomerDelay = 1f;

    private bool isProcessing = false;
    private bool playerInZone = false;

    public void OnPlayerEnter(PlayerInteraction player)
    {
        switch (zoneMode)
        {
            case ZoneMode.Converter:
                if (!isProcessing) OnEnterConverter(player);
                break;
            case ZoneMode.Counter:
                playerInZone = true;
                if (!isProcessing) StartCoroutine(CounterRoutine(player.ItemChain));
                break;
        }
    }

    public void OnPlayerExit(PlayerInteraction player)
    {
        playerInZone = false;
        StopAllCoroutines();
        isProcessing = false;
    }

    // ── Converter ────────────────────────────────────────────────

    void OnEnterConverter(PlayerInteraction player)
    {
        bool hasItems = processor.inputType == ConverterProcessor.InputType.Mineral
            ? player.ItemChain.GetCount() > 0
            : player.ItemChain.GetResultCount() > 0;

        if (!hasItems) return;

        if (display == null || processor == null)
        {
            Debug.LogError("[ConverterZone] display 또는 processor가 연결되지 않았습니다!");
            return;
        }

        StartCoroutine(ConverterRoutine(player.ItemChain));
    }

    IEnumerator ConverterRoutine(ItemChain itemChain)
    {
        isProcessing = true;

        while (true)
        {
            if (processor.inputType == ConverterProcessor.InputType.Mineral)
            {
                MineralItem item = itemChain.PopItem();
                if (item == null) break;

                Vector3 targetPos = display.GetNextPosition();
                item.FlyTo(targetPos, () =>
                {
                    display.AddMineral(item.gameObject);
                    processor.OnItemInserted();
                });
            }
            else
            {
                ResultItem item = itemChain.PopResultItem();
                if (item == null) break;

                Vector3 targetPos = display.GetNextPosition();
                item.FlyTo(targetPos, () =>
                {
                    display.AddMineral(item.gameObject);
                    processor.OnItemInserted();
                });
            }

            yield return new WaitForSeconds(insertInterval);
        }

        isProcessing = false;
    }

    // ── Counter ──────────────────────────────────────────────────

    IEnumerator CounterRoutine(ItemChain itemChain)
    {
        isProcessing = true;

        while (playerInZone)
        {
            Customer customer = customerSpawner.GetFirstCustomer();

            // 1. 손님이 없으면 대기
            if (customer == null)
            {
                yield return null;
                continue;
            }

            // 2. 손님이 요구하는 양을 다 채울 때까지 계속 던지기 시도.
            // DeliverToCustomer가 '완료'될 때까지 기다리지 않고 내부에서 처리

            if (itemChain.GetResultCount() > 0 && !customer.isSatisfied)
            {
                // 아이템이 하나라도 있으면 배달 프로세스 실행
                yield return StartCoroutine(DeliverToCustomer(itemChain, customer));

                // 손님이 만족했다면 다음 손님을 위한 딜레이
                if (customer.isSatisfied)
                {
                    yield return new WaitForSeconds(nextCustomerDelay);
                }
            }
            else
            {
                yield return null; // 손님 또는 아이템 대기
            }
        }

        isProcessing = false;
    }

    IEnumerator DeliverToCustomer(ItemChain itemChain, Customer customer)
    {
        // 플레이어가 아이템을 가지고 있고, 손님이 아직 만족하지 않았다면 계속 실행
        while (itemChain.GetResultCount() > 0 && !customer.isSatisfied)
        {
            ResultItem item = itemChain.PopResultItem();
            if (item == null) break;

            Vector3 targetPos = customer.transform.position + Vector3.up * 1.5f;

            // 아이템 날리기
            item.FlyTo(targetPos, () =>
            {
                // 도착 시 실행될 콜백
                if (item != null) Destroy(item.gameObject);

                // ★ 여기서 손님에게 아이템이 도착했음을 알림!
                customer.AddDeliverCount(1);

                // 보상(돈) 생성은 손님이 만족하는 순간에 한 번만!
                if (customer.currentArrivedCount == customer.itemsRequired)
                {
                    moneyPickupZone?.SpawnMoney(customer.moneyReward);
                }
            });

            // 다음 아이템 투입까지의 간격
            yield return new WaitForSeconds(insertInterval);
        }
    }
}
