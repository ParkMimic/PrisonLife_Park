using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // ── 씬 시스템 참조 ────────────────────────────────────────
    [Header("씬 시스템 참조")]
    public Player            player;
    public MineralSpawner    mineralSpawner;
    public ConverterDisplay  converterDisplay;
    public ConverterProcessor converterProcessor;
    public CustomerSpawner   customerSpawner;
    public MoneyPickupZone   moneyPickupZone;
    public Prison            prison;

    // ── 현재 보유량 ──────────────────────────────────────────
    private int mineralCount  = 0;
    private int handcuffCount = 0;
    private int moneyAmount   = 0;
    private int prisonerCount = 0;

    private bool firstMoneyTriggered = false;

    public int MineralCount  => mineralCount;
    public int HandcuffCount => handcuffCount;
    public int MoneyAmount   => moneyAmount;
    public int PrisonerCount => prisonerCount;

    private void Awake()
    {
        instance = this;
    }

    // ── 광물 ─────────────────────────────────────────────────
    public void AddMineral(int amount = 1)
    {
        mineralCount += amount;
        Debug.Log($"[GameManager] 광물 +{amount} / 보유: {mineralCount}");
    }

    public void RemoveMineral(int amount = 1)
    {
        mineralCount = Mathf.Max(0, mineralCount - amount);
        Debug.Log($"[GameManager] 광물 -{amount} / 보유: {mineralCount}");
    }

    // ── 수갑 ─────────────────────────────────────────────────
    public void AddHandcuff(int amount = 1)
    {
        handcuffCount += amount;
        Debug.Log($"[GameManager] 수갑 +{amount} / 보유: {handcuffCount}");
    }

    public void RemoveHandcuff(int amount = 1)
    {
        handcuffCount = Mathf.Max(0, handcuffCount - amount);
        Debug.Log($"[GameManager] 수갑 -{amount} / 보유: {handcuffCount}");
    }

    // ── 돈 ───────────────────────────────────────────────────
    public void AddMoney(int amount)
    {
        moneyAmount += amount;
        UIManager.instance?.UpdateMoney(moneyAmount);
        Debug.Log($"[GameManager] 돈 +{amount} / 보유: {moneyAmount}");

        if (!firstMoneyTriggered)
        {
            firstMoneyTriggered = true;
            EventManager.instance?.TriggerFirstMoneyEvent();
        }
    }

    public void SpendMoney(int amount)
    {
        moneyAmount = Mathf.Max(0, moneyAmount - amount);
        UIManager.instance?.UpdateMoney(moneyAmount);
        Debug.Log($"[GameManager] 돈 -{amount} / 보유: {moneyAmount}");
    }

    // ── 수감자 ───────────────────────────────────────────────
    public void AddPrisoner(int amount = 1)
    {
        prisonerCount += amount;
        Debug.Log($"[GameManager] 수감자 +{amount} / 총: {prisonerCount}");
    }
}
