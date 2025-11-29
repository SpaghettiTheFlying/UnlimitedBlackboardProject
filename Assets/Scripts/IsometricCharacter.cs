using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class IsometricCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    public int moveRange = 2; // Kaç tile hareket edebilir
    public float moveSpeed = 5f; // Hareket hýzý

    [Header("References")]
    public Tilemap tilemap; // Haritanýn tilemap'i

    private Vector3Int currentGridPosition;
    private bool isMoving = false;
    private Vector3 targetWorldPosition;

    void Start()
    {
        if (tilemap != null)
        {
            currentGridPosition = tilemap.WorldToCell(transform.position);
            Debug.Log("Baþlangýç world pozisyonu: " + transform.position);
            Debug.Log("Baþlangýç grid pozisyonu: " + currentGridPosition);

            SnapToTileCenter();
        }
    }

    void Update()
    {
        // Hareket ediyorsa hedefe doðru ilerle
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);

            // Hedefe ulaþtý mý?
            if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
            {
                transform.position = targetWorldPosition;
                isMoving = false;
            }
        }
    }

    // Karakteri belirtilen grid pozisyonuna hareket ettir
    public bool MoveToGridPosition(Vector3Int targetGridPosition)
    {
        // Zaten hareket ediyorsa yeni hareket baþlatma
        if (isMoving)
            return false;

        // Z deðerini 0 yap
        targetGridPosition.z = 0;

        // Hedef pozisyonda tile var mý kontrol et
        if (!tilemap.HasTile(targetGridPosition))
            return false;

        // Hareket edilebilir tile'lardan biri mi kontrol et
        HashSet<Vector3Int> reachableTiles = GetReachableTiles();

        if (!reachableTiles.Contains(targetGridPosition))
            return false; // Eriþilebilir deðil

        // Hareketi baþlat
        currentGridPosition = targetGridPosition;
        targetWorldPosition = tilemap.GetCellCenterWorld(targetGridPosition);
        isMoving = true;

        return true;
    }

    public HashSet<Vector3Int> GetReachableTiles()
    {
        HashSet<Vector3Int> reachable = new HashSet<Vector3Int>();
        Queue<Vector3Int> toExplore = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        Debug.Log("Karakter baþlangýç pozisyonu: " + currentGridPosition);

        if (!tilemap.HasTile(currentGridPosition))
        {
            Debug.LogWarning("Karakter tile üzerinde deðil!");
            BoundsInt bounds = tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int testPos = new Vector3Int(x, y, 0);
                    if (tilemap.HasTile(testPos))
                    {
                        currentGridPosition = testPos;
                        break;
                    }
                }
            }
        }

        // Baþlangýç noktasý
        toExplore.Enqueue(currentGridPosition);
        distances[currentGridPosition] = 0;

        while (toExplore.Count > 0)
        {
            Vector3Int current = toExplore.Dequeue();
            int currentDistance = distances[current];

            if (currentDistance >= moveRange)
                continue;

            // Ýzometrik Z as Y için 4 yönlü komþular
            // Her tile'ýn 4 komþusu var (diamond pattern)
            Vector3Int[] neighbors = new Vector3Int[]
            {
                new Vector3Int(current.x - 1, current.y - 2, 0),  // Sol-alt
                new Vector3Int(current.x + 1, current.y + 2, 0),  // Sað-üst  
                new Vector3Int(current.x - 1, current.y + 1, 0),  // Sol-üst
                new Vector3Int(current.x + 1, current.y - 1, 0),  // Sað-alt
            };

            foreach (Vector3Int neighbor in neighbors)
            {
                if (distances.ContainsKey(neighbor))
                    continue;

                bool hasTile = tilemap.HasTile(neighbor);

                if (!hasTile)
                    continue;

                int newDistance = currentDistance + 1;
                distances[neighbor] = newDistance;
                reachable.Add(neighbor);

                Debug.Log($"Eriþilebilir tile: {neighbor}, mesafe: {newDistance}");

                if (newDistance < moveRange)
                {
                    toExplore.Enqueue(neighbor);
                }
            }
        }

        Debug.Log("Toplam eriþilebilir tile sayýsý: " + reachable.Count);
        return reachable;
    }

    // Karakteri mevcut tile'ýn ortasýna hizala
    void SnapToTileCenter()
    {
        transform.position = tilemap.GetCellCenterWorld(currentGridPosition);
    }

    public Vector3Int GetCurrentGridPosition()
    {
        return currentGridPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}