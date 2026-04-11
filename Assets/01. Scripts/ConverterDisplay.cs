using UnityEngine;
using System.Collections.Generic;

public class ConverterDisplay : MonoBehaviour
{
    [Header("디스플레이 설정")]
    public GameObject mineralDisplayPrefab;
    public Transform displayBase;
    public float columnOffset = 0.4f;
    public float rowHeight = 0.4f;

    [Header("광물 회전 설정")]
    public Vector3 mineralRotation = new Vector3(0f, 0f, 90f);

    private List<GameObject> displayItems = new List<GameObject>();

    // 다음 착지 위치 반환
    public Vector3 GetNextPosition()
    {
        int index = displayItems.Count;
        int col = index % 2;
        int row = index / 2;

        float zOffset = (col == 0) ? -columnOffset : columnOffset;
        float yOffset = row * rowHeight;

        return displayBase.position + new Vector3(0f, yOffset, zOffset);
    }

    // 날아온 오브젝트를 그대로 등록
    public void AddMineral(GameObject obj)
    {
        obj.transform.rotation = Quaternion.Euler(mineralRotation); 
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