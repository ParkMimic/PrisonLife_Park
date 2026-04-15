using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralItem : MonoBehaviour
{
    public float flyDuration = 0.4f;
    public float arcHeight = 2.0f;

    public void Init(Transform playerTransform)
    {
        ItemChain chain = playerTransform.GetComponent<ItemChain>();

        if (!chain.AddItem(this)) return;
        GameManager.instance.AddMineral();
    }

    public void FlyTo(Vector3 targetPos, System.Action onComplete)
    {
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

            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = linearPos + Vector3.up * arc;

            yield return null;
        }

        transform.position = targetPos;

        onComplete?.Invoke();
    }
}
