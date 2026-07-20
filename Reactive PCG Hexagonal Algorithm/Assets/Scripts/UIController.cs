using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Text UI References")]
    public TMP_Text forestCountText;
    public TMP_Text lakeCountText;
    public TMP_Text mountainCountText;
    public TMP_Text swampCountText;

    [Header("Percentage Bar RectTransforms")]
    public RectTransform forestImageRect;
    public RectTransform lakeImageRect;
    public RectTransform mountainImageRect;
    public RectTransform swampImageRect;

    [Header("Interactions Count")]
    private int forestCount = 1;
    private int lakeCount = 1;
    private int mountainCount = 1;
    private int swampCount = 1;

    [Header("Biome Probabilities (Percentages)")]
    private float forestPercentage = 0.25f;
    private float lakePercentage = 0.25f;
    private float mountainPercentage = 0.25f;
    private float swampPercentage = 0.25f;

    [Header("Current Biome Indicator")]
    public RectTransform currentBiomeIndicator;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (ReactiveHexMapManager.Instance == null) return;

        // 1. Atualiza as contagens
        forestCount = ReactiveHexMapManager.Instance.GetBiomeCount(BiomeType.Forest);
        lakeCount = ReactiveHexMapManager.Instance.GetBiomeCount(BiomeType.Lake);
        mountainCount = ReactiveHexMapManager.Instance.GetBiomeCount(BiomeType.Mountain);
        swampCount = ReactiveHexMapManager.Instance.GetBiomeCount(BiomeType.Swamp);

        // 2. Obtém as probabilidades (0.0 a 1.0)
        forestPercentage = ReactiveHexMapManager.Instance.GetBiomeProbability(BiomeType.Forest);
        lakePercentage = ReactiveHexMapManager.Instance.GetBiomeProbability(BiomeType.Lake);
        mountainPercentage = ReactiveHexMapManager.Instance.GetBiomeProbability(BiomeType.Mountain);
        swampPercentage = ReactiveHexMapManager.Instance.GetBiomeProbability(BiomeType.Swamp);

        // 3. Atualiza os textos
        if (forestCountText != null) forestCountText.text = forestCount.ToString();
        if (lakeCountText != null) lakeCountText.text = lakeCount.ToString();
        if (mountainCountText != null) mountainCountText.text = mountainCount.ToString();
        if (swampCountText != null) swampCountText.text = swampCount.ToString();

        // 4. Atualiza a barra proporcional de porcentagem na UI
        UpdatePercentageBar();
    }

    /// <summary>
    /// Ajusta os Anchors X acumulativamente e zera Left/Right (offsetMin/offsetMax).
    /// </summary>
    private void UpdatePercentageBar()
    {
        float currentAnchorX = 0f;

        // 1. Floresta
        currentAnchorX = SetSegmentAnchors(forestImageRect, currentAnchorX, forestPercentage);

        // 2. Lago
        currentAnchorX = SetSegmentAnchors(lakeImageRect, currentAnchorX, lakePercentage);

        // 3. Montanha
        currentAnchorX = SetSegmentAnchors(mountainImageRect, currentAnchorX, mountainPercentage);

        // 4. Pântano
        currentAnchorX = SetSegmentAnchors(swampImageRect, currentAnchorX, swampPercentage);
    }

    /// <summary>
    /// Configura o inicio/fim do Anchor X para um segmento da barra e zera Left/Right.
    /// </summary>
    private float SetSegmentAnchors(RectTransform rect, float startAnchorX, float percentage)
    {
        if (rect == null) return startAnchorX;

        float endAnchorX = startAnchorX + percentage;

        // Define os limites horizontais dos Anchors (mantendo Y de 0 a 1)
        rect.anchorMin = new Vector2(startAnchorX, 0f);
        rect.anchorMax = new Vector2(endAnchorX, 1f);

        // Zera o Left (offsetMin.x) e o Right (-offsetMax.x)
        rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
        rect.offsetMax = new Vector2(0f, rect.offsetMax.y);

        return endAnchorX;
    }

    public void UpdateIndicatorPosition(float randomValue)
    {
        if (currentBiomeIndicator == null) return;

        // Mapeia o valor de 0..1 para a faixa do painel (0.5 até 0.95)
        float targetAnchorX = Mathf.Lerp(0.5f, 0.95f, randomValue);

        // Configura min e max X para o mesmo valor para manter o pivô centralizado
        currentBiomeIndicator.anchorMin = new Vector2(targetAnchorX, currentBiomeIndicator.anchorMin.y);
        currentBiomeIndicator.anchorMax = new Vector2(targetAnchorX, currentBiomeIndicator.anchorMax.y);

        // Zera o PosX para alinhar com o AnchorX e garante PosY = 0
        currentBiomeIndicator.anchoredPosition = new Vector2(0f, 0f);
    }
}