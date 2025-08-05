using UnityEngine;

public class UnitStats : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public float vidaMaxima = 100f; 
    public float vidaActual = 100f;
    public float danoDeAtaque = 20f;
    public float armadura = 10f;
    public float velocidadMovimiento = 5f;

    // Métodos para modificar estadísticas
    public void TomarDaño(float cantidad, bool esMagico = false)
    {
        float defensa = esMagico ? 0f : armadura; // Eliminar resistenciaMagica
        float dañoReducido = Mathf.Max(0, cantidad - defensa);
        vidaActual -= dañoReducido;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
    }

    public void Curar(float cantidad)
    {
        vidaActual += cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
    }

    public bool EstaMuerto()
    {
        
        return vidaActual <= 0;
    }
} 