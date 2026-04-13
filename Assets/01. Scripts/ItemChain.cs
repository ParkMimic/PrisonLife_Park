using UnityEngine;
using System.Collections.Generic;

public class ItemChain : MonoBehaviour
{
    [Header("스택 설정")]
    public Vector3 stackOffset = new Vector3(0f, 0f, -1f);
    public float itemHeight = 0.5f;
    public float followSpeed = 10f;

    [Header("아이템 타입별 회전 설정")]
    public Vector3 mineralRotation = new Vector3(90f, 0f, 0f);
    public Vector3 resultRotation = new Vector3(0f, 0f, 0f);

    [Header("그룹 간 간격")]
    public float groupOffset = 1.5f;  // 그룹 사이 Z축 간격

    [Header("최대 보유량")]
    public int maxItemCount = 10;

    private List<MineralItem> mineralChain = new List<MineralItem>();
    private List<ResultItem> resultChain = new List<ResultItem>();
    private List<string> groupOrder = new List<string>();

    public int GetResultCount() => resultChain.Count;

    void Update()
    {
        if (GetCount() == 0) return;

        // 첫 번째 그룹 시작 위치
        Vector3 currentBase = transform.position
            + transform.TransformDirection(stackOffset);

        foreach (string group in groupOrder)
        {
            if (group == "mineral")
                currentBase = UpdateGroup(mineralChain, currentBase, mineralRotation);
            else if (group == "result")
                currentBase = UpdateGroup(resultChain, currentBase, resultRotation);
        }
    }

    Vector3 UpdateGroup<T>(List<T> group, Vector3 basePos, Vector3 rotation)
        where T : Component
    {
        for (int i = 0; i < group.Count; i++)
        {
            // Y축으로 쌓기
            Vector3 targetPos = basePos + Vector3.up * (itemHeight * i);

            group[i].transform.position = Vector3.Lerp(
                group[i].transform.position,
                targetPos,
                followSpeed * Time.deltaTime
            );

            group[i].transform.rotation = Quaternion.Euler(rotation);
        }

        // 다음 그룹은 Z축으로 뒤에 배치
        return basePos + transform.TransformDirection(
            new Vector3(0f, 0f, -groupOffset));
    }

    public bool IsFull() => GetCount() >= maxItemCount;

    public int GetCount() => mineralChain.Count + resultChain.Count;

    public Vector3 GetNextStackPosition()
    {
        // 현재 해당 타입 그룹의 다음 위치 반환
        Vector3 stackBase = transform.position
            + transform.TransformDirection(stackOffset);
        return stackBase + Vector3.up * (itemHeight * GetCount());
    }

    public bool AddItem(MineralItem item)
    {
        if (IsFull())
        {
            Destroy(item.gameObject);
            return false;
        }

        if (mineralChain.Count == 0)
            groupOrder.Add("mineral");

        mineralChain.Add(item);
        return true;
    }

    public bool AddResultItem(ResultItem item)
    {
        if (IsFull())
        {
            Debug.Log("[ItemChain] 최대 보유량 도달!");
            return false;
        }

        if (resultChain.Count == 0)
            groupOrder.Add("result");

        resultChain.Add(item);
        return true;
    }

    public MineralItem PopItem()
    {
        if (mineralChain.Count == 0) return null;

        int lastIndex = mineralChain.Count - 1;
        MineralItem item = mineralChain[lastIndex];
        mineralChain.RemoveAt(lastIndex);

        if (mineralChain.Count == 0)
            groupOrder.Remove("mineral");

        return item;
    }

    public ResultItem PopResultItem()
    {
        if (resultChain.Count == 0) return null;

        int lastIndex = resultChain.Count - 1;
        ResultItem item = resultChain[lastIndex];
        resultChain.RemoveAt(lastIndex);

        if (resultChain.Count == 0)
            groupOrder.Remove("result");

        return item;
    }
}