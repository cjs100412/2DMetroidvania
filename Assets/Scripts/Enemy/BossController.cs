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
    [Tooltip("여기에 FarRangeProjectilePattern, CloseRangeKnockbackPattern 등을 드래그")]
    public List<ScriptableObject> patternSOs;

    List<IBossPattern> patterns;
    bool isBusy;

    void Awake()
    {
        Player = GameObject.FindWithTag("Player").transform;
        // SO 리스트를 IBossPattern 리스트로 변환
        patterns = patternSOs
            .OfType<IBossPattern>()
            .ToList();
    }

    private void Start()
    {
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isBusy) return;

        // 실행 가능 패턴만 필터링
        var available = patterns
            .Where(p => p.CanExecute(this, Player))
            .ToArray();

        if (available.Length == 0) return;

        // 랜덤 또는 우선순위 로직으로 하나 선택
        var choice = available[Random.Range(0, available.Length)];
        StartCoroutine(RunPattern(choice));
    }

    IEnumerator RunPattern(IBossPattern pat)
    {
        isBusy = true;
        yield return StartCoroutine(pat.Execute(this, Player));
        // 패턴 간 짧은 공백
        yield return new WaitForSeconds(0.2f);
        isBusy = false;
    }
}