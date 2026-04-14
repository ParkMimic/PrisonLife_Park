using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConverterProcessor : MonoBehaviour
{
    public enum InputType { Mineral, ResultItem }
    public enum OutputType { SpawnPrefab, SatisfyCustomer }

    [Header("프로세서 설정")]
    public InputType inputType = InputType.Mineral;
    public OutputType outputType = OutputType.SpawnPrefab;
    public int itemsRequired = 4;
    public float convertInterval = 1.0f;

    [Header("SpawnPrefab 설정")]
    public GameObject resultPrefab;
    public Transform resultSpawnPoint;
    public float resultStackHeight = 0.5f;

    [Header("SatisfyCustomer 설정")]
    public CustomerSpawner customerSpawner;

    [Header("참조 - Inspector에서 직접 연결")]
    public ConverterDisplay display;

    private int storedCount = 0;
    private bool isConverting = false;

    // 현재 스폰 지점에 있는 아이템 목록 (픽업되면 제거됨)
    private List<ResultItem> spawnedItems = new List<ResultItem>();

    void Update()
    {
        // 파괴된 참조 정리
        spawnedItems.RemoveAll(item => item == null);

        // 인벤토리처럼: 픽업된 아이템이 빠지면 나머지 위치 갱신
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            Vector3 targetPos = resultSpawnPoint.position + Vector3.up * (resultStackHeight * i);
            spawnedItems[i].transform.position = Vector3.Lerp(
                spawnedItems[i].transform.position, targetPos, 10f * Time.deltaTime
            );
        }
    }

    public void OnItemInserted()
    {
        storedCount++;

        if (storedCount >= itemsRequired && !isConverting)
            StartCoroutine(ConvertRoutine());
    }

    IEnumerator ConvertRoutine()
    {
        isConverting = true;

        while (storedCount >= itemsRequired)
        {
            yield return new WaitForSeconds(convertInterval);

            storedCount -= itemsRequired;
            display?.RemoveMineral(itemsRequired);   // 광물 먼저 제거

            yield return new WaitForSeconds(0.15f);  // 광물 소멸 후 결과물 생성

            switch (outputType)
            {
                case OutputType.SpawnPrefab:
                    SpawnResult();
                    break;

                case OutputType.SatisfyCustomer:
                    SatisfyCustomer();
                    break;
            }
        }

        isConverting = false;
    }

    void SpawnResult()
    {
        if (resultPrefab == null || resultSpawnPoint == null) return;

        // 파괴된 참조 정리 후 현재 개수 기준으로 위치 결정
        spawnedItems.RemoveAll(item => item == null);

        Vector3 spawnPos = resultSpawnPoint.position
            + Vector3.up * (resultStackHeight * spawnedItems.Count);

        GameObject obj = Instantiate(resultPrefab, spawnPos, Quaternion.identity);
        ResultItem ri = obj.GetComponent<ResultItem>();

        if (ri != null)
        {
            // 픽업 시 리스트에서 제거 → 나머지 아이템 위치 자동 갱신
            spawnedItems.Add(ri);
            ri.onPickedUp = () => spawnedItems.Remove(ri);
        }

        Debug.Log($"[Processor] 결과물 생성! 스폰 지점 대기 중: {spawnedItems.Count}개");
    }

    // ── 카운터 AI 전용 ────────────────────────────────────────

    public bool HasResultItems()
    {
        spawnedItems.RemoveAll(item => item == null);
        return spawnedItems.Count > 0;
    }

    /// <summary>스폰 지점 아이템을 하나 꺼내 반환. 없으면 null.</summary>
    public ResultItem TakeItem()
    {
        spawnedItems.RemoveAll(item => item == null);
        if (spawnedItems.Count == 0) return null;

        int last = spawnedItems.Count - 1;
        ResultItem item = spawnedItems[last];
        spawnedItems.RemoveAt(last);
        return item;
    }

    // ─────────────────────────────────────────────────────────

    void SatisfyCustomer()
    {
        Customer customer = customerSpawner?.GetFirstCustomer();
        if (customer == null)
        {
            Debug.Log("[Processor] 대기 중인 고객이 없습니다!");
            return;
        }

        customer.Satisfy();
        Debug.Log("[Processor] 고객 만족!");
    }
}
