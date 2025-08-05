using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // Diccionario global de recursos
    private Dictionary<string, int> recursos = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Agregar recursos
    public void AddResource(string tipo, int cantidad)
    {
        if (!recursos.ContainsKey(tipo))
            recursos[tipo] = 0;
        recursos[tipo] += cantidad;
        Debug.Log($"[ResourceManager] Añadido {cantidad} de {tipo}. Total: {recursos[tipo]}");
    }

    // Consultar cantidad de un recurso
    public int GetResource(string tipo)
    {
        if (recursos.ContainsKey(tipo))
            return recursos[tipo];
        return 0;
    }

    // Obtener todos los recursos (opcional)
    public Dictionary<string, int> GetAllResources()
    {
        return new Dictionary<string, int>(recursos);
    }
} 