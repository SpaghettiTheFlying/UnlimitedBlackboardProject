using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class IsometricEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public int moveRange = 1;
    public float moveSpeed = 3f;

    [Header("References")]
    public List<Tilemap> allTilemaps = new List<Tilemap>();

    private Tilemap mainTilemap;

    [Header("VFX")]
    public GameObject deathParticles;

    private Vector3Int currentGridPosition;
    private bool isMoving = false;
    private Vector3 targetWorldPosition;

    void Start()
    {
        UpdateMainTilemapRef();

        if (mainTilemap != null)
        {
            currentGridPosition = mainTilemap.WorldToCell(transform.position);
            SnapToTileCenter();
        }
    }
    public void UpdateMainTilemapRef()
    {
        if (allTilemaps.Count > 0 && allTilemaps[0] != null)
        {
            mainTilemap = allTilemaps[0];
        }
    }
    public bool HasTileOnAnyLayer(Vector3Int pos)
    {
        foreach (var tm in allTilemaps)
        {
            // Tilemap aktifse ve o pozisyonda tile varsa
            if (tm != null && tm.gameObject.activeSelf && tm.HasTile(pos))
                return true;
        }
        return false;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
            {
                transform.position = targetWorldPosition;
                isMoving = false;
            }
        }
    }

    public bool MoveToGridPosition(Vector3Int targetGridPosition)
    {
        if (isMoving)
            return false;

        targetGridPosition.z = 0;

        if (!HasTileOnAnyLayer(targetGridPosition))
            return false;

        HashSet<Vector3Int> reachableTiles = GetReachableTiles();

        if (!reachableTiles.Contains(targetGridPosition))
            return false;

        currentGridPosition = targetGridPosition;

        if (mainTilemap != null)
        {
            targetWorldPosition = mainTilemap.GetCellCenterWorld(targetGridPosition);
            isMoving = true;
            return true;
        }

        return true;
    }

    public HashSet<Vector3Int> GetReachableTiles()
    {
        HashSet<Vector3Int> reachable = new HashSet<Vector3Int>();
        Queue<Vector3Int> toExplore = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        UpdateMainTilemapRef();

        if (mainTilemap == null) return reachable;

        if (!HasTileOnAnyLayer(currentGridPosition))
        {
            // Fallback: Bulunduðu yer boþsa olduðu yerde kalsýn
            reachable.Add(currentGridPosition);
            return reachable;
        }

        toExplore.Enqueue(currentGridPosition);
        distances[currentGridPosition] = 0;

        while (toExplore.Count > 0)
        {
            Vector3Int current = toExplore.Dequeue();
            int currentDistance = distances[current];

            if (currentDistance >= moveRange)
                continue;

            Vector3Int[] neighbors = new Vector3Int[]
            {
                new Vector3Int(current.x - 2, current.y - 1, 0),
                new Vector3Int(current.x - 1, current.y - 2, 0),
                new Vector3Int(current.x + 1, current.y + 2, 0),
                new Vector3Int(current.x + 2, current.y + 1, 0),
            };

            foreach (Vector3Int neighbor in neighbors)
            {
                if (distances.ContainsKey(neighbor))
                    continue;

                if (!HasTileOnAnyLayer(neighbor))
                    continue;

                int newDistance = currentDistance + 1;
                distances[neighbor] = newDistance;
                reachable.Add(neighbor);

                if (newDistance < moveRange)
                {
                    toExplore.Enqueue(neighbor);
                }
            }
        }

        return reachable;
    }

    void SnapToTileCenter()
    {
        if (mainTilemap != null)
        {
            transform.position = mainTilemap.GetCellCenterWorld(currentGridPosition);
        }
    }

    public Vector3Int GetCurrentGridPosition()
    {
        return currentGridPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public void Die()
    {
        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}