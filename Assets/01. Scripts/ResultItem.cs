using UnityEngine;
using System.Collections;

public class ResultItem : MonoBehaviour
{
    public float flyDuration = 0.3f;
    public float arcHeight = 1.5f;

    private bool isInitialized = false;

    // 스폰 지점에서 픽업될 때 ConverterProcessor에 알리는 콜백
    public System.Action onPickedUp;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isInitialized) return;

        Init(other.transform);
    }

    public void Init(Transform playerTransform)
    {
        if (isInitialized) return;  // 이미 픽업 처리 중이거나 완료된 아이템 재호출 방지

        ItemChain chain = playerTransform.GetComponent<ItemChain>();

        isInitialized = true;

        Vector3 targetPos = chain.GetNextStackPosition();
        if (!chain.AddResultItem(this))
        {
            isInitialized = false;  // 실패 시 다시 픽업 가능하게 복구
            return;
        }

        onPickedUp?.Invoke();   // 체인 추가 성공 후에만 스폰 지점 리스트에서 제거
        onPickedUp = null;
        GameManager.instance.AddHandcuff();
        StartCoroutine(FlyRoutine(targetPos));
    }

    public void FlyTo(Vector3 targetPos, System.Action onComplete)
    {
        isInitialized = false;

        // 비행 중 플레이어 트리거에 재진입해서 체인에 다시 추가되는 것을 방지
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(FlyToRoutine(targetPos, onComplete));
    }

    IEnumerator FlyToRoutine(Vector3 targetPos, System.Action onComplete)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = linearPos + Vector3.up * arc;

            yield return null;
        }

        transform.position = targetPos;
        onComplete?.Invoke();
    }

    IEnumerator FlyRoutine(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = linearPos + Vector3.up * arc;

            yield return null;
        }

        transform.position = targetPos;
    }
}
