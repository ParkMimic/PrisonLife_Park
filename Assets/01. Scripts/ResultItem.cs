using UnityEngine;
using System.Collections;

public class ResultItem : MonoBehaviour
{
    public float flyDuration = 0.3f;
    public float arcHeight = 1.5f;

    private bool isInitialized = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isInitialized) return;

        Init(other.transform);
    }

    public void Init(Transform playerTransform)
    {
        ItemChain chain = playerTransform.GetComponent<ItemChain>();

        isInitialized = true;

        Vector3 targetPos = chain.GetNextStackPosition();
        chain.AddResultItem(this);
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