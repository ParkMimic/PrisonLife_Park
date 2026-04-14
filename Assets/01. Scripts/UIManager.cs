using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HUD - 돈")]
    public Text moneyText;

    private void Awake()
    {
        instance = this;
    }

    // ── 돈 ───────────────────────────────────────────────────

    public void UpdateMoney(int amount)
    {
        if (moneyText != null)
            moneyText.text = $"${amount}";
    }
}
