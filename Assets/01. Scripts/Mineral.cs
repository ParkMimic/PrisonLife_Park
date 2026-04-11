using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Mineral : MonoBehaviour
{
    [Header("설정")]
    public GameObject itemPrefab; // 드롭할 작은 광물 아이템
    public float respawnTime = 10f; // 리스폰 대기 시간

    private bool isBroken = false;
    private MineralSpawner spawner;

    public void Init(MineralSpawner spawner)
    {
        this.spawner = spawner;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!other.CompareTag("Player")) return;
    //    Break(other.transform);
    //}

    // 광물 파괴 호출
    public void Break(Transform playerTransform)
    {
        if (isBroken) return;

        // spawner가 null이면 스스로 찾기
        if (spawner == null)
            spawner = FindFirstObjectByType<MineralSpawner>();

        if (spawner == null)
        {
            Debug.LogError($"[Mineral] MineralSpawner를 찾을 수 없어요! ({gameObject.name})");
            return;
        }

        isBroken = true;

        if (itemPrefab != null)
        {
            // 작은 광물 아이템 드롭
            GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            item.GetComponent<MineralItem>()?.Init(playerTransform);
        }

        gameObject.SetActive(false);
        spawner.StartRespawn(this, respawnTime);
    }

    public void Respawn()
    {
        isBroken = false;
        gameObject.SetActive(true);
        StartCoroutine(ScaleIn());
    }

    System.Collections.IEnumerator ScaleIn()
    {
        transform.localScale = Vector3.zero;
        Vector3 oriScale = new Vector3(3.5f, 3.5f, 3.5f);
        float t = 0f;
        while (t < 3.5f)
        {
            t += Time.deltaTime * 2f;
            transform.localScale = Vector3.Lerp(Vector3.zero, oriScale, t);
            yield return null;
        }
        transform.localScale = oriScale;
    }
}
