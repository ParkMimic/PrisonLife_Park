using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMining : MonoBehaviour
{
    [Header("채광 설정")]
    public float miningCooldown = 1.0f; // 광물 간 채광 간격

    float lastMiningTime = -999f; // 마지막 채광 시각
    ItemChain itemChain;

    private void Awake()
    {
        itemChain = GetComponent<ItemChain>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 꽉 찼으면 채광 시도 자체를 막음
        if (itemChain.IsFull())
        {
            Debug.Log("ItFull!!");
            return;
        }

        // 쿨타임 체크
        if (Time.time - lastMiningTime < miningCooldown) return;

        Mineral mineral = other.GetComponent<Mineral>();
        if (mineral == null) return;

        // 채광 실행
        mineral.Break(transform);
        lastMiningTime = Time.time; // 채광 시각 업데이트
    }
}
