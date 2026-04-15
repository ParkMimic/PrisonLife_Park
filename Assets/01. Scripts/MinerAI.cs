using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 광부 AI: 광물을 자동으로 채굴하여 즉시 변환기로 전달한다.
/// NavMeshAgent 필요. 최대 3마리 소환 가능.
/// </summary>
public class MinerAI : MonoBehaviour
{
    [Header("참조 (소환 시 자동 설정 or Inspector 직접 연결)")]
    public MineralSpawner mineralSpawner;
    public ConverterDisplay converterDisplay;
    public ConverterProcessor converterProcessor;

    [Header("설정")]
    public float miningRange     = 1.5f;   // 채굴 시작 거리
    public float miningDelay     = 0.5f;   // 채굴 선딜레이
    public float searchInterval  = 0.5f;   // 광물 없을 때 재탐색 간격

    [Header("사운드")]
    public AudioClip miningClip;
    [Range(0f, 1f)] public float volume = 1f;

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;
    private Mineral targetMineral;

    // 인스턴스 간 공유: 이미 다른 AI가 타겟팅한 광물 중복 방지
    private static readonly HashSet<Mineral> claimedMinerals = new HashSet<Mineral>();

    // ── 초기화 ────────────────────────────────────────────────

    private void Awake()
    {
        agent       = GetComponent<NavMeshAgent>();
        anim        = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        anim?.SetBool("isWalking", agent.velocity.sqrMagnitude > 0.01f);
    }

    private void Start()
    {
        if (mineralSpawner     == null) mineralSpawner     = GameManager.instance.mineralSpawner;
        if (converterDisplay   == null) converterDisplay   = GameManager.instance.converterDisplay;
        if (converterProcessor == null) converterProcessor = GameManager.instance.converterProcessor;

        StartCoroutine(MiningLoop());
    }

    private void OnDestroy()
    {
        ReleaseClaim();
    }

    // ── 메인 루프 ─────────────────────────────────────────────

    IEnumerator MiningLoop()
    {
        while (true)
        {
            // 1. 채굴 가능한 광물 탐색
            targetMineral = FindNearestMineral();

            if (targetMineral == null)
            {
                yield return new WaitForSeconds(searchInterval);
                continue;
            }

            claimedMinerals.Add(targetMineral);

            // 2. 광물로 이동
            agent.isStopped = false;
            agent.SetDestination(targetMineral.transform.position);

            while (true)
            {
                // 광물이 비활성화되면 (다른 루트로 채굴됨) 포기
                if (targetMineral == null || !targetMineral.IsAvailable)
                    break;

                if (Vector3.Distance(transform.position, targetMineral.transform.position) <= miningRange)
                    break;

                yield return null;
            }

            agent.isStopped = true;

            // 3. 변환기가 꽉 찼으면 빌 때까지 대기
            while (converterDisplay != null && converterDisplay.IsFull())
                yield return new WaitForSeconds(searchInterval);

            // 4. 채굴
            if (targetMineral != null && targetMineral.IsAvailable)
            {
                yield return new WaitForSeconds(miningDelay);

                if (audioSource != null && miningClip != null)
                    audioSource.PlayOneShot(miningClip, volume);

                targetMineral.BreakByAI(item =>
                {
                    if (item == null) return;

                    Vector3 targetPos = converterDisplay.GetNextPosition();
                    item.FlyTo(targetPos, () =>
                    {
                        converterDisplay.AddMineral(item.gameObject);
                        converterProcessor.OnItemInserted();
                    });
                });
            }

            ReleaseClaim();
            yield return new WaitForSeconds(searchInterval);
        }
    }

    // ── 유틸 ──────────────────────────────────────────────────

    Mineral FindNearestMineral()
    {
        if (mineralSpawner == null) return null;

        Mineral nearest  = null;
        float   minDist  = float.MaxValue;

        foreach (Mineral mineral in mineralSpawner.minerals)
        {
            if (mineral == null)              continue;
            if (!mineral.IsAvailable)         continue;
            if (claimedMinerals.Contains(mineral)) continue;

            float dist = Vector3.Distance(transform.position, mineral.transform.position);
            if (dist < minDist)
            {
                minDist  = dist;
                nearest  = mineral;
            }
        }

        return nearest;
    }

    void ReleaseClaim()
    {
        if (targetMineral != null)
        {
            claimedMinerals.Remove(targetMineral);
            targetMineral = null;
        }
    }
}
