using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [Header("광물 가득 참 UI")]
    public GameObject fullUI;   // 꽉 찼을 때 표시할 UI 오브젝트

    private Camera mainCamera;
    private ItemChain itemChain;

    void Start()
    {
        mainCamera = Camera.main;
        itemChain = GetComponentInParent<ItemChain>();

        if (fullUI != null)
            fullUI.SetActive(false);
    }

    void LateUpdate()
    {
        // 카메라 정면 바라보기
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);

        // 광물 소지 개수가 꽉 차면 UI 활성화
        if (fullUI != null && itemChain != null)
            fullUI.SetActive(itemChain.IsMineralFull());
    }
}
