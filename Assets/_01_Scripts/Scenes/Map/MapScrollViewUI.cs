using Game.Scenes.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapScrollViewUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private MapNodeWidgetUI nodePrefab;

    [Header("Look")]
    [SerializeField] private bool groupByBiomeWithSeparators = true;
    [SerializeField] private float scrollToCurrentDuration = 0.15f;
    [SerializeField] private bool showOnlyCurrentBiome = true;


    private readonly List<MapNodeWidgetUI> _spawned = new();
    private MapNodeWidgetUI _currentWidget;

    void OnEnable()
    {
        Rebuild();
        ScrollToCurrent(immediate: true);
    }

    public void Rebuild()
    {
        Clear();

        var session = CoreManager.Instance.Session;
        var run = session.Run;

        if (run == null || run.MapDefinition == null || run.MapDefinition.biomes == null)
            return;

        // Guard: Run finished?
        if (run.BiomeIndex < 0 || run.BiomeIndex >= run.MapDefinition.biomes.Count)
            return;

        int b = showOnlyCurrentBiome ? run.BiomeIndex : 0;
        int bEnd = showOnlyCurrentBiome ? (run.BiomeIndex + 1) : run.MapDefinition.biomes.Count;

        int globalIndex = 0;

        // Optional: Wenn nur aktuelles Biome gezeigt wird, kann globalIndex = run.GlobalNodeIndex - run.NodeIndexInBiome sein
        if (showOnlyCurrentBiome)
            globalIndex = run.GlobalNodeIndex - run.NodeIndexInBiome;

        for (int biomeIndex = b; biomeIndex < bEnd; biomeIndex++)
        {
            var biomeMap = run.MapDefinition.biomes[biomeIndex];
            if (biomeMap.nodes == null) continue;

            for (int n = 0; n < biomeMap.nodes.Count; n++)
            {
                var node = biomeMap.nodes[n];

                var w = Instantiate(nodePrefab, contentRoot);

                w.SetNode(n + 1, biomeMap.biome, node.type);

                bool isCurrent = (biomeIndex == run.BiomeIndex && n == run.NodeIndexInBiome);
                bool isPast = showOnlyCurrentBiome ? (n < run.NodeIndexInBiome)
                                                   : ((biomeIndex < run.BiomeIndex) || (biomeIndex == run.BiomeIndex && n < run.NodeIndexInBiome));

                w.SetState(isCurrent, isPast);

                if (isCurrent) _currentWidget = w;

                _spawned.Add(w);
            }

        }
    }

    public void ScrollToCurrent(bool immediate)
    {
        if (_currentWidget == null || scrollRect == null) return;

        Canvas.ForceUpdateCanvases();

        // Calculate normalized position to center current widget
        var content = scrollRect.content;
        var viewport = scrollRect.viewport;

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        if (contentWidth <= viewportWidth) return;

        var target = (RectTransform)_currentWidget.transform;
        float targetCenterX = Mathf.Abs(target.anchoredPosition.x) + target.rect.width * 0.5f;

        float normalized = Mathf.Clamp01((targetCenterX - viewportWidth * 0.5f) / (contentWidth - viewportWidth));

        if (immediate)
        {
            scrollRect.horizontalNormalizedPosition = normalized;
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(SmoothScroll(normalized, scrollToCurrentDuration));
        }
    }

    private System.Collections.IEnumerator SmoothScroll(float target, float duration)
    {
        float start = scrollRect.horizontalNormalizedPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, target, k);
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = target;
    }

    private void Clear()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        _spawned.Clear();
        _currentWidget = null;
    }
}
