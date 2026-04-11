using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralItem : MonoBehaviour
{
    [Header("날아가기 설정")]
    public float flyDuration = 0.4f; // 날아가는 시간
    public float arcHeight = 2.0f; // 포물선 높이

    private bool isInitialized = false; // 초기화 여부

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
        isInitialized = true;
    }

    public void FlyTo(Vector3 targetPos, System.Action onComplete)
    {
        // 날아가는 동안 ItemChain 추적 중단
        isInitialized = false;
        StartCoroutine(FlyRoutine(targetPos, onComplete));
    }

    IEnumerator FlyRoutine(Vector3 targetPos, System.Action onComplete)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            // 포물선 계산
            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = linearPos + Vector3.up * arc;

            yield return null;
        }

        transform.position = targetPos;

        // 착지 완료 콜백
        onComplete?.Invoke();
    }
}
