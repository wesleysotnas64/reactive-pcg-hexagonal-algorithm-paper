using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public BiomeType biomeType = BiomeType.None;
    public int currentEnergy = 0;   
    public int neighborsCount = 0;
    public bool isTrigger = false;
    public GameObject isTriggerIndicator;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    } 

    public void Initialize(Vector2Int position, BiomeType biome, int energy)
    {
        gridPosition = position;
        biomeType = biome;
        currentEnergy = energy;
        neighborsCount = 0;
        gameObject.name = $"Tile_{position.y}_{position.x} ({biome})";
        isTriggerIndicator.SetActive(isTrigger);
        SetColorBiome(biome);
    }

    public void UpdateBiome(BiomeType newBiome)
    {
        biomeType = newBiome;
        SetColorBiome(newBiome);
    }

    private void SetColorBiome(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Grass:
                spriteRenderer.color = new Color(0.3f, 0.8f, 0.4f);
                break;
            case BiomeType.Forest:
                spriteRenderer.color = new Color(0.0f, 0.5f, 0.0f); // Dark green
                break;
            case BiomeType.Mountain:
                spriteRenderer.color = Color.gray;
                break;
            case BiomeType.Lake:
                spriteRenderer.color = Color.blue;
                break;
            case BiomeType.Swamp:
                spriteRenderer.color = new Color(0.4f, 0.2f, 0.0f); // Brownish
                break;
            default:
                spriteRenderer.color = new Color(0.0f, 0.2f, 0.3f); // Default color for None or uninitialized
                break;
        }
    }

    public void SetTriggerState(bool isTrigger)
    {
        this.isTrigger = isTrigger;
        if (isTriggerIndicator != null)
        {
            isTriggerIndicator.SetActive(isTrigger);
        }
    }

}
