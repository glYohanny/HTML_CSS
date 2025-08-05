using UnityEngine;
using UnityEngine.InputSystem;

public class DebugUIManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tecla para alternar la UI de debug")]
    public KeyCode toggleKey = KeyCode.F1;
    
    [Header("Componentes de Debug")]
    [Tooltip("Unit Spawner en la escena")]
    public UnitSpawner unitSpawner;
    [Tooltip("RTS Game Manager en la escena")]
    public RTSGameManager rtsGameManager;
    [Tooltip("Selection Manager en la escena")]
    public SelectionManager selectionManager;
    
    [Header("Estado")]
    [Tooltip("Estado actual de la UI de debug")]
    public bool debugUIEnabled = true;
    
    // Input System
    private InputAction toggleAction;
    
    private void Awake()
    {
        // Configurar Input System
        toggleAction = new InputAction(binding: $"<Keyboard>/{toggleKey}");
        toggleAction.Enable();
    }
    
    private void Start()
    {
        // Buscar componentes automáticamente si no están asignados
        if (unitSpawner == null)
            unitSpawner = FindFirstObjectByType<UnitSpawner>();
            
        if (rtsGameManager == null)
            rtsGameManager = FindFirstObjectByType<RTSGameManager>();
            
        if (selectionManager == null)
            selectionManager = FindFirstObjectByType<SelectionManager>();
            
        // Aplicar estado inicial
        SetDebugUIState(debugUIEnabled);
    }
    
    private void Update()
    {
        // Verificar si se presionó la tecla de alternar
        if (toggleAction.WasPressedThisFrame())
        {
            ToggleDebugUI();
        }
    }
    
    private void OnDestroy()
    {
        toggleAction?.Disable();
    }
    
    /// <summary>
    /// Alterna el estado de la UI de debug
    /// </summary>
    public void ToggleDebugUI()
    {
        debugUIEnabled = !debugUIEnabled;
        SetDebugUIState(debugUIEnabled);
        
        Debug.Log($"Debug UI {(debugUIEnabled ? "activada" : "desactivada")}");
    }
    
    /// <summary>
    /// Establece el estado de la UI de debug
    /// </summary>
    /// <param name="enabled">True para mostrar, False para ocultar</param>
    public void SetDebugUIState(bool enabled)
    {
        debugUIEnabled = enabled;
        
        // Configurar Unit Spawner
        if (unitSpawner != null)
        {
            unitSpawner.showDebugInfo = enabled;
        }
        
        // Configurar RTS Game Manager
        if (rtsGameManager != null)
        {
            rtsGameManager.showDebugInfo = enabled;
        }
        
        // Configurar Selection Manager
        if (selectionManager != null)
        {
            selectionManager.showDebugInfo = enabled;
        }
    }
    
    /// <summary>
    /// Activa la UI de debug
    /// </summary>
    public void EnableDebugUI()
    {
        SetDebugUIState(true);
    }
    
    /// <summary>
    /// Desactiva la UI de debug
    /// </summary>
    public void DisableDebugUI()
    {
        SetDebugUIState(false);
    }
    
    /// <summary>
    /// Obtiene el estado actual de la UI de debug
    /// </summary>
    /// <returns>True si está activada, False si está desactivada</returns>
    public bool IsDebugUIEnabled()
    {
        return debugUIEnabled;
    }
    
    // Método para mostrar información en el inspector
    private void OnGUI()
    {
        if (!debugUIEnabled) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 100, 300, 90));
        GUILayout.Label("Debug UI Manager", GUI.skin.box);
        GUILayout.Label($"Estado: {(debugUIEnabled ? "Activado" : "Desactivado")}");
        GUILayout.Label($"Presiona [{toggleKey}] para alternar");
        GUILayout.EndArea();
    }
} 