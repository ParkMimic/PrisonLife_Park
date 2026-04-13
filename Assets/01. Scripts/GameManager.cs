using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Player player;

    private int totalMoney = 0;
    public int TotalMoney => totalMoney;

    private void Awake()
    {
        instance = this;
    }

    public void AddMoney(int amount)
    {
        totalMoney += amount;
        Debug.Log($"[GameManager] 획득: {amount}원 / 누적: {totalMoney}원");
    }
}
