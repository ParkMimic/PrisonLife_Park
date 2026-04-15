using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("채굴 사운드")]
    public AudioClip miningDefault;
    public AudioClip miningDrill;

    [Header("수령 사운드")]
    public AudioClip receiveClip;
    public AudioClip receiveMoneyClip;

    [Header("투입 사운드")]
    public AudioClip  insertClip;
    public AudioSource insertAudioSource; // 전용 AudioSource (Inspector에서 연결)
    public float insertPitchStep  = 0.05f; // 연속 투입 시 피치 증가량
    public float insertPitchMax   = 2.0f;  // 최대 피치
    public float pitchResetDelay  = 0.5f;  // 이 시간 동안 투입 없으면 피치 초기화

    private AudioSource audioSource;
    private PlayerUpgrade playerUpgrade;

    private float currentInsertPitch = 1.0f;
    private float lastInsertTime     = -999f;

    private void Awake()
    {
        audioSource   = GetComponent<AudioSource>();
        playerUpgrade = GetComponent<PlayerUpgrade>();

        audioSource.playOnAwake = false;
    }

    // ── 채굴 ─────────────────────────────────────────────────

    public void PlayMiningSound()
    {
        AudioClip clip = (playerUpgrade != null && playerUpgrade.DrillLevel >= 1)
            ? miningDrill
            : miningDefault;

        audioSource.pitch = 1.0f;
        PlaySound(clip);
    }

    // ── 수령 ─────────────────────────────────────────────────

    public void PlayReceiveSound()
    {
        audioSource.pitch = 1.0f;
        PlaySound(receiveClip);
    }

    public void PlayReceiveMoneySound()
    {
        audioSource.pitch = 1.0f;
        PlaySound(receiveMoneyClip);
    }

    // ── 투입 ─────────────────────────────────────────────────

    public void PlayInsertSound()
    {
        if (insertClip == null || insertAudioSource == null) return;

        // 일정 시간 투입 없으면 피치 초기화
        if (Time.time - lastInsertTime > pitchResetDelay)
            currentInsertPitch = 1.0f;

        insertAudioSource.pitch = currentInsertPitch;
        insertAudioSource.PlayOneShot(insertClip);

        currentInsertPitch = Mathf.Min(currentInsertPitch + insertPitchStep, insertPitchMax);
        lastInsertTime     = Time.time;
    }

    // ── 공용 재생 ────────────────────────────────────────────

    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
