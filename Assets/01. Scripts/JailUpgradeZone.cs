using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class JailUpgradeZone : MonoBehaviour, IInteractable
{
    [Header("업그레이드 설정")]
    public int upgradeCost = 50;
    public int capacityIncrease = 10; // 업그레이드 시 증가할 최대 수용 인원

    [Header("활성화할 오브젝트")]
    public GameObject upgradeObject; // 업그레이드 완료 시 활성화될 오브젝트

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
    private bool isUpgraded    = false;

    // ──────────────────────────────────────────────────────────

    void Start()
    {
        if (costSlider    != null) costSlider.value = 0f;
        if (upgradeObject != null) upgradeObject.SetActive(false);
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
        StopAllCoroutines();
        isProcessing = false;
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────

    IEnumerator ConsumeMoneyRoutine(PlayerInteraction player)
    {
        isProcessing = true;
        PlayerAudio playerAudio = player.GetComponent<PlayerAudio>();

        while (playerInZone)
        {
            if (isUpgraded)
            {
                UpdateUI();
                yield break;
            }

            int remaining = upgradeCost - depositedMoney;

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

            // 납부 완료 → 업그레이드
            if (depositedMoney >= upgradeCost)
            {
                depositedMoney -= upgradeCost;
                ApplyUpgrade();
                UpdateUI();
                yield break;
            }
        }

        isProcessing = false;
    }

    // ──────────────────────────────────────────────────────────

    void ApplyUpgrade()
    {
        isUpgraded = true;

        EventManager.instance?.TriggerJailUpgradeEvent();

        Prison prison = GameManager.instance.prison;
        if (prison != null)
        {
            prison.maxCapacity += capacityIncrease;
            Debug.Log($"[JailUpgrade] 감옥 최대 수용 인원 → {prison.maxCapacity}명");
        }

        if (upgradeObject != null)
            upgradeObject.SetActive(true);
    }

    void UpdateUI()
    {
        if (titleText != null) titleText.text = "감옥 업그레이드";

        if (isUpgraded)
        {
            if (progressText != null) progressText.text = "업그레이드 완료";
            if (costSlider   != null) costSlider.value  = 1f;
            return;
        }

        if (progressText != null) progressText.text = $"{upgradeCost - depositedMoney}";
        if (costSlider   != null) costSlider.value  = (float)depositedMoney / upgradeCost;
    }
}
