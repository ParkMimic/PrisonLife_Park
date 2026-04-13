using UnityEngine;
using System.Collections.Generic;

public class ItemChain : MonoBehaviour
{
    [Header("스택 설정")]
    public Vector3 stackOffset = new Vector3(0f, 1f, -1f);
    public float itemHeight = 0.5f;
    public float followSpeed = 10f;

    [Header("타입별 회전 설정")]
    public Vector3 mineralRotation = new Vector3(90f, 0f, 0f);
    public Vector3 resultRotation = new Vector3(0f, 0f, 0f);
    public Vector3 moneyRotation = new Vector3(0f, 0f, 0f);

    [Header("그룹 간 간격")]
    public float groupOffset = 1.5f;

    [Header("최대 개수")]
    public int maxItemCount = 10;

    private List<MineralItem> mineralChain = new List<MineralItem>();
    private List<ResultItem> resultChain = new List<ResultItem>();
    private List<MoneyItem> moneyChain = new List<MoneyItem>();

    private List<string> groupOrder = new List<string>();

    public int GetMineralCount() => mineralChain.Count;
    public int GetResultCount()  => resultChain.Count;
    public int GetMoneyCount()   => moneyChain.Count;

    void Update()
    {
        if (GetCount() == 0) return;

        Vector3 currentBase = transform.localPosition
            + transform.TransformDirection(stackOffset);

        foreach (string group in groupOrder)
        {
            if (group == "mineral")
                currentBase = UpdateGroup(mineralChain, currentBase, mineralRotation);
            else if (group == "result")
                currentBase = UpdateGroup(resultChain, currentBase, resultRotation);
            else if (group == "money")
                currentBase = UpdateGroup(moneyChain, currentBase, moneyRotation);
        }
    }

    Vector3 UpdateGroup<T>(List<T> group, Vector3 basePos, Vector3 rotation)
        where T : Component
    {
        for (int i = 0; i < group.Count; i++)
        {
            Vector3 targetPos = basePos + Vector3.up * (itemHeight * i);

            group[i].transform.localPosition = Vector3.Lerp(
                group[i].transform.localPosition,
                targetPos,
                followSpeed * Time.deltaTime
            );

            group[i].transform.localRotation = Quaternion.Euler(rotation);
        }

        return basePos + transform.TransformDirection(new Vector3(0f, 0f, -groupOffset));
    }

public bool IsFull() => GetCount() >= maxItemCount;

    public int GetCount() => mineralChain.Count + resultChain.Count + moneyChain.Count;

    public Vector3 GetNextStackPosition()
    {
        Vector3 stackBase = transform.localPosition
            + transform.TransformDirection(stackOffset);
        return stackBase + Vector3.up * (itemHeight * GetCount());
    }

    // 특정 그룹의 다음 스택 위치를 그룹 오프셋까지 반영해서 반환
    public Vector3 GetNextGroupPosition(string groupName)
    {
        Vector3 currentBase = transform.localPosition
            + transform.TransformDirection(stackOffset);

        foreach (string group in groupOrder)
        {
            if (group == groupName)
            {
                int count = group == "mineral" ? mineralChain.Count
                          : group == "result"  ? resultChain.Count
                          : moneyChain.Count;

                return currentBase + Vector3.up * (itemHeight * count);
            }
            currentBase += transform.TransformDirection(new Vector3(0f, 0f, -groupOffset));
        }

        // 아직 그룹이 없으면 기존 그룹 뒤에 위치
        return currentBase;
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
            Debug.Log("[ItemChain] 최대 개수 초과!");
            return false;
        }

        if (resultChain.Count == 0)
            groupOrder.Add("result");

        resultChain.Add(item);
        return true;
    }

    public bool AddMoneyItem(MoneyItem item)
    {
        if (IsFull())
        {
            Debug.Log("[ItemChain] 최대 개수 초과!");
            return false;
        }

        if (moneyChain.Count == 0)
            groupOrder.Add("money");

        moneyChain.Add(item);
        GameManager.instance.AddMoney(item.value);
        return true;
    }

    public MineralItem PopItem()
    {
        if (mineralChain.Count == 0) return null;

        int lastIndex = mineralChain.Count - 1;
        MineralItem item = mineralChain[lastIndex];
        mineralChain.RemoveAt(lastIndex);
        GameManager.instance.RemoveMineral();

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
        GameManager.instance.RemoveHandcuff();

        if (resultChain.Count == 0)
            groupOrder.Remove("result");

        return item;
    }

    public MoneyItem PopMoneyItem()
    {
        if (moneyChain.Count == 0) return null;

        int lastIndex = moneyChain.Count - 1;
        MoneyItem item = moneyChain[lastIndex];
        moneyChain.RemoveAt(lastIndex);

        if (moneyChain.Count == 0)
            groupOrder.Remove("money");

        return item;
    }
}
