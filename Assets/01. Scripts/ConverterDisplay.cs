using UnityEngine;
using System.Collections.Generic;

public class ConverterDisplay : MonoBehaviour
{
    [Header("디스플레이 설정")]
    public GameObject mineralDisplayPrefab;
    public Transform displayBase;
    public float columnOffset = 0.4f;
    public float rowHeight = 0.4f;

    private List<GameObject> displayItems = new List<GameObject>();

    public void AddMineral()
    {
        int index = displayItems.Count;
        int col = index % 2;
        int row = index / 2;

        float xOffset = (col == 0) ? -columnOffset : columnOffset;
        float yOffset = row * rowHeight;

        Vector3 targetPos = displayBase.position
            + new Vector3(xOffset, yOffset, 0f);

        GameObject obj = Instantiate(
            mineralDisplayPrefab, targetPos, Quaternion.identity);

        displayItems.Add(obj);
    }

    //  위에서부터 count개 제거
    public void RemoveMineral(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (displayItems.Count == 0) break;

            int lastIndex = displayItems.Count - 1;
            Destroy(displayItems[lastIndex]);
            displayItems.RemoveAt(lastIndex);
        }
    }

    public void ClearDisplay()
    {
        foreach (var obj in displayItems)
            Destroy(obj);
        displayItems.Clear();
    }

    public int GetCount() => displayItems.Count;
}