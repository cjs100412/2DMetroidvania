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


    List<IBossPattern> patterns;
    public bool isBusy;

    IBossDeath deathCheck;
    IProjectileSpawner spawner;

    void Awake()
    {
        Player = GameObject.FindWithTag("Player").transform;
        // SO ����Ʈ�� IBossPattern ����Ʈ�� ��ȯ
        patterns = patternSOs
            .OfType<IBossPattern>()
            .ToList();
        deathCheck = GetComponent<IBossDeath>();
        spawner = GetComponent<IProjectileSpawner>();
        Debug.Log($"[Debug] Awake �� patterns loaded: {patterns.Count}");
    }

    private void Start()
    {
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isBusy)
        {
            return;
        }

         if (deathCheck != null && deathCheck.IsDead) return;

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

        foreach (var p in available)
        {
            if (p is ISpawnPattern spawnPat)
                spawnPat.SetSpawnPoint(spawner.ProjectileSpawnPoint);
        }

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