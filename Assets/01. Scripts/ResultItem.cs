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

        //  픽업 시 Processor에 카운트 감소 알림
        ConverterProcessor processor = FindFirstObjectByType<ConverterProcessor>();
        processor?.OnResultPickedUp();

        //  현재 스택 개수 기준으로 바로 뒤에 배치
        Vector3 targetPos = chain.GetNextStackPosition();
        chain.AddResultItem(this);  //  먼저 등록 후 날아가기

        StartCoroutine(FlyRoutine(targetPos));
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

            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 360f * t));

            yield return null;
        }

        //  착지 후 ItemChain Update가 자동으로 위치 관리
    }
}