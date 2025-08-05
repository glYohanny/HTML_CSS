using UnityEngine;

public class RTSGameManager : MonoBehaviour
{
    [Header("Configuración del Sistema RTS")]
    [Tooltip("Tag que deben tener todas las unidades seleccionables")]
    public string unitTag = "Unit";
    
    [Tooltip("Layer para unidades seleccionables")]
    public LayerMask selectableLayer = 1;
    
    [Tooltip("Layer para el terreno")]
    public LayerMask groundLayer = 1;
    
    [Header("Configuración de Selección")]
    [Tooltip("Distancia mínima para considerar un drag válido")]
    public float dragThreshold = 5f;
    
    [Tooltip("Habilitar selección múltiple con drag")]
    public bool enableDragSelection = true;
    
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad base de las unidades")]
    public float baseUnitSpeed = 5f;
    
    [Tooltip("Radio de giro de las unidades")]
    public float unitAngularSpeed = 120f;
    
    [Tooltip("Distancia de parada de las unidades")]
    public float unitStoppingDistance = 0.1f;
    
    [Header("Debug")]
    [Tooltip("Mostrar información de debug")]
    public bool showDebugInfo = true;
    
    // Singleton
    public static RTSGameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Configurar el sistema
        SetupRTSSystem();
        
        if (showDebugInfo)
        {
            Debug.Log("RTS Game Manager inicializado correctamente");
            Debug.Log($"Tag de unidades: {unitTag}");
            Debug.Log($"Layer seleccionable: {selectableLayer.value}");
            Debug.Log($"Layer terreno: {groundLayer.value}");
        }
    }
    
    private void SetupRTSSystem()
    {
        // Verificar que existe un SelectionManager
        if (FindFirstObjectByType<SelectionManager>() == null)
        {
            Debug.LogError("No se encontró SelectionManager en la escena! Agrega uno.");
        }
        
        // Verificar que existe un NavMesh
        if (UnityEngine.AI.NavMesh.CalculateTriangulation().vertices.Length == 0)
        {
            Debug.LogWarning("No se encontró NavMesh en la escena! Las unidades no podrán moverse.");
        }
        
        // Configurar unidades existentes
        SetupExistingUnits();
    }
    
    private void SetupExistingUnits()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag(unitTag);
        
        foreach (GameObject unit in units)
        {
            SetupUnit(unit);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Configuradas {units.Length} unidades existentes");
        }
    }
    
    public void SetupUnit(GameObject unit)
    {
        // Verificar que tenga NavMeshAgent
        var agent = unit.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            agent = unit.AddComponent<UnityEngine.AI.NavMeshAgent>();
            if (showDebugInfo)
            {
                Debug.Log($"Agregado NavMeshAgent a {unit.name}");
            }
        }
        
        // Configurar NavMeshAgent
        agent.speed = baseUnitSpeed;
        agent.angularSpeed = unitAngularSpeed;
        agent.stoppingDistance = unitStoppingDistance;
        
        // Verificar que tenga UnitMovement
        var movement = unit.GetComponent<UnitMovement>();
        if (movement == null)
        {
            movement = unit.AddComponent<UnitMovement>();
            if (showDebugInfo)
            {
                Debug.Log($"Agregado UnitMovement a {unit.name}");
            }
        }
        
        // Verificar que tenga UnitSelectionVisual
        var selectionVisual = unit.GetComponent<UnitSelectionVisual>();
        if (selectionVisual == null)
        {
            selectionVisual = unit.AddComponent<UnitSelectionVisual>();
            if (showDebugInfo)
            {
                Debug.Log($"Agregado UnitSelectionVisual a {unit.name}");
            }
        }
        
        // Verificar que tenga Collider
        var collider = unit.GetComponent<Collider>();
        if (collider == null)
        {
            // Agregar un collider básico
            var boxCollider = unit.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1f, 1f, 1f);
            if (showDebugInfo)
            {
                Debug.Log($"Agregado BoxCollider a {unit.name}");
            }
        }
        
        // Verificar que tenga Renderer
        var renderer = unit.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"La unidad {unit.name} no tiene Renderer. Agrega un modelo 3D.");
        }
    }
    
    // Método para crear una unidad básica
    public GameObject CreateBasicUnit(Vector3 position, string unitName = "Unit")
    {
        GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        unit.name = unitName;
        unit.transform.position = position;
        unit.tag = unitTag;
        
        // Configurar la unidad
        SetupUnit(unit);
        
        if (showDebugInfo)
        {
            Debug.Log($"Creada unidad básica: {unitName} en {position}");
        }
        
        return unit;
    }
    
    // Método para obtener todas las unidades
    public GameObject[] GetAllUnits()
    {
        return GameObject.FindGameObjectsWithTag(unitTag);
    }
    
    // Método para obtener unidades seleccionadas
    public GameObject[] GetSelectedUnits()
    {
        if (SelectionManager.Instance != null)
        {
            return SelectionManager.Instance.GetSelectedUnits().ToArray();
        }
        return new GameObject[0];
    }
    
    // Método para verificar si una unidad está seleccionada
    public bool IsUnitSelected(GameObject unit)
    {
        if (SelectionManager.Instance != null)
        {
            return SelectionManager.Instance.IsUnitSelected(unit);
        }
        return false;
    }
    
    // Método para mover unidades seleccionadas
    public void MoveSelectedUnits(Vector3 destination)
    {
        GameObject[] selectedUnits = GetSelectedUnits();
        
        foreach (GameObject unit in selectedUnits)
        {
            var movement = unit.GetComponent<UnitMovement>();
            if (movement != null)
            {
                movement.MoveTo(destination);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Movidas {selectedUnits.Length} unidades hacia {destination}");
        }
    }
    
    // Método para debug
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("RTS Debug Info", GUI.skin.box);
        
        GameObject[] allUnits = GetAllUnits();
        GameObject[] selectedUnits = GetSelectedUnits();
        
        GUILayout.Label($"Total Units: {allUnits.Length}");
        GUILayout.Label($"Selected Units: {selectedUnits.Length}");
        
        if (SelectionManager.Instance != null)
        {
            GUILayout.Label($"SelectionManager: OK");
        }
        else
        {
            GUILayout.Label($"SelectionManager: MISSING");
        }
        
        GUILayout.EndArea();
    }
} 