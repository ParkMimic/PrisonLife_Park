using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConverterProcessor : MonoBehaviour
{
    [Header("변환 설정")]
    public GameObject resultPrefab; // 변환 결과 프리팹
    public Transform resultSpawnPoint; // 결과물 생성 위치
    public int mineralsPerResult = 2; // 광물 몇 개당 결과물 1개
    public float convertInterval = 1.0f; // 변환 간격

    [Header("결과물 스택 설정")]
    public float resultStackHeight = 0.5f; // 결과물 간 높이 간격

    private ConverterDisplay display;

    private int storedCount = 0; // 현재 저장된 광물 개수
    private bool isConverting = false; // 변환 중 여부
    private int resultCount = 0; // 생성된 결과물 개수

    private void Awake()
    {
        display = GetComponent<ConverterDisplay>();
    }

    public void OnMineralInserted()
    {
        storedCount++;
        Debug.Log($"[Processor] 광물 투입됨 / 누적 : {storedCount}개");

        if (storedCount >= mineralsPerResult && !isConverting)
        {
            Debug.Log("[Processor] 변환 시작!");
            StartCoroutine(ConvertRoutine());
        }
    }

    public void StartConvert()
    {
        Debug.Log("[Processor] 변환 시작!");
        // 변환 중이 아니면 변환 시작
        if (!isConverting)
        {
            StartCoroutine(ConvertRoutine());
        }
    }

    IEnumerator ConvertRoutine()
    {
        isConverting = true;

        Debug.Log($"[Processor] ConvertRoutine 진입! storedCount : {storedCount}, mineralsPerResult : {mineralsPerResult}");

        // 일정 개수마다 결과물 생성
        while (storedCount >= mineralsPerResult)
        {
            Debug.Log($"[Processor] 변환 대기중... 현재 : {storedCount}개");
            // 변환 간격 대기
            yield return new WaitForSeconds(convertInterval);

            // 광물 소모
            storedCount -= mineralsPerResult;

            // 디스플레이에서 소모된 만큼 제거
            display.RemoveMineral(mineralsPerResult);

            if (resultPrefab != null)
            {
                // 생성될 때마다 높이를 올려서 배치
                Vector3 spawnPos = resultSpawnPoint.position + Vector3.up * (resultStackHeight * resultCount);

                Debug.Log($"[Processor] 생성 위치 : {spawnPos}, resultCount : {resultCount}");
                Instantiate(resultPrefab, resultSpawnPoint.position, Quaternion.identity);
                resultCount++;

                Debug.Log($"[Processor] 결과물 생성! 현재 {resultCount}개");
            }
        }

        Debug.Log("[Processor] 변환 종료!");
        isConverting = false;
    }

    // 플레이어가 결과물을 픽업할 때마다 호출
    public void OnResultPickedUp()
    {
        if (resultCount > 0)
            resultCount--;
    }
}
