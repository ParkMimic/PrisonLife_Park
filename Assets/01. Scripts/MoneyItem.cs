using UnityEngine;
using System.Collections;

public class MoneyItem : MonoBehaviour
{
    [Header("설정")]
    public float flyDuration = 0.3f;
    public float arcHeight = 1.5f;
    public int value = 5;

    public void Init(Transform playerTransform)
    {
        ItemChain chain = playerTransform.GetComponent<ItemChain>();
        if (chain == null || chain.IsFull()) return;

        transform.SetParent(null);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Vector3 targetPos = chain.GetNextGroupPosition("money");
        if (!chain.AddMoneyItem(this)) return;

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
            transform.position = linearPos + Vector3.up * arcHeight * Mathf.Sin(Mathf.PI * t);
            yield return null;
        }

        transform.position = targetPos;
    }
}
