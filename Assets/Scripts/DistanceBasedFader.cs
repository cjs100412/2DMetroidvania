using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DistanceBasedFader : MonoBehaviour
{
    private Transform player;

    [Header("이정표(페이드 기준 위치)")]
    public Transform helpTransform;

    public float fadeStartDistance = 2f;
    public float fadeEndDistance = 5f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        canvasGroup = GetComponent<CanvasGroup>();
        if (helpTransform == null) helpTransform = transform.parent;
    }

    void Update()
    {
        if (player == null || helpTransform == null) return;

        transform.position = helpTransform.position + Vector3.up * 1.5f;

        float dist = Vector2.Distance(player.position, helpTransform.position);
        float t = Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, dist);
        canvasGroup.alpha = 1f - Mathf.Clamp01(t);
    }
}