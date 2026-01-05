using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public ObjectSpawner objectSpawner;
    public IsometricCharacter playerCharacter;

    public Tilemap baseTilemap;

    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("Layer Settings")]
    public List<GameObject> mapLayers = new List<GameObject>(); // 7 katman objesi

    [Header("Round Settings")]
    public int currentRound = 1;
    public int maxRounds = 3;

    // Round baþýna düþman sayýsý
    private int[] enemiesPerRound = { 1, 2, 3 };

    // Round baþýna toplam obje sayýsý
    private int[] collectiblesPerRound = { 3, 4, 5 };

    // Round'da spawn edilmiþ obje sayýsý
    private int spawnedCollectiblesThisRound = 0;

    // Round'da kaç turn geçti
    private int turnsInCurrentRound = 0;

    private List<Tilemap> activeTilemaps = new List<Tilemap>();

    void Start()
    {
        if (baseTilemap == null && mapLayers.Count > 0)
        {
            baseTilemap = mapLayers[0].GetComponent<Tilemap>();
        }

        StartRound(1);
    }

    public void StartRound(int roundNumber)
    {
        currentRound = roundNumber;
        spawnedCollectiblesThisRound = 0;
        turnsInCurrentRound = 0;

        Debug.Log($"===== ROUND {currentRound} BAÞLADI =====");

        // Katmanlarý aktif et
        ActivateLayers(roundNumber);

        // Düþmanlarý spawn et
        SpawnEnemies(enemiesPerRound[roundNumber - 1]);

        // Ýlk objeyi hemen spawn et
        SpawnCollectibleForRound();
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
                Vector3 worldPos = baseTilemap.GetCellCenterWorld(spawnPos);

                GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

                // Hierarchy organize
                Transform charContainer = GameObject.Find("--- CHARACTERS ---")?.transform;
                if (charContainer != null) enemyObj.transform.SetParent(charContainer);

                enemyObj.name = $"Enemy_{i + 1}_Round{currentRound}";

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

    Vector3Int GetRandomEnemySpawnPosition()
    {
        // 1. ADIM: TÜM KATMANLARI GEZÝP EN GENÝÞ SINIRLARI BULALIM
        // Baþlangýç için mantýksýz deðerler veriyoruz ki ilk tilemap bunlarý güncellesin
        int xMin = int.MaxValue, xMax = int.MinValue;
        int yMin = int.MaxValue, yMax = int.MinValue;
        bool foundAnyLayer = false;

        foreach (GameObject layerObj in mapLayers)
        {
            if (layerObj == null) continue;

            Tilemap tm = layerObj.GetComponent<Tilemap>();
            if (tm == null) continue;

            tm.CompressBounds(); // Boþluklarý temizle
            BoundsInt b = tm.cellBounds;

            // Bu katman boþsa atla
            if (b.size.x <= 0 || b.size.y <= 0) continue;

            // Sýnýrlarý geniþlet (Union iþlemi)
            if (b.xMin < xMin) xMin = b.xMin;
            if (b.xMax > xMax) xMax = b.xMax;
            if (b.yMin < yMin) yMin = b.yMin;
            if (b.yMax > yMax) yMax = b.yMax;

            foundAnyLayer = true;
        }

        // Eðer hiç katman yoksa veya hepsi boþsa hata ver
        if (!foundAnyLayer)
        {
            Debug.LogError("HATA: Hiçbir katmanda tile bulunamadý!");
            return Vector3Int.zero;
        }

        // 2. ADIM: BULDUÐUMUZ GENÝÞ SINIRLAR ÝÇÝNDE TARAMA YAPALIM
        List<Vector3Int> validPositions = new List<Vector3Int>();
        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        // Debug.Log($"Geniþ Tarama: X({xMin} to {xMax}), Y({yMin} to {yMax})");

        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // A. ZEMÝN KONTROLÜ (Herhangi bir katmanda zemin var mý?)
                if (!HasTileOnAnyActiveLayer(pos))
                    continue;

                // B. MESAFE KONTROLÜ
                int distance = Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y);
                if (distance < 2)
                    continue;

                // C. DOLULUK KONTROLÜ
                if (IsPositionOccupied(pos))
                    continue;

                validPositions.Add(pos);
            }
        }

        Debug.Log($"Tarama Sonucu: {validPositions.Count} uygun yer bulundu.");

        if (validPositions.Count > 0)
        {
            return validPositions[Random.Range(0, validPositions.Count)];
        }

        Debug.LogError("HATA: Yer bulunamadý! Harita çok küçük veya dolu olabilir.");
        return Vector3Int.zero;
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
        // Oyuncu
        if (playerCharacter.GetCurrentGridPosition() == position)
            return true;

        // Düþmanlar
        foreach (IsometricEnemy enemy in turnManager.enemies)
        {
            if (enemy != null && enemy.GetCurrentGridPosition() == position)
                return true;
        }

        return false;
    }

    // Her turn'da çaðrýlacak
    public void OnTurnComplete()
    {
        turnsInCurrentRound++;
        SpawnCollectibleForRound();
    }

    void SpawnCollectibleForRound()
    {
        int maxCollectibles = collectiblesPerRound[currentRound - 1];

        // Bu round'da yeterli obje spawn edildi mi?
        if (spawnedCollectiblesThisRound >= maxCollectibles)
            return;

        // %40 þans ile obje spawn et (her turn'da kontrol edilir)
        if (Random.value < 0.4f)
        {
            if (objectSpawner != null)
            {
                objectSpawner.SpawnCollectible();
                spawnedCollectiblesThisRound++;
                Debug.Log($"Round {currentRound} obje spawn: {spawnedCollectiblesThisRound}/{maxCollectibles}");
            }
        }
    }

    // Tüm düþmanlar öldüðünde çaðrýlacak
    public void OnAllEnemiesDefeated()
    {
        Debug.Log($"===== ROUND {currentRound} TAMAMLANDI =====");

        if (currentRound < maxRounds)
        {
            // Bir sonraki round'a geç
            Invoke("StartNextRound", 2f); // 2 saniye bekle
        }
        else
        {
            // 3. round bitti - Level 2'ye geç
            Invoke("LoadLevel2", 2f);
        }
    }

    void StartNextRound()
    {
        StartRound(currentRound + 1);
    }

    void LoadLevel2()
    {
        Debug.Log("===== LEVEL 2'YE GEÇÝLÝYOR =====");
        // TODO: Level 2 yükleme kodu
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }
}