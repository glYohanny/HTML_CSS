using UnityEngine;

public class MainBuilding : MonoBehaviour
{
    // Este método lo llamará el recolector al llegar al edificio
    public void ReceiveResources(string tipo, int cantidad)
    {
        ResourceManager.Instance.AddResource(tipo, cantidad);
        Debug.Log($"[MainBuilding] Recibido {cantidad} de {tipo}.");
    }
} 