using UnityEngine;

public class ResultPickupZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ItemChain chain = other.GetComponent<ItemChain>();
        if (chain == null) return;
        if (chain.IsFull()) return;

        // 트리거 범위 안의 모든 ResultItem 픽업
        ResultItem[] items = GetComponentsInChildren<ResultItem>();
        foreach (var item in items)
        {
            if (chain.IsFull()) break;
            item.Init(other.transform);
        }
    }
}