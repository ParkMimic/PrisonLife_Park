using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MoneyPickupZone : MonoBehaviour, IInteractable
{
    [Header("돈 스폰 설정")]
    public GameObject moneyPrefab;
    public Transform spawnPoint;
    public float stackHeight = 0.3f;
    public Vector3 moneyRotation = new Vector3(90f, 0f, 0f);

    [Header("적재 설정")]
    public int maxCount = 20;

    [Header("픽업 설정")]
    public float pickupInterval = 0.1f;

    [Header("UI")]
    public Text fullText;

    private int moneyCount = 0;
    private List<MoneyItem> spawnedItems = new List<MoneyItem>();
    private bool playerInZone = false;

    void Start()
    {
        if (fullText != null) fullText.gameObject.SetActive(false);
    }

    bool IsFull() => moneyCount >= maxCount;

    void UpdateFullUI()
    {
        if (fullText != null) fullText.gameObject.SetActive(IsFull());
    }

    public void SpawnMoney(int count)
    {
        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;

        for (int i = 0; i < count; i++)
        {
            if (moneyCount >= maxCount) break;

            Vector3 pos = basePos + Vector3.up * (stackHeight * moneyCount);
            GameObject obj = Instantiate(moneyPrefab, pos, Quaternion.Euler(moneyRotation));

            MoneyItem item = obj.GetComponent<MoneyItem>();
            if (item != null) spawnedItems.Add(item);

            moneyCount++;
        }

        UpdateFullUI();
    }

    public void OnPlayerEnter(PlayerInteraction player)
    {
        playerInZone = true;
        StartCoroutine(PickupRoutine(player));
    }

    public void OnPlayerExit(PlayerInteraction player)
    {
        playerInZone = false;
        StopAllCoroutines();
    }

    IEnumerator PickupRoutine(PlayerInteraction player)
    {
        PlayerAudio playerAudio = player.GetComponent<PlayerAudio>();

        while (playerInZone)
        {
            spawnedItems.RemoveAll(item => item == null);

            if (spawnedItems.Count == 0 || player.ItemChain.IsFull())
            {
                yield return null;
                continue;
            }

            int picked = 0;
            for (int i = spawnedItems.Count - 1; i >= 0; i--)
            {
                if (player.ItemChain.IsFull()) break;

                MoneyItem item = spawnedItems[i];
                spawnedItems.RemoveAt(i);
                moneyCount--;
                item.Init(player.transform);
                picked++;
            }

            if (picked > 0)
            {
                playerAudio?.PlayReceiveMoneySound();
                UpdateFullUI();
                yield return new WaitForSeconds(pickupInterval);
            }
            else
            {
                yield return null;
            }
        }
    }
}
