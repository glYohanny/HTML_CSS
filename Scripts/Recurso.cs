using UnityEngine;
using System.Collections.Generic;

public class Recurso : MonoBehaviour
{
    public enum TipoRecurso
    {
        EsenciasElementales,
        Oro,
        Alimentos,
        Madera,
        Piedra,
        Mana
    }

    [Header("Datos del Recurso")]
    public TipoRecurso tipo;
    public int cantidad = 100;
    public List<string> faccionesPermitidas = new List<string> { "Todas" };

    // Método para verificar si una facción puede recolectar este recurso
    public bool PuedeRecolectar(string faccion)
    {
        return faccionesPermitidas.Contains("Todas") || faccionesPermitidas.Contains(faccion);
    }

    // Método para recolectar una cantidad
    public int Recolectar(int cantidadSolicitada)
    {
        int cantidadRecolectada = Mathf.Min(cantidadSolicitada, cantidad);
        cantidad -= cantidadRecolectada;
        if (cantidad < 0) cantidad = 0;
        return cantidadRecolectada;
    }
} 