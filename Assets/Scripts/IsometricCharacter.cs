using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class IsometricCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    public int moveRange = 2; // Kaç tile hareket edebilir
    public float moveSpeed = 5f; // Hareket hýzý

    [Header("References")]
    public List<Tilemap> allTilemaps = new List<Tilemap>(); // Haritanýn tilemap'i

    private Tilemap mainTilemap;

    public GameObject deathParticles;

    private Vector3Int currentGridPosition;
    private bool isMoving = false;
    private Vector3 targetWorldPosition;
    private bool shouldGiveMovementPoint = true;

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
            mainTilemap = allTilemaps[0];
    }

    //Herhangi bir katmanda Tile var mý?
    public bool HasTileOnAnyLayer(Vector3Int pos)
    {
        foreach (var tm in allTilemaps)
        {
            if (tm != null && tm.gameObject.activeSelf && tm.HasTile(pos))
                return true;
        }
        return false;
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

                if (shouldGiveMovementPoint)
                {
                    ScoreManager.Instance?.OnPlayerMoved();
                }
            }
        }
    }


    // Karakteri belirtilen grid pozisyonuna hareket ettir
    public bool MoveToGridPosition(Vector3Int targetGridPosition, bool isAttackMove = false)
    {
        // Zaten hareket ediyorsa yeni hareket baþlatma
        if (isMoving)
            return false;

        targetGridPosition.z = 0;

        // Hedef pozisyonda tile var mý kontrol et
        if (!HasTileOnAnyLayer(targetGridPosition))
            return false;

        // Hareket edilebilir tile'lardan biri mi kontrol et
        HashSet<Vector3Int> reachableTiles = GetReachableTiles();

        if (!reachableTiles.Contains(targetGridPosition))
            return false; // Eriþilebilir deðil

        // Hareketi baþlat
        currentGridPosition = targetGridPosition;
        targetWorldPosition = mainTilemap.GetCellCenterWorld(targetGridPosition);
        isMoving = true;
        shouldGiveMovementPoint = !isAttackMove;

        // --- SES EKLE ---
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.playerMoveSound);
        }

        shouldGiveMovementPoint = !isAttackMove;

        return true;
    }

    // Karakterin eriþebileceði tüm tile'larý hesapla
    public HashSet<Vector3Int> GetReachableTiles()
    {
        HashSet<Vector3Int> reachable = new HashSet<Vector3Int>();
        Queue<Vector3Int> toExplore = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        UpdateMainTilemapRef();


        if (!HasTileOnAnyLayer(currentGridPosition))
        {
            Debug.LogWarning("Karakter boþlukta!");
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

                bool hasTile = HasTileOnAnyLayer(neighbor);

                if (!hasTile)
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

    // Karakteri mevcut tile'ýn ortasýna hizala
    void SnapToTileCenter()
    {
        if(mainTilemap != null)
        transform.position = mainTilemap.GetCellCenterWorld(currentGridPosition);
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
        if(deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

}