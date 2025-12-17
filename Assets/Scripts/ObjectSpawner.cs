using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public IsometricCharacter playerCharacter;
    public TurnManager turnManager; // Düþmanlara eriþmek için
    public GameObject collectiblePrefab;

    [Header("Spawn Settings")]
    public int minDistanceFromPlayer = 3;
    public int maxCollectiblesOnMap = 5;

    private List<CollectibleObject> activeCollectibles = new List<CollectibleObject>();

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnCollectible();
        }
    }

    public void SpawnCollectible()
    {
        if (activeCollectibles.Count >= maxCollectiblesOnMap)
            return;

        Vector3Int spawnPosition = GetRandomValidPosition();

        if (spawnPosition != Vector3Int.zero)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(spawnPosition);
            GameObject obj = Instantiate(collectiblePrefab, worldPos, Quaternion.identity);

            // Hierarchy'yi organize et
            obj.transform.SetParent(transform);

            CollectibleObject collectible = obj.GetComponent<CollectibleObject>();
            if (collectible != null)
            {
                collectible.Initialize(spawnPosition);
                activeCollectibles.Add(collectible);
                Debug.Log($"Obje spawn edildi: {spawnPosition}");
            }
        }
    }

    Vector3Int GetRandomValidPosition()
    {
        BoundsInt bounds = tilemap.cellBounds;
        List<Vector3Int> validPositions = new List<Vector3Int>();

        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (!tilemap.HasTile(pos))
                    continue;

                // Oyuncuya mesafe kontrolü
                int distance = Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y);
                if (distance < minDistanceFromPlayer)
                    continue;

                // Bu pozisyon dolu mu kontrol et (oyuncu, düþman veya obje)
                if (IsPositionOccupied(pos))
                    continue;

                validPositions.Add(pos);
            }
        }

        if (validPositions.Count > 0)
        {
            return validPositions[Random.Range(0, validPositions.Count)];
        }

        return Vector3Int.zero;
    }

    bool IsPositionOccupied(Vector3Int position)
    {
        // Oyuncu bu pozisyonda mý?
        if (playerCharacter.GetCurrentGridPosition() == position)
            return true;

        // Düþmanlar bu pozisyonda mý?
        if (turnManager != null && turnManager.enemies != null)
        {
            foreach (IsometricEnemy enemy in turnManager.enemies)
            {
                if (enemy != null && enemy.GetCurrentGridPosition() == position)
                    return true;
            }
        }

        // Bir obje bu pozisyonda mý?
        foreach (CollectibleObject collectible in activeCollectibles)
        {
            if (collectible != null && collectible.GetGridPosition() == position)
                return true;
        }

        return false;
    }

    public void OnPlayerMoved()
    {
        for (int i = activeCollectibles.Count - 1; i >= 0; i--)
        {
            if (activeCollectibles[i] == null)
            {
                activeCollectibles.RemoveAt(i);
                continue;
            }

            activeCollectibles[i].DecrementLifetime();

            if (activeCollectibles[i] == null)
            {
                activeCollectibles.RemoveAt(i);
            }
        }

        if (Random.value < 0.5f)
        {
            SpawnCollectible();
        }
    }

    public void CheckCollectiblePickup(Vector3Int position, bool isPlayer)
    {
        for (int i = activeCollectibles.Count - 1; i >= 0; i--)
        {
            if (activeCollectibles[i] == null)
            {
                activeCollectibles.RemoveAt(i);
                continue;
            }

            if (activeCollectibles[i].GetGridPosition() == position)
            {
                if (isPlayer)
                {
                    Debug.Log("Oyuncu obje topladý!");
                    ScoreManager.Instance.OnCollectibleGathered();
                    activeCollectibles[i].Collect(true);
                }
                else
                {
                    Debug.Log("Düþman objeyi yok etti");
                    activeCollectibles[i].Collect(false);
                }

                activeCollectibles.RemoveAt(i);
                break;
            }
        }
    }

    public void RemoveNullCollectibles()
    {
        activeCollectibles.RemoveAll(c => c == null);
    }
}