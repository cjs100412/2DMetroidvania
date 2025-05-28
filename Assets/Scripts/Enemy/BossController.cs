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

    [Header("���� ���࿡ �� ���� ����Ʈ")]
    public Transform projectileSpawnPoint;

    List<IBossPattern> patterns;
    DashBoss dashBoss;
    public bool isBusy;

    void Awake()
    {
        Player = GameObject.FindWithTag("Player").transform;
        // SO ����Ʈ�� IBossPattern ����Ʈ�� ��ȯ
        patterns = patternSOs
            .OfType<IBossPattern>()
            .ToList();

        Debug.Log($"[Debug] Awake �� patterns loaded: {patterns.Count}");
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
            Debug.Log("[Debug] Update �� isBusy, skip");
            return;
        }

        // 2) ������ �׾�����
        if (dashBoss.isDead)
        {
            Debug.Log("[Debug] Update �� isDead, skip");
            return;
        }

        float dist = Vector2.Distance(transform.position, Player.position);
        Debug.Log($"[Debug] Update �� dist={dist:0.00}");

        // ���� ���� ���ϸ� ���͸�
        var available = patterns
            .Where(p =>
            {
                bool ok = p.CanExecute(this, Player);
                Debug.Log($"[Debug]    �� {p.GetType().Name}.CanExecute = {ok}");
                return ok;
            })
            .ToArray();

        Debug.Log($"[Debug] Update �� available.Length = {available.Length}");
        if (available.Length == 0) return;

        // ���� �Ǵ� �켱���� �������� �ϳ� ����
        var choice = available[Random.Range(0, available.Length)];
        Debug.Log($"[Debug] Update �� Starting Pattern: {choice.GetType().Name}");
        StartCoroutine(RunPattern(choice));
    }

    IEnumerator RunPattern(IBossPattern pat)
    {
        Debug.Log($"[Debug] RunPattern ENTER: {pat.GetType().Name}");
        isBusy = true;

        yield return StartCoroutine(pat.Execute(this, Player));
        // ���� �� ª�� ����
        Debug.Log($"[Debug] RunPattern EXIT: {pat.GetType().Name}");
        yield return new WaitForSeconds(0.2f);
        isBusy = false;
    }
}