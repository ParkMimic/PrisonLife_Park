using UnityEngine;

public class PlayerUpgrade : MonoBehaviour
{
    public int DrillLevel { get; private set; } = 0;

    [Header("레벨 0: 기본 드릴 오브젝트 (기본 활성 상태로 배치)")]
    public GameObject drillLv0;

    [Header("레벨 1: 업그레이드 드릴 오브젝트 (비활성 상태로 배치)")]
    public GameObject drillLv1;

    [Header("레벨 2: 확장 채굴 트리거 오브젝트 (비활성 상태로 배치)")]
    public GameObject expandedMiningTrigger;

    private PlayerInteraction interaction;
    private ItemChain itemChain;

    private void Awake()
    {
        interaction = GetComponent<PlayerInteraction>();
        itemChain   = GetComponent<ItemChain>();

        if (drillLv1             != null) drillLv1.SetActive(false);
        if (expandedMiningTrigger != null) expandedMiningTrigger.SetActive(false);
    }

    // ── 채굴 모드 On/Off ─────────────────────────────────────

    public void EnterMiningMode()
    {
        if (DrillLevel == 0 && drillLv0 != null)
            drillLv0.SetActive(true);
        else if (DrillLevel == 1 && drillLv1 != null)
            drillLv1.SetActive(true);

        if (DrillLevel >= 2 && expandedMiningTrigger != null)
            expandedMiningTrigger.SetActive(true);
    }

    public void ExitMiningMode()
    {
        if (drillLv0 != null) drillLv0.SetActive(false);
        if (drillLv1 != null) drillLv1.SetActive(false);
        if (expandedMiningTrigger != null) expandedMiningTrigger.SetActive(false);
    }

    /// <summary>
    /// 드릴을 한 단계 업그레이드합니다. 이미 최대 레벨이면 false 반환.
    /// </summary>
    public bool UpgradeDrill()
    {
        if (DrillLevel >= 2) return false;

        DrillLevel++;

        switch (DrillLevel)
        {
            case 1:
                interaction.miningCooldown = 0f;
                itemChain.maxMineralCount *= 2;
                itemChain.maxResultCount *= 2;
                itemChain.maxMoneyCount *= 2;

                EventManager.instance?.TriggerFirstDrillUpgradeEvent();
                Debug.Log($"[PlayerUpgrade] 드릴 Lv.1: 채굴 딜레이 제거 / 모든 광물 최대 {itemChain.maxMineralCount}개");
                break;

            case 2:
                itemChain.maxMineralCount *= 2;
                itemChain.maxResultCount *= 2;
                itemChain.maxMoneyCount *= 2;
                Debug.Log($"[PlayerUpgrade] 드릴 Lv.2: 채굴 범위 확대 / 모든 광물 최대 {itemChain.maxMineralCount}개");
                break;
        }

        return true;
    }
}
