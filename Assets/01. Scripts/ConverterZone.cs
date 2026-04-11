using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConverterZone : MonoBehaviour
{
    [Header("투입 설정")]
    public float insertInterval = 0.15f; // 아이템 투입 간격

    [Header("참조 - Inspector에서 직접 연결")]
    public ConverterDisplay display; // 발판 디스플레이 참조
    public ConverterProcessor processor; // 변환 처리 참조

    private ItemChain itemChain;
    private bool isInserting = false; // 투입 중 여부

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isInserting) return; // 이미 투입 중이면 무시

        itemChain = other.GetComponent<ItemChain>();
        if (itemChain == null || itemChain.GetCount() == 0) return; // 아이템 체인이 없거나 비어있으면 무시

        if (display == null || processor == null)
        {
            Debug.LogError("[ConverterZone] display 또는 processor가 null이에요!");
            return;
        }
        StartCoroutine(InsertRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 트리거에서 나가면 투입 중단
        StopAllCoroutines();
        isInserting = false;
    }

    IEnumerator InsertRoutine()
    {
        isInserting = true;

        // 맨 위(마지막)부터 차례로 투입
        while (itemChain.GetCount() > 0)
        {
            MineralItem item = itemChain.PopItem(); // 아이템 체인에서 하나 꺼내기
            if (item == null) break;

            // 착지할 목표 위치 계산
            Vector3 targetPos = display.GetNextPosition();

            // 날아가서 착지 후 디스플레이에 등록
            item.FlyTo(targetPos, () =>
            {
                display.AddMineral(item.gameObject); // 착지한 오브젝트 등록
                processor.OnMineralInserted(); // 변환 처리
            });

            yield return new WaitForSeconds(insertInterval); // 다음 아이템 투입까지 대기
        }

        isInserting = false; // 투입 완료
    }
}
