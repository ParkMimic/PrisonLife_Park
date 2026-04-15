using UnityEngine;

public class ResultPickupZone : MonoBehaviour, IInteractable
{
    public void OnPlayerEnter(PlayerInteraction player)
    {
        if (player.ItemChain.IsResultFull()) return;

        ResultItem[] items = GetComponentsInChildren<ResultItem>();
        int picked = 0;
        foreach (var item in items)
        {
            if (player.ItemChain.IsResultFull()) break;
            item.Init(player.transform);
            picked++;
        }

        if (picked > 0)
            player.GetComponent<PlayerAudio>()?.PlayReceiveSound();
    }

    public void OnPlayerExit(PlayerInteraction player) { }
}
