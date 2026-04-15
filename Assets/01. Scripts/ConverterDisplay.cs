using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConverterDisplay : MonoBehaviour
{
    [Header("디스플레이 설정")]
    public GameObject mineralDisplayPrefab;
    public Transform displayBase;
    public float columnOffset = 0.4f;
    public float rowHeight = 0.4f;
    public int maxCount = 20;

    [Header("아이템 회전 설정")]
    public Vector3 mineralRotation = new Vector3(0f, 0f, 90f);

    [Header("위치 갱신 속도")]
    public float repositionSpeed = 10f;

    [Header("UI")]
    public Text fullText;

    private List<GameObject> displayItems = new List<GameObject>();

    void Start()
    {
        if (fullText != null) fullText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 인벤토리처럼: 아이템이 제거되면 나머지가 빈 자리로 이동
        displayItems.RemoveAll(item => item == null);

        for (int i = 0; i < displayItems.Count; i++)
        {
            Vector3 targetPos = GetPositionByIndex(i);
            displayItems[i].transform.position = Vector3.Lerp(
                displayItems[i].transform.position, targetPos, repositionSpeed * Time.deltaTime
            );
        }
    }

    // 인덱스로 목표 위치 계산 (내부 + GetNextPosition 공용)
    Vector3 GetPositionByIndex(int index)
    {
        int col = index % 2;
        int row = index / 2;
        float zOffset = (col == 0) ? -columnOffset : columnOffset;
        float yOffset = row * rowHeight;
        return displayBase.position + new Vector3(0f, yOffset, zOffset);
    }

    // 다음 아이템이 배치될 위치 반환
    public Vector3 GetNextPosition()
    {
        return GetPositionByIndex(displayItems.Count);
    }

    public bool IsFull() => displayItems.Count >= maxCount;

    // 날아온 오브젝트를 디스플레이에 등록. 꽉 찼으면 파괴하고 false 반환.
    public bool AddMineral(GameObject obj)
    {
        if (IsFull()) { Destroy(obj); return false; }
        obj.transform.rotation = Quaternion.Euler(mineralRotation);
        displayItems.Add(obj);
        UpdateFullUI();
        return true;
    }

    // 변환 시 뒤(상단)에서부터 count개 제거
    public void RemoveMineral(int count)
    {
        int removeCount = Mathf.Min(count, displayItems.Count);

        for (int i = 0; i < removeCount; i++)
        {
            int lastIndex = displayItems.Count - 1;
            Destroy(displayItems[lastIndex]);
            displayItems.RemoveAt(lastIndex);
        }

        UpdateFullUI();
    }

    void UpdateFullUI()
    {
        if (fullText != null) fullText.gameObject.SetActive(IsFull());
    }

    public void ClearDisplay()
    {
        foreach (var obj in displayItems)
            Destroy(obj);
        displayItems.Clear();
    }

    public int GetCount() => displayItems.Count;
}
