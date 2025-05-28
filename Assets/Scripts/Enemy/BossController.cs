using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossController : MonoBehaviour
{
    [Header("Components")]
    public Animator Animator;
    Transform Player;

    [Header("Patterns (ScriptableObjects)")]
    public List<ScriptableObject> patternSOs;

    [Header("패턴 실행에 쓸 스폰 포인트")]
    public Transform projectileSpawnPoint;

    List<IBossPattern> patterns;
    DashBoss dashBoss;
    public bool isBusy;

    void Awake()
    {
        Player = GameObject.FindWithTag("Player").transform;
        // SO 리스트를 IBossPattern 리스트로 변환
        patterns = patternSOs
            .OfType<IBossPattern>()
            .ToList();

        Debug.Log($"[Debug] Awake → patterns loaded: {patterns.Count}");
    }

    private void Start()
    {
        Animator = GetComponent<Animator>();
        dashBoss = GetComponent<DashBoss>();
    }

    void Update()
    {
        if (isBusy)
        {
            Debug.Log("[Debug] Update → isBusy, skip");
            return;
        }

        // 2) 보스가 죽었는지
        if (dashBoss.isDead)
        {
            Debug.Log("[Debug] Update → isDead, skip");
            return;
        }

        float dist = Vector2.Distance(transform.position, Player.position);
        Debug.Log($"[Debug] Update → dist={dist:0.00}");

        // 실행 가능 패턴만 필터링
        var available = patterns
            .Where(p =>
            {
                bool ok = p.CanExecute(this, Player);
                Debug.Log($"[Debug]    → {p.GetType().Name}.CanExecute = {ok}");
                return ok;
            })
            .ToArray();

        Debug.Log($"[Debug] Update → available.Length = {available.Length}");
        if (available.Length == 0) return;

        // 랜덤 또는 우선순위 로직으로 하나 선택
        var choice = available[Random.Range(0, available.Length)];
        Debug.Log($"[Debug] Update → Starting Pattern: {choice.GetType().Name}");
        StartCoroutine(RunPattern(choice));
    }

    IEnumerator RunPattern(IBossPattern pat)
    {
        Debug.Log($"[Debug] RunPattern ENTER: {pat.GetType().Name}");
        isBusy = true;

        yield return StartCoroutine(pat.Execute(this, Player));
        // 패턴 간 짧은 공백
        Debug.Log($"[Debug] RunPattern EXIT: {pat.GetType().Name}");
        yield return new WaitForSeconds(0.2f);
        isBusy = false;
    }
}