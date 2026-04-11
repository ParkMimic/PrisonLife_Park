using UnityEngine;
using System.Collections.Generic;

public class ItemChain : MonoBehaviour
{
    [Header("스택 설정")]
    public Vector3 stackOffset = new Vector3(0f, 0f, -1f);
    public float itemHeight = 0.5f;
    public float followSpeed = 10f;

    [Header("아이템 타입별 회전 설정")]
    public Vector3 mineralRotation = new Vector3(90f, 0f, 0f);  // 광물 회전
    public Vector3 resultRotation = new Vector3(0f, 0f, 0f);   // 결과물 회전

    [Header("최대 보유량")]
    public int maxItemCount = 10;

    private List<Component> chain = new List<Component>();

    void Update()
    {
        if (chain.Count == 0) return;

        Vector3 stackBasePosition = transform.position
            + transform.TransformDirection(stackOffset);

        for (int i = 0; i < chain.Count; i++)
        {
            Transform itemTransform = chain[i].transform;

            Vector3 targetPos = stackBasePosition
                + Vector3.up * (itemHeight * i);

            itemTransform.position = Vector3.Lerp(
                itemTransform.position,
                targetPos,
                followSpeed * Time.deltaTime
            );

            //  타입에 따라 회전값 분리 적용
            if (chain[i] is MineralItem)
                itemTransform.rotation = Quaternion.Euler(mineralRotation);
            else
                itemTransform.rotation = Quaternion.Euler(resultRotation);
        }
    }

    public bool IsFull() => chain.Count >= maxItemCount;

    public Vector3 GetNextStackPosition()
    {
        int index = chain.Count;
        Vector3 stackBasePosition = transform.position
            + transform.TransformDirection(stackOffset);
        return stackBasePosition + Vector3.up * (itemHeight * index);
    }

    public bool AddItem(MineralItem item)
    {
        if (IsFull())
        {
            Destroy(item.gameObject);
            return false;
        }
        chain.Add(item);
        return true;
    }

    public bool AddResultItem(ResultItem item)
    {
        if (IsFull())
        {
            Debug.Log("[ItemChain] 최대 보유량 도달!");
            return false;
        }
        chain.Add(item);
        return true;
    }

    public MineralItem PopItem()
    {
        for (int i = chain.Count - 1; i >= 0; i--)
        {
            if (chain[i] is MineralItem mineral)
            {
                chain.RemoveAt(i);
                return mineral;
            }
        }
        return null;
    }

    public ResultItem PopResultItem()
    {
        for (int i = chain.Count - 1; i >= 0; i--)
        {
            if (chain[i] is ResultItem result)
            {
                chain.RemoveAt(i);
                return result;
            }
        }
        return null;
    }

    public int GetCount() => chain.Count;
}