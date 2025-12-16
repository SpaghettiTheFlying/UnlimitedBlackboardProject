using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [Header("Characters")]
    public IsometricCharacter playerCharacter;
    public List<IsometricEnemy> enemies = new List<IsometricEnemy>();

    [Header("Turn Settings")]
    public bool isPlayerTurn = true;

    private int currentEnemyIndex = 0;
    private bool isProcessingTurn = false;

    void Start()
    {
        Debug.Log("Oyun baþladý - Oyuncu sýrasý");
    }

    void Update()
    {
        // Eðer düþman sýrasýysa ve iþlem yapýlmýyorsa
        if (!isPlayerTurn && !isProcessingTurn)
        {
            StartCoroutine(ProcessEnemyTurns());
        }
    }

    public void OnPlayerMoveComplete()
    {
        if (isPlayerTurn && !isProcessingTurn)
        {
            // Oyuncu bir düþmanýn üzerine geldi mi kontrol et
            CheckPlayerAttack();

            Debug.Log("Oyuncu hareketi tamamlandý - Düþman sýrasý");
            isPlayerTurn = false;
            currentEnemyIndex = 0;
        }
    }

    void CheckPlayerAttack()
    {
        if (playerCharacter == null) return;

        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        // Tüm düþmanlarý kontrol et
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null) continue;

            Vector3Int enemyPos = enemies[i].GetCurrentGridPosition();

            if (playerPos == enemyPos)
            {
                Debug.Log("Oyuncu düþmaný yendi!");
                enemies[i].Die();
                enemies.RemoveAt(i);

                // Tüm düþmanlar yenildiyse oyun bitti
                if (enemies.Count == 0)
                {
                    Debug.Log("OYUNCU KAZANDI!");
                }
            }
        }
    }

    IEnumerator ProcessEnemyTurns()
    {
        isProcessingTurn = true;

        if (enemies.Count == 0)
        {
            EndEnemyTurn();
            yield break;
        }

        // Her düþman sýrayla hareket eder
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;

            Debug.Log($"Düþman {i + 1} sýrasý");

            // Düþmaný hareket ettir
            MoveEnemy(enemies[i]);

            // Düþman hareket etmeyi bitirene kadar bekle
            yield return new WaitUntil(() => !enemies[i].IsMoving());

            // Düþman oyuncuyu yendi mi kontrol et
            CheckEnemyAttack(enemies[i]);

            // Bir sonraki düþman için kýsa bekleme
            yield return new WaitForSeconds(0.5f);
        }

        EndEnemyTurn();
    }

    void MoveEnemy(IsometricEnemy enemy)
    {
        if (enemy == null || playerCharacter == null) return;

        // Basit AI: Oyuncuya doðru git veya rastgele hareket et
        HashSet<Vector3Int> reachableTiles = enemy.GetReachableTiles();
        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        if (reachableTiles.Count > 0)
        {
            // Eðer oyuncu eriþilebilir mesafedeyse ona git
            if (reachableTiles.Contains(playerPos))
            {
                enemy.MoveToGridPosition(playerPos);
                Debug.Log("Düþman oyuncuya saldýrýyor!");
            }
            else
            {
                // Rastgele hareket et
                List<Vector3Int> tileList = new List<Vector3Int>(reachableTiles);
                Vector3Int randomTile = tileList[Random.Range(0, tileList.Count)];
                enemy.MoveToGridPosition(randomTile);
                Debug.Log($"Düþman {randomTile} pozisyonuna hareket etti");
            }
        }
        else
        {
            Debug.Log("Düþman hareket edemiyor");
        }
    }

    void CheckEnemyAttack(IsometricEnemy enemy)
    {
        if (enemy == null || playerCharacter == null) return;

        Vector3Int enemyPos = enemy.GetCurrentGridPosition();
        Vector3Int playerPos = playerCharacter.GetCurrentGridPosition();

        if (enemyPos == playerPos)
        {
            Debug.Log("Düþman oyuncuyu yendi!");
            playerCharacter.Die();
            playerCharacter = null;

            Debug.Log("OYUN BÝTTÝ - DÜÞMAN KAZANDI!");
            // Oyunu durdur veya game over ekraný göster
            Time.timeScale = 0;
        }
    }

    void EndEnemyTurn()
    {
        Debug.Log("Düþman turu bitti - Oyuncu sýrasý");
        isPlayerTurn = true;
        isProcessingTurn = false;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    // Düþman listesinden ölen düþmanlarý temizle
    public void RemoveDeadEnemies()
    {
        enemies.RemoveAll(enemy => enemy == null);
    }
}