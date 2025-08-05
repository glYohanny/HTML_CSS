using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MilitaryBuilding : Building
{
    [Header("Producción de Unidades")]
    public GameObject[] unitPrefabs; // Prefabs de las unidades que puede crear
    public string[] unitNames;       // Nombres de las unidades
    public float[] trainingTimes;    // Tiempo de entrenamiento por unidad (en segundos)
    public int[] unitCosts;          // Coste de cada unidad
    
    [Header("Configuración")]
    public Transform spawnPoint;     // Punto donde aparecen las unidades
    public GameObject unitPanel;     // Panel de UI específico de este edificio
    public Transform buttonContainer; // Contenedor para los botones de unidades
    public GameObject unitButtonPrefab; // Prefab del botón de unidad
    
    [Header("Estado")]
    public bool isTraining = false;
    public int currentTrainingIndex = -1;
    public float trainingProgress = 0f;
    
    private Coroutine trainingCoroutine;
    private GameObject progressBar;
    private TextMeshProUGUI progressText;
    
    void Start()
    {
        // Configurar spawn point si no está asignado
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        Debug.Log($"[MilitaryBuilding] Start - unitPanel: {(unitPanel != null ? unitPanel.name : "NULL")}");
        Debug.Log($"[MilitaryBuilding] Start - buttonContainer: {(buttonContainer != null ? buttonContainer.name : "NULL")}");
        Debug.Log($"[MilitaryBuilding] Start - unitButtonPrefab: {(unitButtonPrefab != null ? unitButtonPrefab.name : "NULL")}");
        Debug.Log($"[MilitaryBuilding] Start - unitPrefabs.Length: {unitPrefabs.Length}");
        Debug.Log($"[MilitaryBuilding] Start - unitNames.Length: {unitNames.Length}");
        Debug.Log($"[MilitaryBuilding] Start - unitCosts.Length: {unitCosts.Length}");
        Debug.Log($"[MilitaryBuilding] Start - trainingTimes.Length: {trainingTimes.Length}");
    }
    
    /// <summary>
    /// Método público para abrir el panel y crear los botones
    /// </summary>
    public void OpenUnitPanel()
    {
        Debug.Log($"[MilitaryBuilding] OpenUnitPanel llamado");
        
        if (unitPanel != null)
        {
            unitPanel.SetActive(true);
            Debug.Log($"[MilitaryBuilding] Panel activado: {unitPanel.name}");
            
            // Crear botones si el contenedor y prefab están disponibles
            if (buttonContainer != null && unitButtonPrefab != null)
            {
                CreateUnitButtons();
            }
            else
            {
                Debug.LogWarning($"[MilitaryBuilding] buttonContainer o unitButtonPrefab es null");
            }
        }
        else
        {
            Debug.LogError($"[MilitaryBuilding] unitPanel es null");
        }
    }
    
    /// <summary>
    /// Crea los botones de unidades en la UI
    /// </summary>
    private void CreateUnitButtons()
    {
        Debug.Log($"[MilitaryBuilding] CreateUnitButtons - Iniciando creación de botones");
        Debug.Log($"[MilitaryBuilding] CreateUnitButtons - unitPrefabs.Length: {unitPrefabs.Length}");
        Debug.Log($"[MilitaryBuilding] CreateUnitButtons - unitNames.Length: {unitNames.Length}");
        Debug.Log($"[MilitaryBuilding] CreateUnitButtons - unitCosts.Length: {unitCosts.Length}");
        Debug.Log($"[MilitaryBuilding] CreateUnitButtons - trainingTimes.Length: {trainingTimes.Length}");
        
        // Limpiar botones existentes
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Crear botones para cada unidad
        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            Debug.Log($"[MilitaryBuilding] CreateUnitButtons - Procesando unidad {i}");
            
            if (i < unitNames.Length && i < unitCosts.Length && i < trainingTimes.Length)
            {
                Debug.Log($"[MilitaryBuilding] CreateUnitButtons - Creando botón para: {unitNames[i]} (Costo: {unitCosts[i]}, Tiempo: {trainingTimes[i]}s)");
                CreateUnitButton(i, unitNames[i], unitCosts[i], trainingTimes[i]);
            }
            else
            {
                Debug.LogWarning($"[MilitaryBuilding] CreateUnitButtons - Índice {i} fuera de rango en algún array");
            }
        }
        
        Debug.Log($"[MilitaryBuilding] CreateUnitButtons - Finalizado");
    }
    
    /// <summary>
    /// Crea un botón para una unidad específica
    /// </summary>
    private void CreateUnitButton(int unitIndex, string unitName, int cost, float trainingTime)
    {
        Debug.Log($"[MilitaryBuilding] CreateUnitButton - Creando botón para {unitName}");
        
        GameObject buttonGO = Instantiate(unitButtonPrefab, buttonContainer);
        Debug.Log($"[MilitaryBuilding] CreateUnitButton - Botón instanciado: {(buttonGO != null ? buttonGO.name : "NULL")}");
        
        Button button = buttonGO.GetComponent<Button>();
        Debug.Log($"[MilitaryBuilding] CreateUnitButton - Componente Button: {(button != null ? "ENCONTRADO" : "NO ENCONTRADO")}");
        
        TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
        Debug.Log($"[MilitaryBuilding] CreateUnitButton - Componente TextMeshProUGUI: {(buttonText != null ? "ENCONTRADO" : "NO ENCONTRADO")}");
        
        if (buttonText != null)
        {
            buttonText.text = $"{unitName}\nCosto: {cost}\nTiempo: {trainingTime}s";
            Debug.Log($"[MilitaryBuilding] CreateUnitButton - Texto asignado: {buttonText.text}");
        }
        else
        {
            Debug.LogError($"[MilitaryBuilding] CreateUnitButton - No se encontró TextMeshProUGUI en el botón");
        }
        
        if (button != null)
        {
            int index = unitIndex; // Capturar el índice para el lambda
            button.onClick.AddListener(() => TrainUnit(index));
            Debug.Log($"[MilitaryBuilding] CreateUnitButton - Listener agregado para unidad {index}");
        }
        else
        {
            Debug.LogError($"[MilitaryBuilding] CreateUnitButton - No se encontró Button en el botón");
        }
    }
    
    /// <summary>
    /// Llama este método para crear una unidad por índice
    /// </summary>
    public void TrainUnit(int unitIndex)
    {
        if (isTraining)
        {
            Debug.Log("Ya hay una unidad en entrenamiento");
            return;
        }
        
        if (unitIndex < 0 || unitIndex >= unitPrefabs.Length)
        {
            Debug.LogError("Índice de unidad inválido");
            return;
        }
        
        // Verificar recursos
        if (!CheckResources(unitIndex))
        {
            Debug.Log("No hay suficientes recursos para entrenar esta unidad");
            return;
        }
        
        // Consumir recursos
        ConsumeResources(unitIndex);
        
        // Iniciar entrenamiento
        StartCoroutine(TrainUnitCoroutine(unitIndex));
    }
    
    /// <summary>
    /// Verifica si hay suficientes recursos para entrenar la unidad
    /// </summary>
    private bool CheckResources(int unitIndex)
    {
        if (unitIndex >= unitCosts.Length) return false;
        
        int cost = unitCosts[unitIndex];
        int currentGold = ResourceManager.Instance.GetResource("Oro");
        
        return currentGold >= cost;
    }
    
    /// <summary>
    /// Consume los recursos necesarios para entrenar la unidad
    /// </summary>
    private void ConsumeResources(int unitIndex)
    {
        if (unitIndex >= unitCosts.Length) return;
        
        int cost = unitCosts[unitIndex];
        ResourceManager.Instance.AddResource("Oro", -cost);
    }
    
    /// <summary>
    /// Corrutina para entrenar la unidad
    /// </summary>
    private IEnumerator TrainUnitCoroutine(int unitIndex)
    {
        isTraining = true;
        currentTrainingIndex = unitIndex;
        trainingProgress = 0f;
        
        float trainingTime = trainingTimes[unitIndex];
        string unitName = unitIndex < unitNames.Length ? unitNames[unitIndex] : unitPrefabs[unitIndex].name;
        
        Debug.Log($"Entrenando unidad: {unitName} durante {trainingTime} segundos");
        
        // Mostrar progreso
        ShowTrainingProgress();
        
        float elapsedTime = 0f;
        while (elapsedTime < trainingTime)
        {
            elapsedTime += Time.deltaTime;
            trainingProgress = elapsedTime / trainingTime;
            
            UpdateTrainingProgress();
            yield return null;
        }
        
        // Crear la unidad
        GameObject unit = Instantiate(unitPrefabs[unitIndex], spawnPoint.position, Quaternion.identity);
        unit.name = $"{unitName}_{System.DateTime.Now.Ticks}";
        
        Debug.Log($"Unidad creada: {unit.name}");
        
        // Limpiar estado
        isTraining = false;
        currentTrainingIndex = -1;
        trainingProgress = 0f;
        HideTrainingProgress();
    }
    
    /// <summary>
    /// Muestra la barra de progreso de entrenamiento
    /// </summary>
    private void ShowTrainingProgress()
    {
        // Aquí puedes crear una barra de progreso visual
        // Por ahora solo usamos logs
    }
    
    /// <summary>
    /// Actualiza la barra de progreso de entrenamiento
    /// </summary>
    private void UpdateTrainingProgress()
    {
        // Aquí puedes actualizar la barra de progreso visual
        // Por ahora solo usamos logs
    }
    
    /// <summary>
    /// Oculta la barra de progreso de entrenamiento
    /// </summary>
    private void HideTrainingProgress()
    {
        // Aquí puedes ocultar la barra de progreso visual
    }
    
    /// <summary>
    /// Obtiene el progreso actual del entrenamiento (0-1)
    /// </summary>
    public float GetTrainingProgress()
    {
        return trainingProgress;
    }
    
    /// <summary>
    /// Verifica si está entrenando una unidad
    /// </summary>
    public bool IsTraining()
    {
        return isTraining;
    }
    
    /// <summary>
    /// Obtiene el nombre de la unidad en entrenamiento
    /// </summary>
    public string GetTrainingUnitName()
    {
        if (currentTrainingIndex >= 0 && currentTrainingIndex < unitNames.Length)
        {
            return unitNames[currentTrainingIndex];
        }
        return "Desconocida";
    }
} 