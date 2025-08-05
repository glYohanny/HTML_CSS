using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    [Tooltip("Prefab de la unidad a spawnear (opcional)")]
    public GameObject unitPrefab;
    [Tooltip("Número de unidades a crear")]
    public int unitCount = 5;
    [Tooltip("Radio del área de spawn")]
    public float spawnRadius = 10f;
    [Tooltip("Altura de spawn")]
    public float spawnHeight = 0f;

    [Header("Debug")]
    [Tooltip("Mostrar información de debug")]
    public bool showDebugInfo = true;

    // Nuevo Input System
    private InputAction spawnAction;
    private InputAction clearAction;
    private InputAction formationAction;

    private void Awake()
    {
        spawnAction = new InputAction(binding: "<Keyboard>/space");
        clearAction = new InputAction(binding: "<Keyboard>/c");
        formationAction = new InputAction(binding: "<Keyboard>/f");
        spawnAction.Enable();
        clearAction.Enable();
        formationAction.Enable();
    }

    private void OnDestroy()
    {
        spawnAction.Disable();
        clearAction.Disable();
        formationAction.Disable();
    }

    private void Update()
    {
        if (spawnAction.WasPressedThisFrame())
        {
            SpawnUnits();
        }
        if (clearAction.WasPressedThisFrame())
        {
            ClearAllUnits();
        }
        if (formationAction.WasPressedThisFrame())
        {
            SpawnUnitsInFormation(transform.position, 3, 3, 2f);
        }
    }

    public void SpawnUnits()
    {
        if (RTSGameManager.Instance == null)
        {
            Debug.LogError("RTSGameManager no encontrado!");
            return;
        }
        for (int i = 0; i < unitCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);
            GameObject unit = SpawnUnitAt(spawnPosition, $"Unit_{i + 1}");
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f)
                );
            }
        }
        if (showDebugInfo)
        {
            Debug.Log($"Spawned {unitCount} units around {transform.position}");
        }
    }

    public void ClearAllUnits()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        foreach (GameObject unit in units)
        {
            DestroyImmediate(unit);
        }
        if (showDebugInfo)
        {
            Debug.Log($"Cleared {units.Length} units");
        }
    }

    public GameObject SpawnUnitAt(Vector3 position, string unitName = "Unit")
    {
        GameObject unit = null;
        if (unitPrefab != null)
        {
            unit = Instantiate(unitPrefab, position, unitPrefab.transform.rotation);
            unit.name = unitName;
            unit.tag = "Unit";
        }
        else if (RTSGameManager.Instance != null)
        {
            unit = RTSGameManager.Instance.CreateBasicUnit(position, unitName);
        }
        else
        {
            Debug.LogError("No se pudo crear la unidad: no hay prefab ni RTSGameManager disponible.");
            return null;
        }
        if (showDebugInfo)
        {
            Debug.Log($"Spawned unit {unitName} at {position}");
        }
        return unit;
    }

    public void SpawnUnitsInFormation(Vector3 centerPosition, int rows, int columns, float spacing = 2f)
    {
        if (RTSGameManager.Instance == null && unitPrefab == null)
        {
            Debug.LogError("RTSGameManager o unitPrefab no encontrado!");
            return;
        }
        int unitIndex = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = centerPosition + new Vector3(
                    (col - columns / 2f) * spacing,
                    spawnHeight,
                    (row - rows / 2f) * spacing
                );
                GameObject unit = SpawnUnitAt(position, $"FormationUnit_{unitIndex + 1}");
                var renderer = unit.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.blue;
                }
                unitIndex++;
            }
        }
        if (showDebugInfo)
        {
            Debug.Log($"Spawned {unitIndex} units in formation at {centerPosition}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = Color.green;
        Vector3 heightPosition = transform.position + Vector3.up * spawnHeight;
        Gizmos.DrawWireSphere(heightPosition, 0.5f);
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 120));
        GUILayout.Label("Unit Spawner", GUI.skin.box);
        GUILayout.Label("Presiona [Space] para spawnear unidades");
        GUILayout.Label("Presiona [C] para limpiar todas las unidades");
        GUILayout.Label("Presiona [F] para spawnear formación 3x3");
        GUILayout.EndArea();
    }
} 