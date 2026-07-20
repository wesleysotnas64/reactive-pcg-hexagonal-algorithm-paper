using System.Collections.Generic;
using UnityEngine;

public class ReactiveHexMapManager : MonoBehaviour
{
    public MapMatrix mapMatrix;
    public int width;
    public int height;
    public int minEnergy;
    public int maxEnergy;
    public int initialEnergy;
    public List<BiomeType> availableBiomes = new()
    {
        BiomeType.Forest,
        BiomeType.Mountain,
        BiomeType.Lake,
        BiomeType.Swamp
    };
    public Dictionary<BiomeType, int> biomeInteractions = new Dictionary<BiomeType, int>();
    public Dictionary<BiomeType, float> biomeProbabilities = new Dictionary<BiomeType, float>();

    void Start()
    {
        RunBlock01();
    }

    private void RunBlock01()
    {
        InitializeRoulette();
        mapMatrix.InitializeMatrix(width, height);
        GenerateBaseMap();
        ApplyBoundaryFilter();
    }

    private void InitializeRoulette()
    {
        biomeInteractions.Clear();
        biomeProbabilities.Clear();

        float initialProbability = 1f / availableBiomes.Count;

        foreach (BiomeType biome in availableBiomes)
        {
            biomeInteractions.Add(biome, 1);
            biomeProbabilities.Add(biome, initialProbability);
        }
    }

    private void GenerateBaseMap()
    {
        // Variáveis de posição inicial (por enquanto no centro)
        int xInit = mapMatrix.width / 2;
        int yInit = mapMatrix.height / 2;

        Vector2Int startPos = new Vector2Int(xInit, yInit);
        
        // Dispara a propagação a partir da posição inicial com a energia inicial (ex: 3)
        PropagateEnergy(startPos, BiomeType.Grass, initialEnergy);
    }

    private void PropagateEnergy(Vector2Int currentPos, BiomeType biome, int energy)
    {
        Tile currentTile = mapMatrix.GetTile(currentPos);

        // Caso base: Se o tile for inválido ou a energia for negativa, encerra
        if (currentTile == null || energy < 0) return;

        // Se o tile já possui uma energia maior ou igual gravada, não sobrescreve (evita loop infinito)
        if (currentTile.biomeType == biome && currentTile.currentEnergy >= energy) return;

        // Atribui o bioma e a energia atual
        currentTile.Initialize(currentPos, biome, energy);

        // Se a energia zerou, atinge o limite do raio de expansão nesta ramificação
        if (energy == 0) return;

        // Obtém as coordenadas dos 6 vizinhos hexagonais
        List<Vector2Int> neighbors = GetHexNeighbors(currentPos);

        // Propaga recursivamente para cada vizinho reduzindo a energia em 1
        foreach (Vector2Int neighborPos in neighbors)
        {
            PropagateEnergy(neighborPos, biome, energy - 1);
        }
    }

    private List<Vector2Int> GetHexNeighbors(Vector2Int gridPos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Deslocamento para colunas PARES (x % 2 == 0)
        int[,] evenOffsets = new int[,] {
            { 0, -1 }, { 1, -1 }, { 1, 0 },
            { 0, 1 },  {-1, 0 }, {-1, -1 }
        };

        // Deslocamento para colunas ÍMPARES (x % 2 != 0)
        int[,] oddOffsets = new int[,] {
            { 0, -1 }, { 1, 0 }, { 1, 1 },
            { 0, 1 },  {-1, 1 }, {-1, 0 }
        };

        int[,] selectedOffsets = (gridPos.x % 2 == 0) ? evenOffsets : oddOffsets;

        for (int i = 0; i < 6; i++)
        {
            int nx = gridPos.x + selectedOffsets[i, 0];
            int ny = gridPos.y + selectedOffsets[i, 1];

            neighbors.Add(new Vector2Int(nx, ny));
        }

        return neighbors;
    }

    private void ApplyBoundaryFilter()
    {
        for (int x = 0; x < mapMatrix.width; x++)
        {
            for (int y = 0; y < mapMatrix.height; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                Tile currentTile = mapMatrix.GetTile(currentPos);

                if (currentTile != null && currentTile.biomeType != BiomeType.None)
                {
                    bool isEdge = false;
                    List<Vector2Int> neighbors = GetHexNeighbors(currentPos);

                    foreach (Vector2Int neighborPos in neighbors)
                    {
                        Tile neighborTile = mapMatrix.GetTile(neighborPos);

                        if (neighborTile == null || neighborTile.biomeType == BiomeType.None)
                        {
                            isEdge = true;
                            break; 
                        }
                    }

                    currentTile.SetTriggerState(isEdge);
                }
                else if (currentTile != null)
                {
                    currentTile.SetTriggerState(false);
                }
            }
        }
    }
}