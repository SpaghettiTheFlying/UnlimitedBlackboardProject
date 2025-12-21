using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public ObjectSpawner objectSpawner;
    public Tilemap tilemap;
    public IsometricCharacter playerCharacter;

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

    void Start()
    {
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
                layersToActivate = 3; // Ýlk 3 katman
                break;
            case 2:
                layersToActivate = 5; // Ýlk 5 katman
                break;
            case 3:
                layersToActivate = 7; // Tüm 7 katman
                break;
        }

        // Katmanlarý aktif/pasif et
        for (int i = 0; i < mapLayers.Count; i++)
        {
            if (mapLayers[i] != null)
            {
                mapLayers[i].SetActive(i < layersToActivate);
                Debug.Log($"Layer {i + 1}: {(i < layersToActivate ? "AKTÝF" : "PASÝF")}");
            }
        }

        if (playerCharacter != null && mapLayers.Count > 0)
        {
            Tilemap activeTilemap = mapLayers[0].GetComponent<Tilemap>();
            if (activeTilemap != null)
            {
                playerCharacter.tilemap = activeTilemap;
            }
        }

        Debug.Log($"Aktif katman sayýsý: {layersToActivate}");
    }

    void SpawnEnemies(int count)
    {
        if (enemyPrefab == null || tilemap == null || playerCharacter == null)
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
                Vector3 worldPos = tilemap.GetCellCenterWorld(spawnPos);
                GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

                // Hierarchy organize
                enemyObj.transform.SetParent(GameObject.Find("--- CHARACTERS ---")?.transform);
                enemyObj.name = $"Enemy_{i + 1}_Round{currentRound}";

                IsometricEnemy enemy = enemyObj.GetComponent<IsometricEnemy>();
                if (enemy != null)
                {
                    enemy.tilemap = tilemap;
                    turnManager.enemies.Add(enemy);
                }

                Debug.Log($"Düþman spawn edildi: {spawnPos}");
            }
        }
    }

    Vector3Int GetRandomEnemySpawnPosition()
    {
        BoundsInt bounds = tilemap.cellBounds;
        List<Vector3Int> validPositions = new List<Vector3Int>();

        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        // Tüm tile'larý tara
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (!tilemap.HasTile(pos))
                    continue;

                // Oyuncuya minimum 5 tile uzaklýk
                int distance = Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y);
                if (distance < 5)
                    continue;

                // Bu pozisyon dolu mu?
                if (IsPositionOccupied(pos))
                    continue;

                validPositions.Add(pos);
            }
        }

        if (validPositions.Count > 0)
        {
            return validPositions[Random.Range(0, validPositions.Count)];
        }

        Debug.LogWarning("Düþman için uygun spawn pozisyonu bulunamadý!");
        return Vector3Int.zero;
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

        // Rastgele obje spawn et
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