using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
    public Text titleText;
    public Text progressText;
    public Slider costSlider;

    private int  depositedMoney = 0;
    private bool playerInZone   = false;
    private bool isProcessing   = false;
    private bool isSpawned      = false;

    // ──────────────────────────────────────────────────────────

    void Start()
    {
        if (costSlider != null) costSlider.value = 0f;
    }

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
        PlayerAudio playerAudio = player.GetComponent<PlayerAudio>();

        while (playerInZone)
        {
            if (isSpawned)
            {
                UpdateUI();
                yield break;
            }

            int remaining = spawnCost - depositedMoney;

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

            // 수집한 아이템 한꺼번에 발사
            int pending = batch.Count;
            foreach (var (item, value) in batch)
            {
                var capturedItem  = item;
                var capturedValue = value;
                playerAudio?.PlayInsertSound();
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

            // 마지막 아이템 도착 대기
            yield return new WaitUntil(() => pending <= 0);

            // 납부 완료 → 소환
            if (depositedMoney >= spawnCost)
            {
                depositedMoney -= spawnCost;
                SpawnCounterAI();
                UpdateUI();
                yield break;
            }
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
        if (progressText != null) progressText.text = $"{spawnCost - depositedMoney}";
        if (costSlider   != null) costSlider.value  = (float)depositedMoney / spawnCost;
    }
}
