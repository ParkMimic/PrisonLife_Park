using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HUD - 돈")]
    public Text moneyText;

    [Header("튜토리얼")]
    public Text tutorialText;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (tutorialText == null || !tutorialText.gameObject.activeSelf) return;

        if (Input.touchCount > 0 || Input.anyKeyDown)
            tutorialText.gameObject.SetActive(false);
    }

    // ── 돈 ───────────────────────────────────────────────────

    public void UpdateMoney(int amount)
    {
        if (moneyText != null)
            moneyText.text = $"${amount}";
    }
}
