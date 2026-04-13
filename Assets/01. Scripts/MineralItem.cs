using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralItem : MonoBehaviour
{
    [Header("���ư��� ����")]
    public float flyDuration = 0.4f; // ���ư��� �ð�
    public float arcHeight = 2.0f; // ������ ����

    private bool isInitialized = false; // �ʱ�ȭ ����

    // �÷��̾ ó�� ���
    public void Init(Transform playerTransform)
    {
        // ItemChain�� �ڽ��� ����ϰ� target�� �޾ƿ�
        ItemChain chain = playerTransform.GetComponent <ItemChain>();

        if (chain == null)
        {
            Debug.LogError("[MineralItem] Player�� ItemChain�� �����!");
            return;
        }

        if (!chain.AddItem(this)) return;
        GameManager.instance.AddMineral();
        isInitialized = true;
    }

    public void FlyTo(Vector3 targetPos, System.Action onComplete)
    {
        // ���ư��� ���� ItemChain ���� �ߴ�
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

            // ������ ���
            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = linearPos + Vector3.up * arc;

            yield return null;
        }

        transform.position = targetPos;

        // ���� �Ϸ� �ݹ�
        onComplete?.Invoke();
    }
}
