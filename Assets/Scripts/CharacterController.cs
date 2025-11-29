using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;

    [Header("Visual Feedback")]
    public GameObject movementIndicatorPrefab; // Range göstermek için prefab

    private IsometricCharacter selectedCharacter;
    private Camera mainCamera;
    private List<GameObject> movementIndicators = new List<GameObject>();

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMouseClick();
    }

    void HandleMouseClick()
    {
        // Mouse sol týk kontrolü
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            mouseWorldPos.z = 0;

            // Önce karaktere týklanmýþ mý kontrol et
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                IsometricCharacter character = hit.collider.GetComponent<IsometricCharacter>();

                if (character != null)
                {
                    // Karakteri seç
                    SelectCharacter(character);
                    return;
                }
            }

            // Karakter seçiliyse ve boþ bir tile'a týklandýysa
            if (selectedCharacter != null)
            {
                Vector3Int clickedCell = tilemap.WorldToCell(mouseWorldPos);

                if (tilemap.HasTile(clickedCell))
                {
                    bool moved = selectedCharacter.MoveToGridPosition(clickedCell);

                    if (moved)
                    {
                        Debug.Log("Karakter hareket ediyor: " + clickedCell);
                        DeselectCharacter(); // Hareket ettikten sonra seçimi kaldýr
                    }
                    else
                    {
                        Debug.Log("Oraya hareket edilemiyor!");
                    }
                }
            }
        }
    }

    void SelectCharacter(IsometricCharacter character)
    {
        // Önceki seçimi temizle
        DeselectCharacter();

        selectedCharacter = character;
        Debug.Log("Karakter seçildi: " + character.gameObject.name);

        // Hareket edilebilir tile'larý göster
        ShowMovementRange();
    }

    void DeselectCharacter()
    {
        selectedCharacter = null;
        HideMovementRange();
    }

    void ShowMovementRange()
    {
        if (selectedCharacter == null || movementIndicatorPrefab == null)
            return;

        HashSet<Vector3Int> reachableTiles = selectedCharacter.GetReachableTiles();

        foreach (Vector3Int tile in reachableTiles)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(tile);
            GameObject indicator = Instantiate(movementIndicatorPrefab, worldPos, Quaternion.identity);
            movementIndicators.Add(indicator);
        }
    }

    void HideMovementRange()
    {
        foreach (GameObject indicator in movementIndicators)
        {
            Destroy(indicator);
        }
        movementIndicators.Clear();
    }
}