using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralItem : MonoBehaviour
{
    // 플레이어가 처음 등록
    public void Init(Transform playerTransform)
    {
        // ItemChain에 자신을 등록하고 target을 받아옴
        ItemChain chain = playerTransform.GetComponent <ItemChain>();

        if (chain == null)
        {
            Debug.LogError("[MineralItem] Player에 ItemChain이 없어요!");
            return;
        }

        chain.AddItem(this);
    } 
}
