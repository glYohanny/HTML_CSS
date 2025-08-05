using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstructionUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject constructionPanel;
    public Transform buildingButtonsContainer;
    public GameObject buildingButtonPrefab;
    
    [Header("Building Prefabs")]
    public GameObject[] buildingPrefabs;
    public string[] buildingNames;
    public int[] buildingCosts;
    
    [Header("Construction Settings")]
    public LayerMask groundLayer = 1;
    public float previewHeight = 0.1f;
    
    private Camera cam;
    private GameObject currentPreview;
    private GameObject selectedBuildingPrefab;
    private bool isPlacingBuilding = false;
    private Recolector selectedRecolector;
    
    void Start()
    {
        cam = Camera.main;
        if (constructionPanel != null)
            constructionPanel.SetActive(false);
    }
    
    void Update()
    {
        if (isPlacingBuilding && selectedBuildingPrefab != null)
        {
            HandleBuildingPlacement();
        }
    }
    
    /// <summary>
    /// Abre la interfaz de construcción para un recolector específico
    /// </summary>
    public void OpenConstructionUI(Recolector recolector)
    {
        selectedRecolector = recolector;
        if (constructionPanel != null)
        {
            constructionPanel.SetActive(true);
            PopulateBuildingButtons();
        }
    }
    
    /// <summary>
    /// Cierra la interfaz de construcción
    /// </summary>
    public void CloseConstructionUI()
    {
        if (constructionPanel != null)
            constructionPanel.SetActive(false);
        
        if (currentPreview != null)
            Destroy(currentPreview);
        
        isPlacingBuilding = false;
        selectedBuildingPrefab = null;
        selectedRecolector = null;
    }
    
    /// <summary>
    /// Puebla los botones de construcción
    /// </summary>
    private void PopulateBuildingButtons()
    {
        Debug.Log("[ConstructionUI] PopulateBuildingButtons llamado");
        
        if (buildingButtonsContainer == null)
        {
            Debug.LogError("[ConstructionUI] buildingButtonsContainer es null");
            return;
        }
        
        if (buildingButtonPrefab == null)
        {
            Debug.LogError("[ConstructionUI] buildingButtonPrefab es null");
            return;
        }
        
        Debug.Log($"[ConstructionUI] Total de prefabs: {buildingPrefabs.Length}");
        Debug.Log($"[ConstructionUI] Total de nombres: {buildingNames.Length}");
        Debug.Log($"[ConstructionUI] Total de costos: {buildingCosts.Length}");
        
        // Limpiar botones existentes
        foreach (Transform child in buildingButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Crear botones para cada edificio
        for (int i = 0; i < buildingPrefabs.Length; i++)
        {
            if (i < buildingNames.Length && i < buildingCosts.Length)
            {
                CreateBuildingButton(i, buildingNames[i], buildingCosts[i]);
            }
            else
            {
                Debug.LogWarning($"[ConstructionUI] Índice {i} fuera de rango para nombres o costos");
            }
        }
    }
    
    /// <summary>
    /// Crea un botón para un edificio específico
    /// </summary>
    private void CreateBuildingButton(int buildingIndex, string buildingName, int cost)
    {
        Debug.Log($"[ConstructionUI] Creando botón para edificio: {buildingName} (índice: {buildingIndex})");
        
        if (buildingButtonPrefab == null)
        {
            Debug.LogError("[ConstructionUI] buildingButtonPrefab es null");
            return;
        }
        
        if (buildingButtonsContainer == null)
        {
            Debug.LogError("[ConstructionUI] buildingButtonsContainer es null");
            return;
        }
        
        GameObject buttonGO = Instantiate(buildingButtonPrefab, buildingButtonsContainer);
        Button button = buttonGO.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonText != null)
        {
            buttonText.text = $"{buildingName}\nCosto: {cost}";
        }
        else
        {
            Debug.LogWarning("[ConstructionUI] No se encontró TextMeshProUGUI en el botón");
        }
        
        if (button != null)
        {
            int index = buildingIndex; // Capturar el índice para el lambda
            button.onClick.AddListener(() => StartBuildingPlacement(index));
            Debug.Log($"[ConstructionUI] Listener agregado al botón para índice: {index}");
        }
        else
        {
            Debug.LogError("[ConstructionUI] No se encontró componente Button en el botón");
        }
    }
    
    /// <summary>
    /// Inicia el proceso de colocación de un edificio
    /// </summary>
    private void StartBuildingPlacement(int buildingIndex)
    {
        Debug.Log($"[ConstructionUI] StartBuildingPlacement llamado con índice: {buildingIndex}");
        
        if (buildingIndex >= 0 && buildingIndex < buildingPrefabs.Length)
        {
            selectedBuildingPrefab = buildingPrefabs[buildingIndex];
            isPlacingBuilding = true;
            
            Debug.Log($"[ConstructionUI] Edificio seleccionado: {selectedBuildingPrefab.name}");
            
            // Crear preview del edificio
            if (currentPreview != null)
                Destroy(currentPreview);
                
            currentPreview = Instantiate(selectedBuildingPrefab);
            SetPreviewTransparency(currentPreview, 0.5f);
            
            Debug.Log($"[ConstructionUI] Preview creado: {currentPreview.name}");
            
            // Ocultar panel de construcción
            if (constructionPanel != null)
                constructionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError($"[ConstructionUI] Índice de edificio inválido: {buildingIndex}. Total de prefabs: {buildingPrefabs.Length}");
        }
    }
    
    /// <summary>
    /// Maneja la colocación del edificio
    /// </summary>
    private void HandleBuildingPlacement()
    {
        if (currentPreview == null) 
        {
            Debug.LogWarning("[ConstructionUI] currentPreview es null en HandleBuildingPlacement");
            return;
        }
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            // Posicionar preview en el punto del mouse
            Vector3 previewPosition = hit.point;
            previewPosition.y += previewHeight;
            currentPreview.transform.position = previewPosition;
            
            // Rotar con la rueda del mouse
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                currentPreview.transform.Rotate(0, Input.GetAxis("Mouse ScrollWheel") * 90f, 0);
            }
            
            // Click izquierdo para colocar
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"[ConstructionUI] Click izquierdo detectado, colocando edificio en {hit.point}");
                PlaceBuilding(hit.point, currentPreview.transform.rotation);
            }
            
            // Click derecho para cancelar
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("[ConstructionUI] Click derecho detectado, cancelando construcción");
                CancelBuildingPlacement();
            }
        }
        else
        {
            Debug.LogWarning("[ConstructionUI] No se detectó groundLayer en el raycast");
        }
    }
    
    /// <summary>
    /// Coloca el edificio en la posición especificada
    /// </summary>
    private void PlaceBuilding(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"[ConstructionUI] PlaceBuilding llamado en posición: {position}");
        
        if (selectedBuildingPrefab != null)
        {
            GameObject building = Instantiate(selectedBuildingPrefab, position, rotation);
            Debug.Log($"[ConstructionUI] Edificio instanciado: {building.name}");
            
            // Agregar componente de construcción si no lo tiene
            if (building.GetComponent<ConstructionBuilding>() == null)
            {
                ConstructionBuilding constructionBuilding = building.AddComponent<ConstructionBuilding>();
                constructionBuilding.buildingRenderer = building.GetComponent<Renderer>();
                constructionBuilding.buildingType = GetBuildingType(selectedBuildingPrefab);
                Debug.Log($"[ConstructionUI] Componente ConstructionBuilding agregado, tipo: {constructionBuilding.buildingType}");
            }
            
            // Asignar el recolector para construir el edificio
            if (selectedRecolector != null)
            {
                selectedRecolector.IrAConstruir(building);
                Debug.Log($"[ConstructionUI] Recolector asignado para construir: {selectedRecolector.name}");
            }
            else
            {
                Debug.LogWarning("[ConstructionUI] selectedRecolector es null");
            }
            
            Debug.Log($"Edificio colocado en {position}");
        }
        else
        {
            Debug.LogError("[ConstructionUI] selectedBuildingPrefab es null en PlaceBuilding");
        }
        
        // Limpiar
        if (currentPreview != null)
            Destroy(currentPreview);
            
        isPlacingBuilding = false;
        selectedBuildingPrefab = null;
    }
    
    /// <summary>
    /// Determina el tipo de edificio basado en el prefab
    /// </summary>
    private string GetBuildingType(GameObject buildingPrefab)
    {
        if (buildingPrefab.GetComponent<MilitaryBuilding>() != null)
            return "Military";
        if (buildingPrefab.GetComponent<ResourceBuilding>() != null)
            return "Resource";
        if (buildingPrefab.GetComponent<ScienceBuilding>() != null)
            return "Science";
        if (buildingPrefab.GetComponent<DefenseBuilding>() != null)
            return "Defense";
        
        return "Basic";
    }
    
    /// <summary>
    /// Cancela la colocación del edificio
    /// </summary>
    private void CancelBuildingPlacement()
    {
        if (currentPreview != null)
            Destroy(currentPreview);
            
        isPlacingBuilding = false;
        selectedBuildingPrefab = null;
        
        // Reabrir panel de construcción
        if (constructionPanel != null)
            constructionPanel.SetActive(true);
    }
    
    /// <summary>
    /// Establece la transparencia del preview
    /// </summary>
    private void SetPreviewTransparency(GameObject preview, float alpha)
    {
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                Color color = material.color;
                color.a = alpha;
                material.color = color;
                
                // Habilitar transparencia
                if (material.HasProperty("_Mode"))
                {
                    material.SetFloat("_Mode", 3); // Transparent mode
                }
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
        }
    }
} 