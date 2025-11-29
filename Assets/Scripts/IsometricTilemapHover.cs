using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class IsometricTilemapHover : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap tilemap;

    [Header("Hover Settings")]
    public Color hoverColor = new Color(1f, 1f, 1f, 0.8f);
    public GameObject hoverIndicatorPrefab;

    private Vector3Int currentHoveredCell;
    private Vector3Int lastHoveredCell;
    private GameObject hoverIndicator;
    private Color originalColor;
    private Camera mainCamera;

    void Start()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }

        mainCamera = Camera.main;
        originalColor = tilemap.color;

        if (hoverIndicatorPrefab != null)
        {
            hoverIndicator = Instantiate(hoverIndicatorPrefab);
            hoverIndicator.SetActive(false);
        }

        lastHoveredCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    }

    void Update()
    {
        HandleMouseHover();
    }

    void HandleMouseHover()
    {
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
        mouseWorldPos.z = 0;

        
        currentHoveredCell = tilemap.WorldToCell(mouseWorldPos);

        
        bool hasTile = tilemap.HasTile(currentHoveredCell);

        if (hasTile && currentHoveredCell != lastHoveredCell)
        {
            
            if (tilemap.HasTile(lastHoveredCell))
            {
                OnTileHoverExit(lastHoveredCell);
            }

            
            OnTileHoverEnter(currentHoveredCell);
            lastHoveredCell = currentHoveredCell;
        }
        else if (!hasTile && lastHoveredCell != new Vector3Int(int.MinValue, int.MinValue, 0))
        {
            
            OnTileHoverExit(lastHoveredCell);
            lastHoveredCell = new Vector3Int(int.MinValue, int.MinValue, 0);
        }
    }

    void OnTileHoverEnter(Vector3Int cellPosition)
    {
        
        tilemap.SetTileFlags(cellPosition, TileFlags.None);
        tilemap.SetColor(cellPosition, hoverColor);

        
        if (hoverIndicator != null)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(cellPosition);
            hoverIndicator.transform.position = worldPos;
            hoverIndicator.SetActive(true);
        }
    }

    void OnTileHoverExit(Vector3Int cellPosition)
    {
        
        tilemap.SetColor(cellPosition, Color.white);
        tilemap.SetTileFlags(cellPosition, TileFlags.LockColor);

        
        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(false);
        }
    }

   
    public Vector3Int GetCellAtMousePosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
        return tilemap.WorldToCell(mouseWorldPos);
    }

    
    public bool HasTileAt(Vector3Int cellPosition)
    {
        return tilemap.HasTile(cellPosition);
    }
}