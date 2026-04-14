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

            int cost = GetCurrentCost();

            // 납부 완료 → 업그레이드 적용
            if (depositedMoney >= cost)
            {
                depositedMoney -= cost;
                targetUpgrade.UpgradeDrill();
                UpdateUI();

                if (IsMaxed()) yield break;
                continue;
            }

            // 돈 아이템 한 개 꺼내기
            MoneyItem moneyItem = player.ItemChain.PopMoneyItem();
            if (moneyItem == null)
            {
                yield return null;  // 돈이 없으면 대기
                continue;
            }

            int value = moneyItem.value;
            Vector3 targetPos = transform.position + Vector3.up * 0.5f;

            moneyItem.FlyTo(targetPos, () =>
            {
                depositedMoney += value;
                GameManager.instance.SpendMoney(value);
                Destroy(moneyItem.gameObject);
                UpdateUI();
            });

            yield return new WaitForSeconds(insertInterval);
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
