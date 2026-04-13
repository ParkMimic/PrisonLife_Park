using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("채굴 설정")]
    public float miningCooldown = 1.0f;

    public ItemChain ItemChain { get; private set; }

    private float lastMiningTime = -999f;

    private void Awake()
    {
        ItemChain = GetComponent<ItemChain>();
    }

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<IInteractable>()?.OnPlayerEnter(this);
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<IInteractable>()?.OnPlayerExit(this);
    }

    public bool TryMine()
    {
        if (ItemChain.IsFull()) return false;
        if (Time.time - lastMiningTime < miningCooldown) return false;
        lastMiningTime = Time.time;
        return true;
    }
}
