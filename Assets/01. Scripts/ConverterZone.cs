using UnityEngine;
using System.Collections;

public class ConverterZone : MonoBehaviour
{
    [Header("투입 설정")]
    public float insertInterval = 0.15f;

    [Header("참조 - Inspector에서 직접 연결")]
    public ConverterDisplay display;
    public ConverterProcessor processor;

    private ItemChain itemChain;
    private bool isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isProcessing) return;

        itemChain = other.GetComponent<ItemChain>();
        if (itemChain == null) return;

        // inputType에 따라 보유량 확인
        bool hasItems = processor.inputType == ConverterProcessor.InputType.Mineral
            ? itemChain.GetCount() > 0
            : itemChain.GetResultCount() > 0;

        if (!hasItems) return;

        if (display == null || processor == null)
        {
            Debug.LogError("[ConverterZone] display 또는 processor가 연결되지 않았어요!");
            return;
        }

        StartCoroutine(InsertRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        StopAllCoroutines();
        isProcessing = false;
    }

    IEnumerator InsertRoutine()
    {
        isProcessing = true;

        while (true)
        {
            // inputType에 따라 아이템 Pop
            if (processor.inputType == ConverterProcessor.InputType.Mineral)
            {
                MineralItem item = itemChain.PopItem();
                if (item == null) break;

                Vector3 targetPos = display.GetNextPosition();
                item.FlyTo(targetPos, () =>
                {
                    display.AddMineral(item.gameObject);
                    processor.OnItemInserted();
                });
            }
            else
            {
                ResultItem item = itemChain.PopResultItem();
                if (item == null) break;

                Vector3 targetPos = display.GetNextPosition();
                item.FlyTo(targetPos, () =>
                {
                    display.AddMineral(item.gameObject);
                    processor.OnItemInserted();
                });
            }

            yield return new WaitForSeconds(insertInterval);
        }

        isProcessing = false;
    }
}