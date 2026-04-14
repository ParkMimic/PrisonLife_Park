using UnityEngine;

/// <summary>
/// 거대 드릴 자식 트리거에 부착.
/// 부모의 PlayerInteraction을 통해 광물 채굴을 위임한다.
/// </summary>
public class MiningTriggerProxy : MonoBehaviour
{
    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        playerInteraction = GetComponentInParent<PlayerInteraction>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (playerInteraction == null) return;

        Mineral mineral = other.GetComponent<Mineral>();
        if (mineral == null) return;

        if (playerInteraction.TryMine())
            mineral.Break(playerInteraction.transform);
    }
}
