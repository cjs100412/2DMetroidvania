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
    [Tooltip("���⿡ FarRangeProjectilePattern, CloseRangeKnockbackPattern ���� �巡��")]
    public List<ScriptableObject> patternSOs;

    List<IBossPattern> patterns;
    bool isBusy;

    void Awake()
    {
        Player = GameObject.FindWithTag("Player").transform;
        // SO ����Ʈ�� IBossPattern ����Ʈ�� ��ȯ
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

        // ���� ���� ���ϸ� ���͸�
        var available = patterns
            .Where(p => p.CanExecute(this, Player))
            .ToArray();

        if (available.Length == 0) return;

        // ���� �Ǵ� �켱���� �������� �ϳ� ����
        var choice = available[Random.Range(0, available.Length)];
        StartCoroutine(RunPattern(choice));
    }

    IEnumerator RunPattern(IBossPattern pat)
    {
        isBusy = true;
        yield return StartCoroutine(pat.Execute(this, Player));
        // ���� �� ª�� ����
        yield return new WaitForSeconds(0.2f);
        isBusy = false;
    }
}