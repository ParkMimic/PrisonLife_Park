using UnityEngine;
using System.Collections;
using TMPro;

public class DrillUpgradeZone : MonoBehaviour, IInteractable
{
    [Header("업그레이드 비용")]
    public int level1Cost = 20;   // MoneyObject 4개 (5원 x 4)
    public int level2Cost = 50;   // MoneyObject 10개 (5원 x 10)

    [Header("돈 투입 간격")]
    public float insertInterval = 0.3f;

    [Header("World Space UI")]
    public GameObject uiRoot;
    public TextMeshPro titleText;
    public TextMeshPro progressText;

    // 누적 납부 금액 (Zone을 나갔다 들어와도 유지)
    private int depositedMoney = 0;

    private bool playerInZone = false;
    private bool isProcessing = false;
    private PlayerUpgrade targetUpgrade;

    // ──────────────────────────────────────────────────────────

    public void OnPlayerEnter(PlayerInteraction player)
    {
        targetUpgrade = player.GetComponent<PlayerUpgrade>();
        if (targetUpgrade == null) return;

        playerInZone = true;

        if (uiRoot != null) uiRoot.SetActive(true);
        UpdateUI();

        if (!isProcessing)
            StartCoroutine(ConsumeMoneyRoutine(player));
    }

    public void OnPlayerExit(PlayerInteraction player)
    {
        playerInZone = false;
        StopAllCoroutines();
        isProcessing = false;

        if (uiRoot != null) uiRoot.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────

    IEnumerator ConsumeMoneyRoutine(PlayerInteraction player)
    {
        isProcessing = true;

        while (playerInZone)
        {
            if (IsMaxed())
            {
                UpdateUI();
                yield break;
            }

            int cost      = GetCurrentCost();
            int remaining = cost - depositedMoney;

            // 필요한 금액만큼 아이템 미리 수집
            var batch = new System.Collections.Generic.List<(MoneyItem item, int value)>();
            int willAdd = 0;
            while (willAdd < remaining)
            {
                MoneyItem moneyItem = player.ItemChain.PopMoneyItem();
                if (moneyItem == null) break;
                batch.Add((moneyItem, moneyItem.value));
                willAdd += moneyItem.value;
            }

            if (batch.Count == 0)
            {
                yield return null;
                continue;
            }

            // 수집한 아이템 한꺼번에 발사 (FlyTo 중간에도 다음 아이템 발사)
            int pending = batch.Count;
            foreach (var (item, value) in batch)
            {
                var capturedItem  = item;
                var capturedValue = value;
                capturedItem.FlyTo(transform.position + Vector3.up * 0.5f, () =>
                {
                    depositedMoney += capturedValue;
                    GameManager.instance.SpendMoney(capturedValue);
                    Destroy(capturedItem.gameObject);
                    UpdateUI();
                    pending--;
                });
                yield return new WaitForSeconds(insertInterval);
            }

            // 마지막 아이템이 도착할 때까지 대기
            yield return new WaitUntil(() => pending <= 0);

            // 납부 완료 → 업그레이드
            if (depositedMoney >= cost)
            {
                depositedMoney -= cost;
                targetUpgrade.UpgradeDrill();
                UpdateUI();
                if (IsMaxed()) yield break;
            }
        }

        isProcessing = false;
    }

    // ──────────────────────────────────────────────────────────

    bool IsMaxed() => targetUpgrade != null && targetUpgrade.DrillLevel >= 2;

    int GetCurrentCost()
    {
        if (targetUpgrade == null) return level1Cost;
        return targetUpgrade.DrillLevel == 0 ? level1Cost : level2Cost;
    }

    void UpdateUI()
    {
        if (targetUpgrade == null) return;

        if (IsMaxed())
        {
            if (titleText != null) titleText.text = "드릴 업그레이드";
            if (progressText != null) progressText.text = "최대 레벨";
            return;
        }

        int nextLevel = targetUpgrade.DrillLevel + 1;
        int cost = GetCurrentCost();

        if (titleText != null)
            titleText.text = $"드릴 Lv.{nextLevel} 업그레이드";

        if (progressText != null)
            progressText.text = $"{depositedMoney} / {cost}";
    }
}
