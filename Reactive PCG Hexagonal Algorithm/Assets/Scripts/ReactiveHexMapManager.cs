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
        BiomeType.Lake,
        BiomeType.Mountain,
        BiomeType.Swamp
    };
    public Dictionary<BiomeType, int> biomeInteractions = new Dictionary<BiomeType, int>();
    public Dictionary<BiomeType, float> biomeProbabilities = new Dictionary<BiomeType, float>();

    public static ReactiveHexMapManager Instance { get; private set; }

    //"Limiar de fechamento de malha. Quanto maior, mais restrita/fechada a propagação."
    public float closeGridThreshold = 0.15f;

    // Matriz de fatores primos para colunas PARES (gridPos.x % 2 == 0)
    private readonly int[,] POS_MAP_EVEN = new int[,] {
        { -1,  3, -1 },
        {  2, -1,  5 },
        { 13, 11,  7 }
    };

    // Matriz de fatores primos para colunas ÍMPARES (gridPos.x % 2 != 0)
    private readonly int[,] POS_MAP_ODD = new int[,] {
        {  2,  3,  5 },
        { 13, -1,  7 },
        { -1, 11, -1 }
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        
        // Dispara a propagação EPA a partir da posição inicial com a energia inicial
        PropagateEnergy(startPos, BiomeType.Grass, initialEnergy);
    }

    private int PropagateEnergy(Vector2Int gridPos, BiomeType biome, int energy)
    {
        // 1. Checagem de Limites da Matriz
        Tile currentTile = mapMatrix.GetTile(gridPos);
        if (currentTile == null) return 0;

        // 2. Condição de Parada por Energia
        if (energy <= 0) return 0;

        // 3. Fator Ruído / Restrição CLOSE_GRID
        if (currentTile.biomeType != BiomeType.None && Random.value < closeGridThreshold)
        {
            return 0;
        }

        // 4. Marca o terreno e atualiza o estado da célula
        currentTile.Initialize(gridPos, biome, energy);
        int v = energy;

        // 5. Chamada Recursiva para os 6 Vizinhos Hexagonais
        List<Vector2Int> neighbors = GetHexNeighbors(gridPos);
        int[] returnedEnergies = new int[6];

        for (int i = 0; i < 6; i++)
        {
            Vector2Int neighborPos = neighbors[i];
            Tile neighborTile = mapMatrix.GetTile(neighborPos);

            // Propaga apenas para posições dentro do mapa que sejam None ou do mesmo bioma
            if (neighborTile != null && (neighborTile.biomeType == BiomeType.None || neighborTile.biomeType == biome))
            {
                returnedEnergies[i] = PropagateEnergy(neighborPos, biome, energy - 1);
                
                // Atualiza a ponte de conexão lógica entre a célula atual e o vizinho
                UpdateCellsConnection(gridPos, neighborPos, returnedEnergies[i]);
            }
        }

        // 6. Atualização da Energia com o valor máximo retornado dos ramos
        int maxReturnedEnergy = v;
        for (int i = 0; i < 6; i++)
        {
            if (returnedEnergies[i] > maxReturnedEnergy)
            {
                maxReturnedEnergy = returnedEnergies[i];
            }
        }

        currentTile.currentEnergy = maxReturnedEnergy;
        return maxReturnedEnergy;
    }

    /// <summary>
    /// Atualiza a máscara numérica de conexões primas entre a célula atual e a vizinha
    /// </summary>
    private void UpdateCellsConnection(Vector2Int currentPos, Vector2Int neighborPos, int energy)
    {
        if (energy <= 0) return;

        int[] factors = MultiplyAroundHex(currentPos, neighborPos);
        if (factors[0] == -1 || factors[1] == -1) return;

        Tile currentTile = mapMatrix.GetTile(currentPos);
        Tile neighborTile = mapMatrix.GetTile(neighborPos);

        if (currentTile != null && neighborTile != null)
        {
            // Aplica os fatores primos nas máscaras numéricas de conexão
            currentTile.connectionMask *= factors[1];
            neighborTile.connectionMask *= factors[0];
        }
    }

    /// <summary>
    /// Determina os fatores primos baseados na posição relativa e na paridade da coluna X
    /// </summary>
    private int[] MultiplyAroundHex(Vector2Int currentPos, Vector2Int neighborPos)
    {
        int dx = neighborPos.x - currentPos.x + 1;
        int dy = neighborPos.y - currentPos.y + 1;

        // Validação de intervalo do kernel 3x3
        if (dx < 0 || dx > 2 || dy < 0 || dy > 2)
            return new int[] { -1, -1 };

        int[,] mapToUse = (currentPos.x % 2 == 0) ? POS_MAP_EVEN : POS_MAP_ODD;

        int factorNeighbor = mapToUse[dy, dx];     // Fator do vizinho
        int factorSelf = mapToUse[2 - dy, 2 - dx]; // Fator oposto (da célula atual)

        if (factorNeighbor == -1 || factorSelf == -1)
            return new int[] { -1, -1 };

        return new int[] { factorNeighbor, factorSelf };
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

    public void ProcessTileInteraction(Tile tile)
    {
        if (tile == null || tile.biomeType == BiomeType.None) return;
        
        if (tile.biomeType != BiomeType.Grass)
        {
            RegisterBiomeInteraction(tile.biomeType);
        }

        if (tile.isTrigger)
        {
            ExpandMapFromTrigger(tile);
        }
    }

    private void RegisterBiomeInteraction(BiomeType biome)
    {
        if (!biomeInteractions.ContainsKey(biome)) return;

        biomeInteractions[biome]++;

        UpdateBiomeProbabilities();
        FindObjectOfType<UIController>()?.UpdateUI();

        Debug.Log($"[Telemetria] Bioma {biome} interagido! Novo total: {biomeInteractions[biome]}");
    }

    private void UpdateBiomeProbabilities()
    {
        int totalInteractions = 0;

        foreach (var pair in biomeInteractions)
        {
            totalInteractions += pair.Value;
        }

        foreach (BiomeType biome in availableBiomes)
        {
            float probability = (float)biomeInteractions[biome] / totalInteractions;
            biomeProbabilities[biome] = probability;

            Debug.Log($"[Roleta] P({biome}): {probability * 100f:F1}%");
        }
    }

    private void ExpandMapFromTrigger(Tile triggerTile)
    {
        // 1. Consulta a Roleta Linear para sortear o bioma
        BiomeType selectedBiome = GetRandomBiomeFromRoulette();

        // 2. Calcula a energia dinâmica baseada na probabilidade P(bioma)
        int expansionEnergy = CalculateDynamicEnergy(selectedBiome);

        Debug.Log($"[Expansão] Bioma sorteado: {selectedBiome} | Energia Dinâmica: {expansionEnergy}");

        // Propaga a energia EPA a partir do ponto do trigger
        PropagateEnergy(triggerTile.gridPosition, selectedBiome, expansionEnergy);

        // 3. Analisa Adjacência (recalcula a densidade de conexões físicas)
        CalculateAdjacency();

        // 4. Aplica Poda Topológica (remove pontas soltas/isoladas)
        ApplyPruning(minRequiredNeighbors: 2);

        // 5. Injeta novos triggers via Filtro de Sobel (atualiza as bordas ativas)
        ApplyBoundaryFilter();
    }

    private BiomeType GetRandomBiomeFromRoulette()
    {
        float randomValue = Random.Range(0f, 1f);
        float cumulativeProbability = 0f;

        // Atualiza a posição do triângulo indicador na tela com o valor sorteado
        FindObjectOfType<UIController>()?.UpdateIndicatorPosition(randomValue);

        foreach (BiomeType biome in availableBiomes)
        {
            cumulativeProbability += biomeProbabilities[biome];
            if (randomValue <= cumulativeProbability)
            {
                return biome;
            }
        }

        // Fallback caso ocorra arredondamento de ponto flutuante
        return availableBiomes[availableBiomes.Count - 1];
    }

    private int CalculateDynamicEnergy(BiomeType biome)
    {
        float p = biomeProbabilities[biome];
        int dynamicEnergy = Mathf.RoundToInt(minEnergy + (maxEnergy - minEnergy) * p);
        return dynamicEnergy;
    }

    private void CalculateAdjacency()
    {
        for (int x = 0; x < mapMatrix.width; x++)
        {
            for (int y = 0; y < mapMatrix.height; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                Tile currentTile = mapMatrix.GetTile(currentPos);

                if (currentTile != null && currentTile.biomeType != BiomeType.None)
                {
                    int count = 0;
                    List<Vector2Int> neighbors = GetHexNeighbors(currentPos);

                    foreach (Vector2Int neighborPos in neighbors)
                    {
                        Tile neighborTile = mapMatrix.GetTile(neighborPos);
                        if (neighborTile != null && neighborTile.biomeType != BiomeType.None)
                        {
                            count++;
                        }
                    }

                    currentTile.neighborsCount = count;
                }
                else if (currentTile != null)
                {
                    currentTile.neighborsCount = 0;
                }
            }
        }
    }

    private void ApplyPruning(int minRequiredNeighbors = 2)
    {
        bool tilePruned = false;

        for (int x = 0; x < mapMatrix.width; x++)
        {
            for (int y = 0; y < mapMatrix.height; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                Tile currentTile = mapMatrix.GetTile(currentPos);

                // Se for um tile ativo com vizinhança abaixo do limiar (ex: 1 vizinho = ponta solta)
                if (currentTile != null && currentTile.biomeType != BiomeType.None)
                {
                    if (currentTile.neighborsCount < minRequiredNeighbors)
                    {
                        // Reseta o tile para o estado neutro/vazio (None)
                        currentTile.Initialize(currentPos, BiomeType.None, 0);
                        currentTile.connectionMask = 1;
                        tilePruned = true;
                    }
                }
            }
        }

        // Se alguma célula foi podada, recalcula a adjacência para refletir a nova topologia
        if (tilePruned)
        {
            CalculateAdjacency();
        }
    }

    /// <summary>
    /// Retorna a contagem total de interações de um bioma específico.
    /// </summary>
    public int GetBiomeCount(BiomeType biome)
    {
        if (biomeInteractions.TryGetValue(biome, out int count))
        {
            return count;
        }
        return 1; // Fallback para a neutralidade inicial
    }

    /// <summary>
    /// Retorna a probabilidade P(bioma) atual de um bioma específico (de 0.0 a 1.0).
    /// </summary>
    public float GetBiomeProbability(BiomeType biome)
    {
        if (biomeProbabilities.TryGetValue(biome, out float prob))
        {
            return prob;
        }
        return 0.25f; // Fallback equiprovável
    }
}