using UnityEngine;
using System.Collections;

public class ConverterProcessor : MonoBehaviour
{
    public enum InputType { Mineral, ResultItem }
    public enum OutputType { SpawnPrefab, SatisfyCustomer }

    [Header("프로세서 설정")]
    public InputType inputType = InputType.Mineral;
    public OutputType outputType = OutputType.SpawnPrefab;
    public int itemsRequired = 4;       // 몇 개 투입당 결과 1개
    public float convertInterval = 1.0f;   // 변환 간격

    [Header("SpawnPrefab 설정")]
    public GameObject resultPrefab;
    public Transform resultSpawnPoint;
    public float resultStackHeight = 0.5f;

    [Header("SatisfyCustomer 설정")]
    public CustomerSpawner customerSpawner;

    [Header("참조 - Inspector에서 직접 연결")]
    public ConverterDisplay display;

    private int storedCount = 0;
    private int resultCount = 0;
    private bool isConverting = false;

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
            display?.RemoveMineral(itemsRequired);

            // outputType에 따라 결과 처리
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

        Vector3 spawnPos = resultSpawnPoint.position
            + Vector3.up * (resultStackHeight * resultCount);

        Instantiate(resultPrefab, spawnPos, Quaternion.identity);
        resultCount++;

        Debug.Log($"[Processor] 결과물 생성! {resultCount}개");
    }

    void SatisfyCustomer()
    {
        Customer customer = customerSpawner?.GetFirstCustomer();
        if (customer == null)
        {
            Debug.Log("[Processor] 대기 중인 손님이 없어요!");
            return;
        }

        customer.Satisfy();
        Debug.Log("[Processor] 손님 만족!");
    }

    public void OnResultPickedUp()
    {
        if (resultCount > 0)
            resultCount--;
    }
}