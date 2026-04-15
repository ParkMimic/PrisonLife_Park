using UnityEngine;
using System.Collections;
using Cinemachine;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    // ── 시네머신 ──────────────────────────────────────────────
    [Header("시네머신")]
    public CinemachineVirtualCamera[] virtualCameras;

    [Header("이벤트 타이밍")]
    public float cameraHoldDuration    = 2.0f; // 이벤트 카메라 유지 시간
    public float cameraBlendDuration   = 1.0f; // 카메라 전환 블렌드 대기 시간
    public float drillZoneActivateDelay = 0.5f; // 카메라 전환 후 드릴 존 활성화까지 대기 시간

    // ── 업그레이드 존 부모 오브젝트 ───────────────────────────
    [Header("업그레이드 존 루트")]
    public GameObject drillUpgradeZoneRoot;
    public GameObject minerSpawnZoneRoot;
    public GameObject counterSpawnZoneRoot;
    public GameObject jailUpgradeZoneRoot;

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        instance = this;
    }

    // ── 카메라 ────────────────────────────────────────────────

    /// <summary>지정한 인덱스의 Virtual Camera를 반환합니다.</summary>
    CinemachineVirtualCamera GetCamera(int index)
    {
        if (virtualCameras == null || index < 0 || index >= virtualCameras.Length) return null;
        return virtualCameras[index];
    }

    /// <summary>Virtual Camera의 FOV(Field of View)를 변경합니다.</summary>
    public void SetCameraFOV(int index, float fov)
    {
        var cam = GetCamera(index);
        if (cam == null) return;
        cam.m_Lens.FieldOfView = fov;
    }

    /// <summary>Virtual Camera의 우선순위를 변경합니다. (높을수록 활성화)</summary>
    public void SetCameraPriority(int index, int priority)
    {
        var cam = GetCamera(index);
        if (cam == null) return;
        cam.Priority = priority;
    }

    // ── 존 활성화 / 비활성화 ──────────────────────────────────

    public void SetDrillUpgradeZone(bool active)
    {
        if (drillUpgradeZoneRoot != null)
            drillUpgradeZoneRoot.SetActive(active);
    }

    public void SetMinerSpawnZone(bool active)
    {
        if (minerSpawnZoneRoot != null)
            minerSpawnZoneRoot.SetActive(active);
    }

    public void SetCounterSpawnZone(bool active)
    {
        if (counterSpawnZoneRoot != null)
            counterSpawnZoneRoot.SetActive(active);
    }

    public void SetJailUpgradeZone(bool active)
    {
        if (jailUpgradeZoneRoot != null)
            jailUpgradeZoneRoot.SetActive(active);
    }

    // ── 이벤트 ────────────────────────────────────────────────

    /// <summary>처음 돈을 얻었을 때 이벤트</summary>
    public void TriggerFirstMoneyEvent()
    {
        StartCoroutine(FirstMoneyRoutine());
    }

    IEnumerator FirstMoneyRoutine()
    {
        PlayerMovement movement = GameManager.instance.player.GetComponent<PlayerMovement>();

        // 1. 조작 정지
        if (movement != null) movement.controllable = false;

        // 2. Virtual Camera_1 으로 전환
        SetCameraPriority(0, 1);
        SetCameraPriority(1, 20);

        // 3. 블렌드 대기 후 드릴 존 활성화
        yield return new WaitForSeconds(cameraBlendDuration + drillZoneActivateDelay);
        SetDrillUpgradeZone(true);

        // 4. 남은 유지 시간 대기
        float remaining = cameraHoldDuration - drillZoneActivateDelay;
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        // 5. Virtual Camera 로 복귀
        SetCameraPriority(1, 1);
        SetCameraPriority(0, 10);

        // 6. 블렌드 대기 후 조작 재개
        yield return new WaitForSeconds(cameraBlendDuration);

        if (movement != null) movement.controllable = true;
    }

    /// <summary>처음으로 드릴 업그레이드 완료 시 이벤트</summary>
    public void TriggerFirstDrillUpgradeEvent()
    {
        SetMinerSpawnZone(true);
        SetCounterSpawnZone(true);
        SetJailUpgradeZone(true);
    }

    /// <summary>감옥 업그레이드 완료 시 이벤트</summary>
    public void TriggerJailUpgradeEvent()
    {
        StartCoroutine(JailUpgradeRoutine());
    }

    IEnumerator JailUpgradeRoutine()
    {
        PlayerMovement movement = GameManager.instance.player.GetComponent<PlayerMovement>();

        // 1. 조작 정지
        if (movement != null) movement.controllable = false;

        // 2. Virtual Camera_3 으로 전환
        SetCameraPriority(0, 1);
        SetCameraPriority(3, 20);

        // 3. 블렌드 + 유지
        yield return new WaitForSeconds(cameraBlendDuration + cameraHoldDuration);

        // 4. Virtual Camera 로 복귀
        SetCameraPriority(3, 1);
        SetCameraPriority(0, 10);

        // 5. 블렌드 대기 후 조작 재개
        yield return new WaitForSeconds(cameraBlendDuration);

        if (movement != null) movement.controllable = true;
    }

    /// <summary>수감시설이 꽉 찼을 때 이벤트</summary>
    public void TriggerPrisonFullEvent()
    {
        StartCoroutine(PrisonFullRoutine());
    }

    IEnumerator PrisonFullRoutine()
    {
        PlayerMovement movement = GameManager.instance.player.GetComponent<PlayerMovement>();

        // 1. 조작 정지
        if (movement != null) movement.controllable = false;

        // 2. Virtual Camera_3 으로 전환 (인덱스 3)
        SetCameraPriority(0, 1);
        SetCameraPriority(3, 20);

        // 3. 블렌드 + 유지
        yield return new WaitForSeconds(cameraBlendDuration + cameraHoldDuration);

        // 4. Virtual Camera 로 복귀
        SetCameraPriority(3, 1);
        SetCameraPriority(0, 10);

        // 5. 블렌드 대기 후 조작 재개
        yield return new WaitForSeconds(cameraBlendDuration);

        if (movement != null) movement.controllable = true;
    }
}
