using UnityEngine;

public class HierarchyOrganizer : MonoBehaviour
{
    [Header("Container Parents")]
    public Transform managersContainer;
    public Transform charactersContainer;
    public Transform environmentContainer;
    public Transform uiContainer;
    public Transform vfxContainer;

    void Awake()
    {
        OrganizeHierarchy();
    }

    [ContextMenu("Organize Hierarchy")]
    public void OrganizeHierarchy()
    {
        // Containerlarý oluþtur
        CreateContainers();

        // Mevcut objeleri organize et
        OrganizeExistingObjects();
    }

    void CreateContainers()
    {
        if (managersContainer == null)
        {
            GameObject managers = GameObject.Find("--- MANAGERS ---");
            if (managers == null)
            {
                managers = new GameObject("--- MANAGERS ---");
            }
            managersContainer = managers.transform;
        }

        if (charactersContainer == null)
        {
            GameObject characters = GameObject.Find("--- CHARACTERS ---");
            if (characters == null)
            {
                characters = new GameObject("--- CHARACTERS ---");
            }
            charactersContainer = characters.transform;
        }

        if (environmentContainer == null)
        {
            GameObject environment = GameObject.Find("--- ENVIRONMENT ---");
            if (environment == null)
            {
                environment = new GameObject("--- ENVIRONMENT ---");
            }
            environmentContainer = environment.transform;
        }

        if (uiContainer == null)
        {
            GameObject ui = GameObject.Find("--- UI ---");
            if (ui == null)
            {
                ui = new GameObject("--- UI ---");
            }
            uiContainer = ui.transform;
        }

        if (vfxContainer == null)
        {
            GameObject vfx = GameObject.Find("--- VFX ---");
            if (vfx == null)
            {
                vfx = new GameObject("--- VFX ---");
            }
            vfxContainer = vfx.transform;
        }
    }

    void OrganizeExistingObjects()
    {
        // Managers
        MoveToContainer("GameController", managersContainer);
        MoveToContainer("TurnManager", managersContainer);
        MoveToContainer("ScoreManager", managersContainer);
        MoveToContainer("ObjectSpawner", managersContainer);
        MoveToContainer("RoundManager", managersContainer);

        // Characters
        MoveToContainer("PlayerCharacter", charactersContainer);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.transform.parent == null)
                enemy.transform.SetParent(charactersContainer);
        }

        // Environment
        MoveToContainer("Grid", environmentContainer);
        MoveToContainer("Tilemap", environmentContainer);

        // UI
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null && canvas.transform.parent == null)
        {
            canvas.transform.SetParent(uiContainer);
        }

        // Sýralama
        managersContainer.SetSiblingIndex(0);
        charactersContainer.SetSiblingIndex(1);
        environmentContainer.SetSiblingIndex(2);
        uiContainer.SetSiblingIndex(3);
        vfxContainer.SetSiblingIndex(4);
    }

    void MoveToContainer(string objectName, Transform container)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null && obj.transform.parent == null && container != null)
        {
            obj.transform.SetParent(container);
        }
    }
}