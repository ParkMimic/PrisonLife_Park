using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// 플레이어가 Zone 진입 후 50원 납부 시 카운터 AI를 소환한다. (1마리)
/// </summary>
public class CounterSpawnZone : MonoBehaviour, IInteractable
{
    [Header("소환 설정")]
    public GameObject counterAIPrefab;
    public Transform  spawnPoint;
    public Transform  pickupPoint;   // CounterAI에게 전달할 수갑 픽업 위치
    public Transform  counterPoint;  // CounterAI에게 전달할 카운터 위치
    public int        spawnCost = 50;

    [Header("돈 투입 간격")]
    public float insertInterval = 0.3f;

    [Header("World Space UI")]
    public GameObject  uiRoot;
    public TextMeshPro titleText;
    public TextMeshPro progressText;

    private int  depositedMoney = 0;
    private bool playerInZone   = false;
    private bool isProcessing   = false;
    private bool isSpawned      = false;

    // ──────────────────────────────────────────────────────────

    public void OnPlayerEnter(PlayerInteraction player)
    {
        playerInZone = true;
        if (uiRoot != null) uiRoot.SetActive(true);
        UpdateUI();

        if (!isProcessing)
            StartCoroutine(ConsumeMoneyRoutine(player));
    }

    public void OnPlayerExit(PlayerInteraction player)
    {
        playerInZone = false;
        isProcessing = false;
        StopAllCoroutines();
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────

    IEnumerator ConsumeMoneyRoutine(PlayerInteraction player)
    {
        isProcessing = true;

        while (playerInZone)
        {
            if (isSpawned)
            {
                UpdateUI();
                yield break;
            }

            if (depositedMoney >= spawnCost)
            {
                depositedMoney -= spawnCost;
                SpawnCounterAI();
                UpdateUI();
                yield break;
            }

            MoneyItem moneyItem = player.ItemChain.PopMoneyItem();
            if (moneyItem == null)
            {
                yield return null;
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

    void SpawnCounterAI()
    {
        if (counterAIPrefab == null || spawnPoint == null) return;

        GameObject obj = Instantiate(counterAIPrefab, spawnPoint.position, spawnPoint.rotation);
        obj.GetComponent<CounterAI>()?.Init(pickupPoint, counterPoint);
        isSpawned = true;

        Debug.Log("[CounterSpawnZone] 카운터 AI 소환!");
    }

    void UpdateUI()
    {
        if (isSpawned)
        {
            if (titleText    != null) titleText.text    = "카운터 AI";
            if (progressText != null) progressText.text = "소환 완료";
            return;
        }

        if (titleText    != null) titleText.text    = "카운터 AI 소환";
        if (progressText != null) progressText.text = $"{depositedMoney} / {spawnCost}";
    }
}
