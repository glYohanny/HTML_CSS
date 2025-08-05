using UnityEngine;
using System.Collections;

public class ConstructionBuilding : MonoBehaviour
{
    [Header("Construcción")]
    public float constructionTime = 10f;
    public int constructionCost = 100;
    public string buildingType = "Basic";
    
    [Header("Estado")]
    public bool isUnderConstruction = true;
    public float constructionProgress = 0f;
    public float constructionSpeed = 1f;
    
    [Header("Componentes")]
    public GameObject constructionPreview;
    public GameObject completedBuilding;
    public Renderer buildingRenderer;
    
    [Header("Efectos")]
    public ParticleSystem constructionParticles;
    public AudioSource constructionSound;
    
    private Coroutine constructionCoroutine;
    private Material originalMaterial;
    private Color constructionColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    
    void Start()
    {
        // Configurar el edificio para construcción
        SetupForConstruction();
        
        // Iniciar construcción automáticamente
        StartConstruction();
    }
    
    /// <summary>
    /// Configura el edificio para estar en construcción
    /// </summary>
    private void SetupForConstruction()
    {
        isUnderConstruction = true;
        constructionProgress = 0f;
        
        // Configurar material de construcción
        if (buildingRenderer != null)
        {
            originalMaterial = buildingRenderer.material;
            buildingRenderer.material.color = constructionColor;
        }
        
        // Activar efectos de construcción
        if (constructionParticles != null)
            constructionParticles.Play();
            
        if (constructionSound != null)
            constructionSound.Play();
    }
    
    /// <summary>
    /// Inicia el proceso de construcción
    /// </summary>
    public void StartConstruction()
    {
        if (constructionCoroutine != null)
            StopCoroutine(constructionCoroutine);
            
        constructionCoroutine = StartCoroutine(ConstructionProcess());
    }
    
    /// <summary>
    /// Proceso de construcción
    /// </summary>
    private IEnumerator ConstructionProcess()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < constructionTime)
        {
            elapsedTime += Time.deltaTime * constructionSpeed;
            constructionProgress = elapsedTime / constructionTime;
            
            // Actualizar apariencia visual
            UpdateConstructionVisual();
            
            yield return null;
        }
        
        // Construcción completada
        CompleteConstruction();
    }
    
    /// <summary>
    /// Actualiza la apariencia visual durante la construcción
    /// </summary>
    private void UpdateConstructionVisual()
    {
        if (buildingRenderer != null)
        {
            Color currentColor = Color.Lerp(constructionColor, Color.white, constructionProgress);
            buildingRenderer.material.color = currentColor;
        }
    }
    
    /// <summary>
    /// Completa la construcción
    /// </summary>
    private void CompleteConstruction()
    {
        isUnderConstruction = false;
        constructionProgress = 1f;
        
        // Restaurar material original
        if (buildingRenderer != null && originalMaterial != null)
        {
            buildingRenderer.material = originalMaterial;
        }
        
        // Detener efectos de construcción
        if (constructionParticles != null)
            constructionParticles.Stop();
            
        if (constructionSound != null)
            constructionSound.Stop();
        
        // Activar el edificio completado
        if (completedBuilding != null)
        {
            completedBuilding.SetActive(true);
        }
        
        // Desactivar preview de construcción
        if (constructionPreview != null)
        {
            constructionPreview.SetActive(false);
        }
        
        // Agregar componentes necesarios según el tipo de edificio
        SetupCompletedBuilding();
        
        Debug.Log($"Edificio {buildingType} completado en {transform.position}");
    }
    
    /// <summary>
    /// Configura el edificio completado según su tipo
    /// </summary>
    private void SetupCompletedBuilding()
    {
        switch (buildingType.ToLower())
        {
            case "military":
                gameObject.AddComponent<MilitaryBuilding>();
                break;
            case "resource":
                gameObject.AddComponent<ResourceBuilding>();
                break;
            case "science":
                gameObject.AddComponent<ScienceBuilding>();
                break;
            case "defense":
                gameObject.AddComponent<DefenseBuilding>();
                break;
            default:
                // Edificio básico
                break;
        }
        
        // Agregar tag de edificio
        gameObject.tag = "Building";
    }
    
    /// <summary>
    /// Acelera la construcción (usado por recolectores)
    /// </summary>
    public void AccelerateConstruction(float speedMultiplier)
    {
        constructionSpeed *= speedMultiplier;
    }
    
    /// <summary>
    /// Obtiene el progreso de construcción (0-1)
    /// </summary>
    public float GetConstructionProgress()
    {
        return constructionProgress;
    }
    
    /// <summary>
    /// Verifica si la construcción está completada
    /// </summary>
    public bool IsConstructionComplete()
    {
        return !isUnderConstruction;
    }
    
    /// <summary>
    /// Obtiene el tiempo restante de construcción
    /// </summary>
    public float GetRemainingConstructionTime()
    {
        if (isUnderConstruction)
        {
            return (constructionTime - (constructionProgress * constructionTime)) / constructionSpeed;
        }
        return 0f;
    }
    
    // Método para mostrar información en el inspector
    private void OnGUI()
    {
        if (isUnderConstruction)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPos.z > 0)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 20), 
                         $"Construyendo: {constructionProgress:P0}");
            }
        }
    }
} 