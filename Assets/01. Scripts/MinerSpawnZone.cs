using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 Zone 진입 후 50원 납부 시 광부 AI를 소환한다.
/// 최대 3마리까지 소환 가능.
/// </summary>
public class MinerSpawnZone : MonoBehaviour, IInteractable
{
    [Header("소환 설정")]
    public GameObject minerPrefab;
    public Transform spawnPoint;
    public int spawnCost   = 50;
    public int maxMiners   = 3;

    [Header("돈 투입 간격")]
    public float insertInterval = 0.3f;

    [Header("World Space UI")]
    public GameObject uiRoot;
    public Text titleText;
    public Text progressText;
    public Slider costSlider;

    private int depositedMoney = 0;
    private bool playerInZone  = false;
    private bool isProcessing  = false;

    private List<MinerAI> spawnedMiners = new List<MinerAI>();

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
        playerInZone  = false;
        isProcessing  = false;
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
            if (IsMaxed())
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
                SpawnMiner();
                UpdateUI();
                if (IsMaxed()) yield break;
            }
        }

        isProcessing = false;
    }

    // ──────────────────────────────────────────────────────────

    void SpawnMiner()
    {
        if (minerPrefab == null || spawnPoint == null) return;

        spawnedMiners.RemoveAll(m => m == null);

        // 50원으로 maxMiners마리 한꺼번에 소환
        for (int i = 0; i < maxMiners; i++)
        {
            GameObject obj = Instantiate(minerPrefab, spawnPoint.position, spawnPoint.rotation);
            MinerAI ai = obj.GetComponent<MinerAI>();
            if (ai != null) spawnedMiners.Add(ai);
        }

        Debug.Log($"[MinerSpawnZone] 광부 {maxMiners}마리 소환!");
    }

    bool IsMaxed()
    {
        spawnedMiners.RemoveAll(m => m == null);
        return spawnedMiners.Count >= maxMiners;
    }

    void UpdateUI()
    {
        if (IsMaxed())
        {
            if (titleText    != null) titleText.text    = "광부 소환";
            if (progressText != null) progressText.text = "소환 완료";
            return;
        }

        if (titleText    != null) titleText.text    = $"광부 소환 x{maxMiners}";
        if (progressText != null) progressText.text = $"{spawnCost - depositedMoney}";
        if (costSlider   != null) costSlider.value  = (float)depositedMoney / spawnCost;
    }
}
