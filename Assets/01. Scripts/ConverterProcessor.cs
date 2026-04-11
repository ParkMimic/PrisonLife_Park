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

    private int storedCount = 0; // 현재 저장된 광물 개수
    private bool isConverting = false; // 변환 중 여부
    private ConverterDisplay display;

    private void Awake()
    {
        display = GetComponentInParent<ConverterDisplay>();
    }

    public void OnMineralInserted()
    {
        storedCount++;

        // 변환 중이 아니면 변환 시작
        if (!isConverting)
        {
            StartCoroutine(ConvertRoutine());
        }
    }

    IEnumerator ConvertRoutine()
    {
        isConverting = true;

        // 일정 개수마다 결과물 생성
        while (storedCount >= mineralsPerResult)
        {
            // 변환 간격 대기
            yield return new WaitForSeconds(convertInterval);

            // 광물 소모
            storedCount -= mineralsPerResult;

            // 디스플레이에서 소모된 만큼 제거
            display.RemoveMineral(mineralsPerResult);

            // 결과물 생성
            if (resultPrefab != null)
            {
                Instantiate(resultPrefab, resultSpawnPoint.position, Quaternion.identity);

                Debug.Log("[Converter] 결과물 생성!");
            }

            isConverting = false;
        }
    }
}
