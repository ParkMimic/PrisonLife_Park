using UnityEngine;

public class ResultPickupZone : MonoBehaviour, IInteractable
{
    public void OnPlayerEnter(PlayerInteraction player)
    {
        if (player.ItemChain.IsFull()) return;

        ResultItem[] items = GetComponentsInChildren<ResultItem>();
        foreach (var item in items)
        {
            if (player.ItemChain.IsFull()) break;
            item.Init(player.transform);
        }
    }

    public void OnPlayerExit(PlayerInteraction player) { }
}
