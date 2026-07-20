using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public BiomeType biomeType = BiomeType.None;
    public int currentEnergy = 0;   
    public int neighborsCount = 0;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    } 

    public void Initialize(Vector2Int position, BiomeType biome, int energy)
    {
        this.gridPosition = position;
        this.biomeType = biome;
        this.currentEnergy = energy;
        this.neighborsCount = 0;
        this.gameObject.name = $"Tile_{position.x}_{position.y} ({biome})";

        SetColorBiome(biome);
    }

    public void UpdateBiome(BiomeType newBiome)
    {
        this.biomeType = newBiome;
        SetColorBiome(newBiome);
    }

    private void SetColorBiome(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Grass:
                spriteRenderer.color = Color.green;
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
                spriteRenderer.color = Color.white; // Default color for None or uninitialized
                break;
        }
    }

}
