using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
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
        if (!turnManager.IsPlayerTurn())
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
                Vector3Int clickedCell = tilemap.WorldToCell(mouseWorldPos);
                
                if (tilemap.HasTile(clickedCell))
                {
                    bool moved = selectedCharacter.MoveToGridPosition(clickedCell);
                    
                    if (moved)
                    {
                        Debug.Log("Karakter hareket ediyor: " + clickedCell);

                        StartCoroutine(WaitForMoveComplete(selectedCharacter));
                        DeselectCharacter();                        
                        
                    }
                    else
                    {
                        Debug.Log("Oraya hareket edilemiyor!");
                    }
                }
            }
        }
    }
    
    System.Collections.IEnumerator WaitForMoveComplete(IsometricCharacter movingCharacter)
    {
        // Karakterin hareketi bitene kadar bekle
        yield return new WaitUntil(() => !movingCharacter.IsMoving());

        DeselectCharacter();
        
        // Sýrayý deðiþtir
        turnManager.OnPlayerMoveComplete();
    }
    
    void SelectCharacter(IsometricCharacter character)
    {
        DeselectCharacter();
        
        selectedCharacter = character;
        Debug.Log("Karakter seçildi: " + character.gameObject.name);
        
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