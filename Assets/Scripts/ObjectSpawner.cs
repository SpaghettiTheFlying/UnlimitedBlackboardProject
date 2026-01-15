using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

public class ObjectSpawner : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public IsometricCharacter playerCharacter;
    public TurnManager turnManager;
    public GameObject collectiblePrefab;

    [Header("Spawn Settings")]
    public int minDistanceFromPlayer = 2;
    public int maxCollectiblesOnMap = 5;

    private List<CollectibleObject> activeCollectibles = new List<CollectibleObject>();


    public void SpawnCollectible()
    {
        RemoveNullCollectibles();
        if (activeCollectibles.Count >= maxCollectiblesOnMap) return;

        Vector3Int spawnPosition = GetRandomValidPosition();

        if (spawnPosition != Vector3Int.zero)
        {
            Vector3 worldPos = GetWorldPositionForCollectible(spawnPosition);
            GameObject obj = Instantiate(collectiblePrefab, worldPos, Quaternion.identity);
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
    Vector3 GetWorldPositionForCollectible(Vector3Int pos)
    {
        if (RoundManager.Instance == null) return tilemap.GetCellCenterWorld(pos);

        for (int i = 0; i < RoundManager.Instance.activeTilemaps.Count; i++)
        {
            Tilemap tm = RoundManager.Instance.activeTilemaps[i];

            for (int z = 2; z >= -2; z--)
            {
                Vector3Int checkPos = new Vector3Int(pos.x, pos.y, z);
                if (tm.HasTile(checkPos))
                {
                    Vector3 worldPos = tm.GetCellCenterWorld(checkPos);

                    worldPos.z -= 0.5f;

                    return worldPos;
                }
            }
        }
        return tilemap.GetCellCenterWorld(pos);
    }

    Vector3Int GetRandomValidPosition()
    {
        if (RoundManager.Instance == null) return Vector3Int.zero;

        // DÜZELTME: Layer 1 boþ olsa bile tüm haritayý tara
        BoundsInt bounds = RoundManager.Instance.GetTotalBounds();

        List<Vector3Int> validPositions = new List<Vector3Int>();
        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Zemin kontrolü
                if (!HasGroundAt(pos)) continue;

                // Mesafe kontrolü
                if (Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y) < minDistanceFromPlayer) continue;

                // Doluluk kontrolü
                if (IsPositionOccupied(pos)) continue;

                validPositions.Add(pos);
            }
        }

        if (validPositions.Count > 0)
        {
            return validPositions[Random.Range(0, validPositions.Count)];
        }

        return Vector3Int.zero;
    }

    bool HasGroundAt(Vector3Int pos)
    {
        foreach (var tm in RoundManager.Instance.activeTilemaps)
        {
            for (int z = -2; z <= 2; z++)
            {
                if (tm.HasTile(new Vector3Int(pos.x, pos.y, z))) return true;
            }
        }
        return false;
    }
    bool IsPositionOccupied(Vector3Int pos)
    {
        if (playerCharacter.GetCurrentGridPosition().x == pos.x && playerCharacter.GetCurrentGridPosition().y == pos.y) return true;

        foreach (var enemy in turnManager.enemies)
        {
            if (enemy != null && enemy.GetCurrentGridPosition().x == pos.x && enemy.GetCurrentGridPosition().y == pos.y) return true;
        }

        foreach (var col in activeCollectibles)
        {
            if (col != null && col.GetGridPosition().x == pos.x && col.GetGridPosition().y == pos.y) return true;
        }
        return false;
    }

    public void OnPlayerMoved()
    {
        RemoveNullCollectibles();
        
        for (int i = activeCollectibles.Count - 1; i >= 0; i--) 
        { 
            activeCollectibles[i].DecrementLifetime();

            if (activeCollectibles[i] == null)
            {
                activeCollectibles.RemoveAt(i);
            }
        }
    }

    public void CheckCollectiblePickup(Vector3Int playerGridPos, bool isPlayer)
    {
        RemoveNullCollectibles();
        for (int i = activeCollectibles.Count - 1; i >= 0; i--)
        {
            Vector3Int colPos = activeCollectibles[i].GetGridPosition();

            if (colPos.x == playerGridPos.x && colPos.y == playerGridPos.y)
            {
                if (isPlayer)
                {
                    Debug.Log("Obje Toplandý!");
                    ScoreManager.Instance.OnCollectibleGathered();
                    activeCollectibles[i].Collect(true);
                }
                else
                {
                    Debug.Log("Düþman objeyi ezdi!");
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