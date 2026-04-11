using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemChain : MonoBehaviour
{
    [Header("스택 설정")]
    public Vector3 stackOffset = new Vector3(0f, 0f, -1f); // 플레이어 뒤 위치
    public float itemHeight = 0.5f; // 아이템 간 높이 간격
    public float followSpeed = 10f; // 스택 따라오는 속도

    [Header("아이템 회전 설정")]
    public Vector3 itemRotation = new Vector3(0f, 0f, 90f);

    [Header("최대 보유량")]
    public int maxItemCount = 40;

    private List<MineralItem> chain = new List<MineralItem>();

    private void Update()
    {
        if (chain.Count == 0) return;

        // 플레이어 뒤 기준 위치 계산 (플레이어 회전 반영)
        Vector3 stackBasePosition = transform.position + transform.TransformDirection(stackOffset);

        // 모든 아이템을 높이 순서대로 배치
        for (int i = 0; i < chain.Count; i++)
        {
            Vector3 targetPos = stackBasePosition + Vector3.up * (itemHeight * i);

            // 위치 + 회전 동시 적용
            chain[i].transform.position = Vector3.Lerp(
                chain[i].transform.position,
                targetPos,
                followSpeed * Time.deltaTime);

            chain[i].transform.rotation = Quaternion.Euler(itemRotation);
        }
    }

    public bool IsFull() => chain.Count >= maxItemCount;

    // 새 아이템 등록 -> 따라갈 target 반환
    public bool AddItem(MineralItem item)
    {
        if (IsFull())
        {
            Debug.Log("최대 보유량 도달!");
            Destroy(item.gameObject); // 아이템 파괴
            return false;
        }
        chain.Add(item);
        return true; // 플레이어의 transform 반환
    }

    // 아이템 소비 (창고 납품 등)
    public MineralItem PopItem()
    {
        if (chain.Count == 0) return null;
        MineralItem last = chain[chain.Count - 1];
        chain.RemoveAt(chain.Count - 1);
        return last;
    }

    // 현재 보유 수량 UI 등에 활용
    public int GetCount() => chain.Count;
}
