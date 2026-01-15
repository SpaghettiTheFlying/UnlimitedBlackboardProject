using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("References")]
    public TurnManager turnManager;
    public ObjectSpawner objectSpawner;
    public IsometricCharacter playerCharacter;
    public Tilemap baseTilemap;

    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("Layer Settings")]
    public List<GameObject> mapLayers = new List<GameObject>(); // 7 katman objesi

    [Header("Level Settings")]
    public int currentLevel = 1;
    public int difficultyIncrease = 1;

    [Header("Round Settings")]
    public int currentRound = 1;
    public int maxRounds = 3;

    // Round baþýna düþman sayýsý
    private int[] baseEnemiesPerRound = { 1, 2, 3 };

    private int maxCollectiblesPerRound = 2;

    // Round'da spawn edilmiþ obje sayýsý
    private int spawnedCollectiblesThisRound = 0;

    // Round'da kaç turn geçti
    private int turnsInCurrentRound = 0;

    public List<Tilemap> activeTilemaps = new List<Tilemap>();

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (baseTilemap == null && mapLayers.Count > 0)
        {
            baseTilemap = mapLayers[0].GetComponent<Tilemap>();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.musicSource.loop = true;

            SoundManager.Instance.PlayMusic(SoundManager.Instance.gameMusic);
        }

        StartGame();
    }

    void StartGame()
    {
        currentLevel = 1;
        StartRound(1);
    }

    public void StartRound(int roundNumber)
    {
        currentRound = roundNumber;
        spawnedCollectiblesThisRound = 0;
        turnsInCurrentRound = 0;

        Debug.Log($"===== LEVEL {currentLevel} - ROUND {currentRound} BAÞLADI =====");

        ActivateLayers(roundNumber);

        if(roundNumber == 1)
        {
            TeleportPlayerToPeak();
        }
       
        int enemyCount = CalculateEnemyCount(roundNumber);
        SpawnEnemies(enemyCount);

        SpawnCollectibleForRound(true);

        if (turnManager != null)
        {
            turnManager.isPlayerTurn = true;
            Debug.Log("Sýra Oyuncuda");
        }
    }

    int CalculateEnemyCount(int round)
    {
        int index = Mathf.Clamp(round - 1, 0, baseEnemiesPerRound.Length - 1);
        int baseCount = baseEnemiesPerRound[index];
        int extraEnemies = (currentLevel - 1) * difficultyIncrease;

        return baseCount + extraEnemies;
    }

    void ActivateLayers(int roundNumber)
    {
        int layersToActivate = 0;

        switch (roundNumber)
        {
            case 1:
                layersToActivate = 3; 
                break;
            case 2:
                layersToActivate = 5; 
                break;
            case 3:
                layersToActivate = 7; 
                break;
            default:
                layersToActivate = mapLayers.Count;
                break;
        }

        activeTilemaps.Clear();
        if (playerCharacter != null)
        {
            playerCharacter.allTilemaps.Clear();
        }


        for (int i = 0; i < mapLayers.Count; i++)
        {
            if (mapLayers[i] != null)
            {
                bool shouldBeActive = i < layersToActivate;
                mapLayers[i].SetActive(shouldBeActive);
                
                if(shouldBeActive)
                {
                    Tilemap tm = mapLayers[i].GetComponent<Tilemap>();
                    if(tm != null)
                    {
                        activeTilemaps.Add(tm);

                        if (playerCharacter != null)
                        {
                            playerCharacter.allTilemaps.Add(tm);
                        }
                    }
                }
            }
        }

        if (playerCharacter != null)
        {
            playerCharacter.UpdateMainTilemapRef();
        }

    }

    void SpawnEnemies(int count)
    {
        if (enemyPrefab == null || baseTilemap == null || playerCharacter == null)
        {
            Debug.LogError("Enemy prefab, tilemap veya player character eksik!");
            return;
        }

        // TurnManager'daki düþman listesini temizle
        turnManager.enemies.Clear();

        for (int i = 0; i < count; i++)
        {
            Vector3Int spawnPos = GetRandomEnemySpawnPosition();

            if (spawnPos != Vector3Int.zero)
            {
                Vector3 worldPos = GetWorldPositionOnTop(spawnPos);

                GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

                // Hierarchy organize
                Transform charContainer = GameObject.Find("--- CHARACTERS ---")?.transform;
                if (charContainer != null) enemyObj.transform.SetParent(charContainer);

                enemyObj.name = $"Enemy_{currentLevel}_Round{currentRound}_{i}";

                IsometricEnemy enemy = enemyObj.GetComponent<IsometricEnemy>();
                if (enemy != null)
                {
                    enemy.allTilemaps = new List<Tilemap>(activeTilemaps);
                    enemy.UpdateMainTilemapRef();
                    turnManager.enemies.Add(enemy);
                }

                Debug.Log($"Düþman spawn edildi: {spawnPos}");
            }
        }
    }
    Vector3 GetWorldPositionOnTop(Vector3Int pos)
    {
        // Önce bu X,Y koordinatýndaki en yüksek Z'li tile'ý bul
        foreach (Tilemap tm in activeTilemaps)
        {
            // Yukarýdan aþaðý tara (2'den -2'ye)
            for (int z = 2; z >= -2; z--)
            {
                Vector3Int checkPos = new Vector3Int(pos.x, pos.y, z);
                if (tm.HasTile(checkPos))
                {
                    return tm.GetCellCenterWorld(checkPos);
                }
            }
        }
        // Bulamazsa varsayýlaný döndür
        return baseTilemap.GetCellCenterWorld(pos);
    }

    public BoundsInt GetTotalBounds()
    {
        if (activeTilemaps.Count == 0) return new BoundsInt();

        int xMin = int.MaxValue, xMax = int.MinValue;
        int yMin = int.MaxValue, yMax = int.MinValue;
        bool foundBounds = false;

        foreach (var tm in activeTilemaps)
        {
            tm.CompressBounds(); // Boþluklarý sýkýþtýr
            if (tm.cellBounds.size.x > 0 && tm.cellBounds.size.y > 0)
            {
                if (tm.cellBounds.xMin < xMin) xMin = tm.cellBounds.xMin;
                if (tm.cellBounds.xMax > xMax) xMax = tm.cellBounds.xMax;
                if (tm.cellBounds.yMin < yMin) yMin = tm.cellBounds.yMin;
                if (tm.cellBounds.yMax > yMax) yMax = tm.cellBounds.yMax;
                foundBounds = true;
            }
        }

        if (!foundBounds) return baseTilemap.cellBounds; // Yedek

        return new BoundsInt(new Vector3Int(xMin, yMin, 0), new Vector3Int(xMax - xMin, yMax - yMin, 1));
    }


    Vector3Int GetRandomEnemySpawnPosition()
    {

        BoundsInt bounds = GetTotalBounds();

        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        List<Vector3Int> bestSpots = new List<Vector3Int>();
        List<Vector3Int> mediumSpots = new List<Vector3Int>();
        List<Vector3Int> anyValidSpots = new List<Vector3Int>();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Zemin kontrolü
                if (!HasGroundAt(pos)) continue;

                // Doluluk kontrolü
                if (IsPositionOccupied(pos)) continue;

                // Mesafe Hesapla
                int distance = Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y);

                if (distance >= 6) bestSpots.Add(pos);
                else if (distance >= 4) mediumSpots.Add(pos);
                else if (distance >= 2) anyValidSpots.Add(pos);
            }
        }

        // Debug Raporu
        if (bestSpots.Count + mediumSpots.Count + anyValidSpots.Count == 0)
        {
            Debug.LogError($"HATA: Tarama yapýldý ama uygun yer bulunamadý! Taranan Alan: {bounds}");
            return Vector3Int.zero;
        }

        if (bestSpots.Count > 0) return bestSpots[Random.Range(0, bestSpots.Count)];
        if (mediumSpots.Count > 0) return mediumSpots[Random.Range(0, mediumSpots.Count)];
        if (anyValidSpots.Count > 0) return anyValidSpots[Random.Range(0, anyValidSpots.Count)];

        return Vector3Int.zero;
    }
    bool HasGroundAt(Vector3Int pos)
    {
        foreach (var tm in activeTilemaps)
        {
            for (int z = -2; z <= 2; z++)
            {
                if (tm.HasTile(new Vector3Int(pos.x, pos.y, z))) return true;
            }
        }
        return false;
    }
    bool IsPositionValidForSpawn(Vector3Int pos, Vector3Int playerPos)
    {
        // Herhangi bir katmanda burada zemin var mý?
        bool hasGround = false;
        foreach (var tm in activeTilemaps)
        {
            // Z taramasý
            for (int z = -2; z <= 2; z++)
            {
                if (tm.HasTile(new Vector3Int(pos.x, pos.y, z))) { hasGround = true; break; }
            }
        }
        if (!hasGround) return false;

        // Mesafe Kontrolü
        if (Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y) < 4) return false;

        // Doluluk Kontrolü (Sadece X,Y'ye bakar)
        if (playerCharacter.GetCurrentGridPosition().x == pos.x && playerCharacter.GetCurrentGridPosition().y == pos.y) return false;

        foreach (var enemy in turnManager.enemies)
        {
            if (enemy != null && enemy.GetCurrentGridPosition().x == pos.x && enemy.GetCurrentGridPosition().y == pos.y) return false;
        }

        return true;
    }
    bool HasTileOnAnyActiveLayer(Vector3Int pos)
    {
        foreach (Tilemap tm in activeTilemaps)
        {
            if (tm.HasTile(pos)) return true;
        }
        return false;
    }
    bool IsPositionOccupied(Vector3Int position)
    {
        if (playerCharacter.GetCurrentGridPosition().x == position.x && playerCharacter.GetCurrentGridPosition().y == position.y)
            return true;

        foreach (IsometricEnemy enemy in turnManager.enemies)
        {
            if (enemy != null && enemy.GetCurrentGridPosition().x == position.x && enemy.GetCurrentGridPosition().y == position.y)
                return true;
        }
        return false;
    }

    // Her turn'da çaðrýlacak
    public void OnTurnComplete()
    {
        turnsInCurrentRound++;
        SpawnCollectibleForRound(false);
    }

    void SpawnCollectibleForRound(bool forceSpawn = false)
    {
        if (spawnedCollectiblesThisRound >= maxCollectiblesPerRound)
            return;

        // Eðer forceSpawn true ise (Raunt baþý) spawn et.
        // Deðilse %40 þansla spawn et.
        if (forceSpawn || Random.value < 0.4f)
        {
            if (objectSpawner != null)
            {
                objectSpawner.SpawnCollectible();
                spawnedCollectiblesThisRound++;
                Debug.Log($"Obje Çýktý ({spawnedCollectiblesThisRound}/{maxCollectiblesPerRound})");
            }
        }
    }

    // Tüm düþmanlar öldüðünde çaðrýlacak
    public void OnAllEnemiesDefeated()
    {
        Debug.Log($"===== ROUND {currentRound} TAMAMLANDI =====");

        if (currentRound < maxRounds)
        {
            Invoke("StartNextRound", 2f);
        }
        else
        {
            // LEVEL TAMAMLANDI (Round 3 Bitti)
            Debug.Log($"*** LEVEL {currentLevel} BÝTTÝ! SONRAKÝ LEVEL HAZIRLANIYOR... ***");

            // --- YENÝ: Level Bitiþ Sesi Çal ---
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.levelCompleteSound);
            }
            // ----------------------------------

            Invoke("StartNextLevel", 3f);
        }
    }

    void StartNextRound()
    {
        StartRound(currentRound + 1);
    }

    void StartNextLevel()
    {
        currentLevel++;
        StartRound(1); // Yeni levelin 1. roundundan baþla
    }
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    public int GetCurrentRound()
    {
        return currentRound;
    }

    void TeleportPlayerToPeak()
    {
        if (playerCharacter == null || activeTilemaps.Count == 0) return;

        BoundsInt bounds = GetTotalBounds();

        // En üst katmandan aþaðýya (Layer 7 -> Layer 1) tara
        for (int i = activeTilemaps.Count - 1; i >= 0; i--)
        {
            Tilemap currentLayer = activeTilemaps[i];
            List<Vector3Int> peakTiles = new List<Vector3Int>();

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (int z = 2; z >= -2; z--)
                    {
                        if (currentLayer.HasTile(new Vector3Int(x, y, z)))
                        {
                            peakTiles.Add(new Vector3Int(x, y, z));
                            goto NextCell;
                        }
                    }
                NextCell:;
                }
            }

            if (peakTiles.Count == 0) continue;

            // Zirvedeki taþlardan merkeze en yakýn olaný seç
            Vector3Int bestSpot = peakTiles[0];
            Vector3 worldPos = currentLayer.GetCellCenterWorld(bestSpot);

            // --- HASSAS YÜKSEKLÝK AYARI ---
            // i * 0.5f : Katman yüksekliði (Merdiven etkisi)
            // + 0.3f   : Taþýn kalýnlýðý (Karakter içine girmesin, üstüne bassýn)
            float totalHeightOffset = (i * 0.5f) + 0.3f;

            worldPos.y += totalHeightOffset;
            worldPos.z -= 1f; // Sýralama için biraz öne al

            playerCharacter.TeleportToGridPosition(bestSpot, worldPos);
            Debug.Log($"Oyuncu zirveye kondu. Yükseklik: {totalHeightOffset}");

            return;
        }
    }
}