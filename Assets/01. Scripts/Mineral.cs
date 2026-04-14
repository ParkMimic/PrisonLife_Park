using System.Collections;
using UnityEngine;

public class Mineral : MonoBehaviour
{
    [Header("설정")]
    public GameObject itemPrefab;
    public float respawnTime = 10f;

    private bool isBroken = false;
    private bool isRespawning = false;
    private MineralSpawner spawner;

    // 채굴 가능 여부 (광부 AI가 타겟 선정 시 사용)
    public bool IsAvailable => !isBroken && !isRespawning;

    public void Init(MineralSpawner spawner)
    {
        this.spawner = spawner;
    }

    // 광물이 플레이어 범위 안에 있는 동안 매 물리 프레임마다 호출
    private void OnTriggerStay(Collider other)
    {
        if (isBroken || isRespawning) return;

        PlayerInteraction player = other.GetComponent<PlayerInteraction>();
        if (player == null) return;

        if (player.TryMine())
            Break(player.transform);
    }

    public void Break(Transform playerTransform)
    {
        if (isBroken) return;

        if (spawner == null)
            spawner = GameManager.instance.mineralSpawner;

        if (spawner == null)
        {
            Debug.LogError($"[Mineral] MineralSpawner를 찾을 수 없습니다! ({gameObject.name})");
            return;
        }

        isBroken = true;

        if (itemPrefab != null)
        {
            GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            item.GetComponent<MineralItem>()?.Init(playerTransform);
        }

        gameObject.SetActive(false);
        spawner.StartRespawn(this, respawnTime);
    }

    // 광부 AI 전용: 아이템을 직접 변환기로 보냄
    public void BreakByAI(System.Action<MineralItem> onItemCreated)
    {
        if (!IsAvailable) return;

        if (spawner == null)
            spawner = GameManager.instance.mineralSpawner;

        if (spawner == null)
        {
            Debug.LogError($"[Mineral] MineralSpawner를 찾을 수 없습니다! ({gameObject.name})");
            return;
        }

        isBroken = true;

        if (itemPrefab != null)
        {
            GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            onItemCreated?.Invoke(item.GetComponent<MineralItem>());
        }

        gameObject.SetActive(false);
        spawner.StartRespawn(this, respawnTime);
    }

    public void Respawn()
    {
        isBroken = false;
        isRespawning = true;
        gameObject.SetActive(true);
        StartCoroutine(ScaleIn());
    }

    IEnumerator ScaleIn()
    {
        transform.localScale = Vector3.zero;
        Vector3 oriScale = new Vector3(3.5f, 3.5f, 3.5f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.localScale = Vector3.Lerp(Vector3.zero, oriScale, t);
            yield return null;
        }
        transform.localScale = oriScale;
        isRespawning = false;
    }
}
