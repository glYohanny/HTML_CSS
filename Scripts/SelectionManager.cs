using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SelectionManager : MonoBehaviour
{
    [Header("Componentes")]
    private Camera cam;
    private Canvas canvas;
    
    [Header("Configuración")]
    public LayerMask selectableLayer = 1; // Default layer para unidades seleccionables
    public LayerMask groundLayer = 1; // Layer para el terreno
    
    [Header("Selección Múltiple")]
    public float dragThreshold = 5f; // Distancia mínima para considerar un drag
    public bool enableDragSelection = true;
     
    [Header("UI")]
    public GameObject selectionBoxPrefab; // Prefab para la caja de selección visual
    public GameObject unitPanel; // Asigna tu UnitPanel aquí en el Inspector
    public ConstructionUI constructionUI; // Referencia a la interfaz de construcción
    private GameObject currentSelectionBox;
    private RectTransform selectionBoxRect;
    private MilitaryBuilding selectedMilitaryBuilding;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    // Input System
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction mousePositionAction;
    private InputAction shiftAction;
    
    // Variables de selección
    private Vector2 dragStartPosition;
    private bool isDragging = false;
    private List<GameObject> selectedUnits = new List<GameObject>();
    private List<GameObject> allSelectableUnits = new List<GameObject>();
    private bool clickSelectionPending = false;
    
    // Singleton
    public static SelectionManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        { 
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Obtener componentes
        cam = Camera.main;
        canvas = FindFirstObjectByType<Canvas>();
        if (cam == null)
        {
            Debug.LogError("No se encontró la cámara principal!");
            enabled = false;
            return;
        }
        if (canvas == null)
        {
            Debug.LogError("No se encontró un Canvas en la escena!");
            enabled = false;
            return;
        }
        // Configurar Input System
        SetupInputSystem();
        // Crear caja de selección
        CreateSelectionBox();
        if (showDebugInfo)
        {
            Debug.Log("SelectionManager inicializado correctamente");
        }
    }
    
    private void SetupInputSystem()
    {
        try
        {
            // Crear acciones directamente
            leftClickAction = new InputAction(binding: "<Mouse>/leftButton");
            rightClickAction = new InputAction(binding: "<Mouse>/rightButton");
            mousePositionAction = new InputAction(binding: "<Mouse>/position");
            shiftAction = new InputAction(binding: "<Keyboard>/leftShift");
            shiftAction.AddBinding("<Keyboard>/rightShift");
            leftClickAction.Enable();
            rightClickAction.Enable();
            mousePositionAction.Enable();
            shiftAction.Enable();
            if (showDebugInfo)
            {
                Debug.Log("Input System configurado correctamente");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error configurando Input System: {e.Message}");
        }
    }
    
    private void CreateSelectionBox()
    {
        if (selectionBoxPrefab != null)
        {
            currentSelectionBox = Instantiate(selectionBoxPrefab, Vector3.zero, Quaternion.identity);
            selectionBoxRect = currentSelectionBox.GetComponent<RectTransform>();
            currentSelectionBox.SetActive(false);
        }
        else if (canvas != null)
        {
            // Buscar un hijo llamado 'SelectionBox' en el Canvas
            Transform box = canvas.transform.Find("SelectionBox");
            if (box != null)
            {
                currentSelectionBox = box.gameObject;
                selectionBoxRect = currentSelectionBox.GetComponent<RectTransform>();
                currentSelectionBox.SetActive(false);
                return;
            }
            // Si no existe, crear una caja básica
            CreateBasicSelectionBox();
        }
        else
        {
            // Si no hay canvas, crear uno y la caja
            CreateBasicSelectionBox();
        }
    }
    
    private void CreateBasicSelectionBox()
    {
        // Crear un GameObject para la caja de selección
        currentSelectionBox = new GameObject("SelectionBox");
        currentSelectionBox.transform.SetParent(transform);
        // Agregar componentes necesarios
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        currentSelectionBox.transform.SetParent(canvas.transform, false);
        UnityEngine.UI.Image image = currentSelectionBox.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 1, 0, 0.3f); // Verde semi-transparente
        image.raycastTarget = false;
        selectionBoxRect = currentSelectionBox.GetComponent<RectTransform>();
        currentSelectionBox.SetActive(false);
    }
    
    private void Update()
    {
        if (cam == null)
        {
            Debug.LogWarning("[Update] Camera es null");
            return;
        }
        
        if (leftClickAction == null)
        {
            Debug.LogWarning("[Update] leftClickAction es null");
            return;
        }
        
        if (mousePositionAction == null)
        {
            Debug.LogWarning("[Update] mousePositionAction es null");
            return;
        }
        
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        
        // Verificar si el mouse está sobre UI
        if (IsMouseOverUI())
        {
            // Resetear estado si está sobre UI
            if (leftClickAction.WasPressedThisFrame())
            {
                clickSelectionPending = false;
                isDragging = false;
            }
            return; // No procesar clicks si está sobre UI
        }
        
        // Detectar inicio de drag o clic
        if (leftClickAction.WasPressedThisFrame())
        {
            dragStartPosition = mousePos;
            isDragging = false;
            clickSelectionPending = true;
        }
        // Detectar drag
        if (leftClickAction.IsPressed() && clickSelectionPending)
        {
            float dragDistance = Vector2.Distance(dragStartPosition, mousePos);
            if (dragDistance > dragThreshold)
            {
                isDragging = true;
                clickSelectionPending = false;
                StartDrag(dragStartPosition);
            }
        }
        // Actualizar drag
        if (isDragging)
        {
            UpdateDrag(mousePos);
        }
        // Detectar fin de drag o clic
        if (leftClickAction.WasReleasedThisFrame())
        {
            if (isDragging)
            {
                EndDrag(mousePos);
            }
            else if (clickSelectionPending)
            {
                // Selección por clic
                SelectByClick(mousePos);
                clickSelectionPending = false;
            }
        }
        // Detectar clic derecho para movimiento
        if (rightClickAction.WasPressedThisFrame())
        {
            HandleRightClick(mousePos);
        }
    }
    
    /// <summary>
    /// Verifica si el mouse está sobre un elemento de UI
    /// </summary>
    private bool IsMouseOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        
        // Solo bloquear si está sobre un botón específicamente
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem.IsPointerOverGameObject())
        {
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            var eventData = new UnityEngine.EventSystems.PointerEventData(eventSystem);
            eventData.position = Input.mousePosition;
            eventSystem.RaycastAll(eventData, results);
            
            foreach (var result in results)
            {
                // Solo bloquear si está sobre un botón
                if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private void StartDrag(Vector2 mousePos)
    {
        // Mostrar caja de selección
        if (currentSelectionBox != null)
        {
            currentSelectionBox.SetActive(true);
            UpdateSelectionBox(mousePos);
        }
    }
    
    private void UpdateDrag(Vector2 mousePos)
    {
        if (currentSelectionBox != null)
        {
            UpdateSelectionBox(mousePos);
        }
    }
    
    private void EndDrag(Vector2 mousePos)
    {
        isDragging = false;
        // Ocultar caja de selección
        if (currentSelectionBox != null)
        {
            currentSelectionBox.SetActive(false);
        }
        // Verificar si fue un drag válido
        float dragDistance = Vector2.Distance(dragStartPosition, mousePos);
        if (dragDistance > dragThreshold && enableDragSelection)
        {
            // Selección múltiple por drag
            SelectUnitsInBox(dragStartPosition, mousePos);
        }
    }
    
    private void UpdateSelectionBox(Vector2 currentMousePos)
    {
        if (selectionBoxRect == null || canvas == null) return;
        Vector2 startLocal, endLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, dragStartPosition, canvas.worldCamera, out startLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, currentMousePos, canvas.worldCamera, out endLocal);
        Vector2 size = endLocal - startLocal;
        selectionBoxRect.anchoredPosition = startLocal;
        selectionBoxRect.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
        selectionBoxRect.pivot = new Vector2(size.x >= 0 ? 0 : 1, size.y >= 0 ? 0 : 1);
    }
    
    private void SelectByClick(Vector2 mousePos)
    {
        bool isShiftPressed = shiftAction.ReadValue<float>() > 0.5f;
        GameObject clickedUnit = GetUnitAtPosition(mousePos);
        
        Debug.Log($"[SelectByClick] clickedUnit: {(clickedUnit != null ? clickedUnit.name : "null")}");
        
        if (clickedUnit != null)
        {
            Debug.Log($"[Depuración] Clicked object: {clickedUnit.name}");
            
            // --- Primero: ¿Es un edificio militar? ---
            MilitaryBuilding building = clickedUnit.GetComponent<MilitaryBuilding>();
            if (building != null)
            {
                Debug.Log($"[Depuración] ¡Edificio militar detectado: {clickedUnit.name}!");
                Debug.Log($"[Depuración] building.unitPanel: {(building.unitPanel != null ? building.unitPanel.name : "NULL")}");
                
                // Desactiva el panel del edificio anterior si existe
                if (selectedMilitaryBuilding != null && selectedMilitaryBuilding.unitPanel != null)
                    selectedMilitaryBuilding.unitPanel.SetActive(false);
                selectedMilitaryBuilding = building;
                
                // Seleccionar la unidad normalmente
                if (isShiftPressed)
                {
                    if (IsUnitSelected(clickedUnit))
                    {
                        DeselectUnit(clickedUnit);
                    }
                    else
                    {
                        SelectUnit(clickedUnit);
                    }
                }
                else
                {
                    DeselectAll();
                    SelectUnit(clickedUnit);
                }
                
                // Activa el panel del edificio seleccionado
                Debug.Log($"[Depuración] selectedMilitaryBuilding: {(selectedMilitaryBuilding != null ? selectedMilitaryBuilding.name : "NULL")}");
                Debug.Log($"[Depuración] selectedMilitaryBuilding.unitPanel: {(selectedMilitaryBuilding != null && selectedMilitaryBuilding.unitPanel != null ? selectedMilitaryBuilding.unitPanel.name : "NULL")}");
                
                if (selectedMilitaryBuilding != null)
                {
                    Debug.Log("[Depuración] Llamando OpenUnitPanel del edificio seleccionado");
                    selectedMilitaryBuilding.OpenUnitPanel();
                }
                else
                {
                    Debug.LogWarning("[Depuración] selectedMilitaryBuilding es null");
                }
                return;
            }
            else
            {
                Debug.Log("[Depuración] No es un edificio militar");
                // Desactiva el panel del edificio anterior si existe
                if (selectedMilitaryBuilding != null && selectedMilitaryBuilding.unitPanel != null)
                    selectedMilitaryBuilding.unitPanel.SetActive(false);
                selectedMilitaryBuilding = null;
            }
            
            // --- Segundo: ¿Es un recolector? ---
            Recolector recolector = clickedUnit.GetComponent<Recolector>();
            if (recolector != null)
            {
                Debug.Log("[Depuración] ¡Recolector detectado!");
                // Cerrar cualquier interfaz abierta
                if (selectedMilitaryBuilding != null && selectedMilitaryBuilding.unitPanel != null)
                    selectedMilitaryBuilding.unitPanel.SetActive(false);
                selectedMilitaryBuilding = null;
                
                // Seleccionar la unidad normalmente
                if (isShiftPressed)
                {
                    if (IsUnitSelected(clickedUnit))
                    {
                        DeselectUnit(clickedUnit);
                    }
                    else
                    {
                        SelectUnit(clickedUnit);
                    }
                }
                else
                {
                    DeselectAll();
                    SelectUnit(clickedUnit);
                }
                
                // Abrir interfaz de construcción
                if (constructionUI != null)
                {
                    constructionUI.OpenConstructionUI(recolector);
                }
                else
                {
                    Debug.LogWarning("[Depuración] constructionUI es null");
                }
                return;
            }
            
            // --- Si no es edificio ni recolector, sigue con la lógica de unidades ---
            Debug.Log($"[SelectByClick] Click sobre: {clickedUnit.name} | Shift: {isShiftPressed}");
            if (isShiftPressed)
            {
                if (IsUnitSelected(clickedUnit))
                {
                    DeselectUnit(clickedUnit);
                }
                else
                {
                    SelectUnit(clickedUnit);
                }
            }
            else
            {
                DeselectAll();
                SelectUnit(clickedUnit);
            }
        }
        else
        {
            Debug.Log("[Depuración] No se hizo click en ningún objeto seleccionable");
            if (!isShiftPressed)
            {
                DeselectAllAndCloseUI();
            }
            // Desactiva el panel del edificio anterior si existe
            if (selectedMilitaryBuilding != null && selectedMilitaryBuilding.unitPanel != null)
                selectedMilitaryBuilding.unitPanel.SetActive(false);
            selectedMilitaryBuilding = null;
        }
    }
    
    private GameObject GetUnitAtPosition(Vector2 screenPos)
    {
        if (cam == null)
        {
            Debug.LogError("[GetUnitAtPosition] Camera es null");
            return null;
        }
        
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
        {
            if (hit.collider != null)
            {
                return hit.collider.gameObject;
            }
            else
            {
                Debug.LogWarning("[GetUnitAtPosition] hit.collider es null");
                return null;
            }
        }
        return null;
    }
    
    private void SelectUnitsInBox(Vector2 startPos, Vector2 endPos)
    {
        bool isShiftPressed = shiftAction.ReadValue<float>() > 0.5f;
        Vector2 min = Vector2.Min(startPos, endPos);
        Vector2 max = Vector2.Max(startPos, endPos);
        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");
        if (!isShiftPressed)
        {
            DeselectAllAndCloseUI();
        }
        
        bool hasRecolector = false;
        Recolector selectedRecolector = null;
        
        foreach (GameObject unit in selectableUnits)
        {
            Vector3 unitScreenPos = cam.WorldToScreenPoint(unit.transform.position);
            if (unitScreenPos.x >= min.x && unitScreenPos.x <= max.x &&
                unitScreenPos.y >= min.y && unitScreenPos.y <= max.y)
            {
                // Verificar si es un recolector
                Recolector recolector = unit.GetComponent<Recolector>();
                if (recolector != null)
                {
                    hasRecolector = true;
                    selectedRecolector = recolector;
                }
                
                if (isShiftPressed)
                {
                    if (!IsUnitSelected(unit))
                        SelectUnit(unit);
                }
                else
                {
                    SelectUnit(unit);
                }
            }
        }
        
        // Si hay un recolector seleccionado, abrir la interfaz de construcción
        if (hasRecolector && selectedRecolector != null && constructionUI != null)
        {
            constructionUI.OpenConstructionUI(selectedRecolector);
        }
    }
    
    private void SelectUnit(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            SetUnitSelectionVisual(unit, true);
            if (showDebugInfo)
            {
                Debug.Log($"Unidad seleccionada: {unit.name}");
            }
        }
        else
        {
            // Refuerza: si ya está seleccionada, asegúrate de que el visual esté activo
            SetUnitSelectionVisual(unit, true);
            if (showDebugInfo)
            {
                Debug.Log($"Unidad ya estaba seleccionada (reforzando visual): {unit.name}");
            }
        }
        
        // Verificar si hay recolectores seleccionados y abrir interfaz de construcción
        CheckForRecolectorAndOpenUI();
    }
    
    /// <summary>
    /// Verifica si hay recolectores seleccionados y abre la interfaz de construcción
    /// </summary>
    private void CheckForRecolectorAndOpenUI()
    {
        if (constructionUI == null) return;
        
        foreach (GameObject unit in selectedUnits)
        {
            Recolector recolector = unit.GetComponent<Recolector>();
            if (recolector != null)
            {
                constructionUI.OpenConstructionUI(recolector);
                return; // Solo necesitamos encontrar uno
            }
        }
    }
    
    private void DeselectUnit(GameObject unit)
    {
        if (selectedUnits.Contains(unit))
        {
            selectedUnits.Remove(unit);
            SetUnitSelectionVisual(unit, false);
            if (showDebugInfo)
            {
                Debug.Log($"Unidad deseleccionada: {unit.name}");
            }
        }
        
        // Si se deseleccionó un edificio militar, cerrar su panel
        MilitaryBuilding building = unit.GetComponent<MilitaryBuilding>();
        if (building != null && building == selectedMilitaryBuilding)
        {
            if (selectedMilitaryBuilding != null && selectedMilitaryBuilding.unitPanel != null)
            {
                selectedMilitaryBuilding.unitPanel.SetActive(false);
            }
            selectedMilitaryBuilding = null;
        }
        
        // Verificar si aún hay recolectores seleccionados
        CheckForRecolectorAndOpenUI();
    }
    
    private void DeselectAll()
    {
        foreach (GameObject unit in selectedUnits)
        {
            SetUnitSelectionVisual(unit, false);
        }
        selectedUnits.Clear();
        if (showDebugInfo)
        {
            Debug.Log("Todas las unidades deseleccionadas");
        }
        // NO desactivar el panel del edificio aquí, solo limpiar la referencia
        // selectedMilitaryBuilding = null;
    }
    
    /// <summary>
    /// Deselecciona todo y cierra todas las interfaces
    /// </summary>
    private void DeselectAllAndCloseUI()
    {
        DeselectAll();
        
        // Cerrar panel del edificio militar si está abierto
        if (selectedMilitaryBuilding != null && selectedMilitaryBuilding.unitPanel != null)
        {
            selectedMilitaryBuilding.unitPanel.SetActive(false);
            selectedMilitaryBuilding = null;
        }
        
        // Cerrar interfaz de construcción si está abierta
        if (constructionUI != null)
        {
            constructionUI.CloseConstructionUI();
        }
    }
    
    private void SetUnitSelectionVisual(GameObject unit, bool selected)
    {
        var selectionVisual = unit.GetComponent<UnitSelectionVisual>();
        if (selectionVisual != null)
        {
            selectionVisual.SetSelected(selected);
        }
        else
        {
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (selected)
                {
                    renderer.material.color = Color.green;
                }
                else
                {
                    renderer.material.color = Color.white;
                }
            }
        }
    }
    
    private void HandleRightClick(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log($"Enemigo detectado (click derecho): {hit.collider.gameObject.name}");
                if (selectedUnits.Count > 0)
                {
                    foreach (var unidad in selectedUnits)
                    {
                        var unitAttack = unidad.GetComponent<UnitAttack>();
                        if (unitAttack != null)
                        {
                            unitAttack.AttackTarget(hit.collider.gameObject);
                        }
                    }
                }
            }
            else if (hit.collider.CompareTag("Resource") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Resource"))
            {
                Debug.Log($"Recurso detectado (click derecho): {hit.collider.gameObject.name}");
                if (selectedUnits.Count > 0)
                {
                    foreach (var unidad in selectedUnits)
                    {
                        var recolector = unidad.GetComponent<Recolector>();
                        if (recolector != null)
                        {
                            recolector.IrARecurso(hit.collider.gameObject);
                        }
                    }
                }
            }
            else if (hit.collider.CompareTag("Ground") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (selectedUnits.Count > 0)
                {
                    // --- Formación en cuadrícula ---
                    int unitsCount = selectedUnits.Count;
                    int rowSize = Mathf.CeilToInt(Mathf.Sqrt(unitsCount));
                    float spacing = 1.5f; // Espacio entre unidades
                    Vector3 center = hit.point;
                    int i = 0;
                    foreach (var unidad in selectedUnits)
                    {
                        var unitAttack = unidad.GetComponent<UnitAttack>();
                        if (unitAttack != null)
                        {
                            unitAttack.CancelAttack();
                        }
                        var unitMovement = unidad.GetComponent<UnitMovement>();
                        if (unitMovement != null)
                        {
                            int row = i / rowSize;
                            int col = i % rowSize;
                            float offsetX = (col - (rowSize - 1) / 2.0f) * spacing;
                            float offsetZ = (row - (rowSize - 1) / 2.0f) * spacing;
                            Vector3 formationPos = center + new Vector3(offsetX, 0, offsetZ);
                            unitMovement.MoveTo(formationPos);
                        }
                        i++;
                    }
                    // --- Fin formación ---
                }
            }
        }
    }
    // Métodos públicos para acceder desde otros scripts
    public List<GameObject> GetSelectedUnits()
    {
        return new List<GameObject>(selectedUnits);
    }
    public bool IsUnitSelected(GameObject unit)
    {
        return selectedUnits.Contains(unit);
    }
    public int GetSelectedUnitsCount()
    {
        return selectedUnits.Count;
    }
    public void OnTrainUnitButton(int unitIndex)
    {
        if (selectedMilitaryBuilding != null)
        {
            selectedMilitaryBuilding.TrainUnit(unitIndex);
        }
        else
        {
            Debug.LogWarning("No hay edificio militar seleccionado.");
        }
    }
    private void OnDestroy()
    {
        if (leftClickAction != null)
        {
            leftClickAction.Disable();
            leftClickAction.Dispose();
        }
        if (rightClickAction != null)
        {
            rightClickAction.Disable();
            rightClickAction.Dispose();
        }
        if (mousePositionAction != null)
        {
            mousePositionAction.Disable();
            mousePositionAction.Dispose();
        }
        if (shiftAction != null)
        {
            shiftAction.Disable();
            shiftAction.Dispose();
        }
    }
} 