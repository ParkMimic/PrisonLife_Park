using UnityEngine;

/// <summary>
/// 광물 지역 전체를 감싸는 트리거 콜라이더에 부착.
/// 플레이어가 안에 있는 동안 채굴 모드 ON, 빠져나가면 OFF.
/// </summary>
public class MiningZone : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        PlayerUpgrade upgrade = other.GetComponent<PlayerUpgrade>();
        if (upgrade == null) return;

        upgrade.EnterMiningMode();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerUpgrade upgrade = other.GetComponent<PlayerUpgrade>();
        if (upgrade == null) return;

        upgrade.ExitMiningMode();
    }
}
