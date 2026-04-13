using UnityEngine;
using System.Collections;

public class ResultItem : MonoBehaviour
{
    [Header("날아가기 설정")]
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

        if (chain == null)
        {
            Debug.LogError("[ResultItem] Player에 ItemChain이 없어요!");
            return;
        }

        if (chain.IsFull())
        {
            Debug.Log("[ResultItem] 최대 보유량 도달!");
            return;
        }

        isInitialized = true;

        Vector3 targetPos = chain.GetNextStackPosition();
        chain.AddResultItem(this);
        StartCoroutine(FlyRoutine(targetPos));
    }

    // ConverterZone에서 호출하는 FlyTo 추가
    public void FlyTo(Vector3 targetPos, System.Action onComplete)
    {
        isInitialized = false;
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