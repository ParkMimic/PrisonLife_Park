using UnityEngine;
using System.Collections.Generic;

public class MoneyPickupZone : MonoBehaviour, IInteractable
{
    [Header("돈 스폰 설정")]
    public GameObject moneyPrefab;
    public Transform spawnPoint;
    public float stackHeight = 0.3f;
    public Vector3 moneyRotation = new Vector3(90f, 0f, 0f);

    [Header("픽업 설정")]
    public int maxPickupCount = 4;

    private int moneyCount = 0;
    private List<MoneyItem> spawnedItems = new List<MoneyItem>();

    public void SpawnMoney(int count)
    {
        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = basePos + Vector3.up * (stackHeight * moneyCount);
            GameObject obj = Instantiate(moneyPrefab, pos, Quaternion.Euler(moneyRotation));

            MoneyItem item = obj.GetComponent<MoneyItem>();
            if (item != null) spawnedItems.Add(item);

            moneyCount++;
        }
    }

    public void OnPlayerEnter(PlayerInteraction player)
    {
        spawnedItems.RemoveAll(item => item == null);

        int picked = 0;
        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            if (player.ItemChain.IsFull()) break;
            if (picked >= maxPickupCount) break;

            MoneyItem item = spawnedItems[i];
            spawnedItems.RemoveAt(i);
            moneyCount--;
            item.Init(player.transform);
            picked++;
        }
    }

    public void OnPlayerExit(PlayerInteraction player) { }
}
