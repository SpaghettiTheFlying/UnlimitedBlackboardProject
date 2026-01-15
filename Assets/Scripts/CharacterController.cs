using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    [Header("References")]
    public Tilemap mainTilemap;
    public TurnManager turnManager;
    
    [Header("Visual Feedback")]
    public GameObject movementIndicatorPrefab;
    
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
        if (turnManager == null || !turnManager.IsPlayerTurn())
            return;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            mouseWorldPos.z = 0;
            
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
            
            if (hit.collider != null)
            {
                IsometricCharacter character = hit.collider.GetComponent<IsometricCharacter>();
                
                if (character != null)
                {
                    SelectCharacter(character);
                    return;
                }
            }
            
            if (selectedCharacter != null)
            {
                Vector3Int clickedCell = mainTilemap.WorldToCell(mouseWorldPos);
                clickedCell.z = 0;

                if (selectedCharacter.HasTileOnAnyLayer(clickedCell))
                {
                    bool isEnemyAtPosition = IsEnemyAtPosition(clickedCell);

                    bool moved = selectedCharacter.MoveToGridPosition(clickedCell, isEnemyAtPosition);

                    if (moved)
                    {
                        if (isEnemyAtPosition)
                        {
                            Debug.Log("Karakter düþmana saldýrýyor: " + clickedCell);
                        }
                        else
                        {
                            Debug.Log("Karakter hareket ediyor: " + clickedCell);
                        }

                        StartCoroutine(WaitForMoveComplete(selectedCharacter));
                        DeselectCharacter();
                    }
                    else
                    {
                        Debug.Log("Mesafe çok uzak veya gidilemez!");
                    }
                }
                else
                {
                    Debug.Log("Týklanan yerde (aktif katmanlarda) zemin yok.");
                }



            }
        }
    }
    bool IsEnemyAtPosition(Vector3Int position)
    {
        if (turnManager == null || turnManager.enemies == null)
            return false;

        foreach (IsometricEnemy enemy in turnManager.enemies)
        {
            if (enemy != null && enemy.GetCurrentGridPosition() == position)
            {
                return true;
            }
        }

        return false;
    }

    System.Collections.IEnumerator WaitForMoveComplete(IsometricCharacter movingCharacter)
    {
        // Karakterin hareketi bitene kadar bekle
        yield return new WaitUntil(() => !movingCharacter.IsMoving());

        DeselectCharacter();

        // Sýrayý deðiþtir
        if (turnManager != null)
            turnManager.OnPlayerMoveComplete();
    }
    
    void SelectCharacter(IsometricCharacter character)
    {
        DeselectCharacter();
        
        selectedCharacter = character;
        Debug.Log("Karakter seçildi: " + character.gameObject.name);
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.characterSelectSound);
        }

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
            Vector3 worldPos = mainTilemap.GetCellCenterWorld(tile);

            worldPos.z = -1f;

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