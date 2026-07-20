using UnityEngine;

public class MapMatrix : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject tilePrefab;
    public float hexRadius = 0.5f; 
    public float gap = 0.05f; 

    private Tile[,] matrix;
    private Vector3 worldOffset;

    void Start()
    {
        CalculateWorldOffset();
        InitializeMatrix();
    }

    private void CalculateWorldOffset()
    {
        float xSize = (hexRadius * 2f) + gap;
        float ySize = (hexRadius * Mathf.Sqrt(3f)) + gap;

        float totalWidth = (width - 1) * (xSize * 0.75f);
        float totalHeight = (height - 1) * ySize;

        worldOffset = new Vector3(-totalWidth / 2f, totalHeight / 2f, 0f);
    }

    public void InitializeMatrix()
    {
        matrix = new Tile[width, height];

        // Varredura: Cima -> Baixo
        for (int y = 0; y < height; y++)
        {
            // Varredura: Esquerda -> Direita
            for (int x = 0; x < width; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                
                // Instancia o tile neutro/vazio na matriz
                Tile newTile = CreateTileInstance(gridPos, BiomeType.None, 0);
                matrix[x, y] = newTile;
            }
        }
    }

    private Tile CreateTileInstance(Vector2Int gridPos, BiomeType biome, int energy)
    {
        Vector3 worldPosition = CalculateHexWorldPosition(gridPos);
        GameObject tileObj = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
        Tile tile = tileObj.GetComponent<Tile>();

        tile.Initialize(gridPos, biome, energy);
        
        return tile;
    }

    private Vector3 CalculateHexWorldPosition(Vector2Int gridPos)
    {
        float xSize = (hexRadius * 2f) + gap;
        float ySize = (hexRadius * Mathf.Sqrt(3f)) + gap;

        // X cresce para a direita
        float xPos = gridPos.x * (xSize * 0.75f);
        
        // Y cresce para BAIXO (subtraindo no eixo Y do mundo)
        float yPos = -gridPos.y * ySize;

        // Deslocamento vertical intercalado nas colunas ímpares (Odd-R Hex Grid)
        if (gridPos.x % 2 != 0)
        {
            yPos -= ySize * 0.5f;
        }

        // Aplica o offset para centralizar o mapa na tela
        return new Vector3(xPos, yPos, 0f) + worldOffset;
    }

    public Tile GetTile(Vector2Int gridPos)
    {
        if (gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height)
        {
            return matrix[gridPos.x, gridPos.y];
        }
        return null;
    }
}