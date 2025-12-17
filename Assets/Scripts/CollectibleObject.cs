using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [Header("Settings")]
    public int pointValue = 10;
    public int lifetime = 3;

    [Header("VFX")]
    public GameObject collectParticles;

    private Vector3Int gridPosition;

    public void Initialize(Vector3Int position)
    {
        gridPosition = position;
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    public void DecrementLifetime()
    {
        lifetime--;

        if (lifetime <= 0)
        {
            Debug.Log("Obje ömrü bitti, yok oluyor");
            Destroy(gameObject);
        }
    }

    public void Collect(bool givePoints = true)
    {
        if (givePoints && collectParticles != null)
        {
            Instantiate(collectParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public int GetLifetime()
    {
        return lifetime;
    }
}