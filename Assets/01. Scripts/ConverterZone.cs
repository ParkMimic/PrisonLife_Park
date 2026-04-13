using UnityEngine;
using System.Collections;

public class ConverterZone : MonoBehaviour, IInteractable
{
    public enum ZoneMode { Converter, Counter }

    [Header("모드 선택")]
    public ZoneMode zoneMode = ZoneMode.Converter;

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

            if (customer != null && itemChain.GetResultCount() >= customer.itemsRequired)
            {
                yield return StartCoroutine(DeliverToCustomer(itemChain, customer));
                yield return new WaitForSeconds(nextCustomerDelay);
            }
            else
                yield return null; // 손님 또는 아이템 대기
        }

        isProcessing = false;
    }

    IEnumerator DeliverToCustomer(ItemChain itemChain, Customer customer)
    {
        int arrivedCount = 0;
        int required = customer.itemsRequired;

        for (int i = 0; i < required; i++)
        {
            ResultItem item = itemChain.PopResultItem();
            if (item == null) break;

            Vector3 targetPos = customer.transform.position + Vector3.up * 1.5f;
            item.FlyTo(targetPos, () =>
            {
                Destroy(item.gameObject);
                arrivedCount++;

                if (arrivedCount >= required)
                {
                    customer.Satisfy();
                    moneyPickupZone?.SpawnMoney(customer.moneyReward);
                }
            });

            yield return new WaitForSeconds(insertInterval);
        }
    }
}
