using UnityEngine;
using System.Collections;

public class Recolector : MonoBehaviour
{
    public enum EstadoUnidad { Idle, Moviendose, Recolectando, Construyendo }
    public EstadoUnidad estadoActual = EstadoUnidad.Idle;

    [Header("Recolección")]
    public float velocidadRecoleccion = 1f; // Recursos por segundo
    public int capacidadCarga = 10;
    private int recursosTransportados = 0;
    private GameObject recursoObjetivo;
    private Coroutine rutinaRecoleccion;

    [Header("Construcción")]
    public float velocidadConstruccion = 1f; // Progreso por segundo
    private GameObject edificioObjetivo;
    private Coroutine rutinaConstruccion;

    [Header("Datos de la Unidad")]
    public string faccion = "Todas";

    private UnitMovement movimiento;
    private GameObject ultimoMainBuilding;

    void Start()
    {
        movimiento = GetComponent<UnitMovement>();
    }

    void Update()
    {
        // Aquí puedes añadir lógica para cambiar de estado según la acción
        // Por ejemplo, si está recolectando y llega a la capacidad máxima, cambiar a Moviendose para entregar
    }

    // --- RECOLECCIÓN ---
    public void IrARecurso(GameObject recurso)
    {
        Debug.Log($"[{gameObject.name}] IrARecurso llamado hacia: {recurso.name}");
        recursoObjetivo = recurso;
        movimiento.MoveTo(recurso.transform.position);
        estadoActual = EstadoUnidad.Moviendose;
    }

    public void IrAEntregarRecursos(GameObject edificio)
    {
        edificioObjetivo = edificio;
        movimiento.MoveTo(edificio.transform.position);
        estadoActual = EstadoUnidad.Moviendose;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{gameObject.name}] OnTriggerEnter con: {other.gameObject.name}");
        // --- Entrega de recursos ---
        if (estadoActual == EstadoUnidad.Moviendose && edificioObjetivo != null && other.gameObject == edificioObjetivo)
        {
            MainBuilding mainBuilding = other.GetComponent<MainBuilding>();
            if (mainBuilding != null && recursosTransportados > 0)
            {
                Recurso recursoScript = recursoObjetivo != null ? recursoObjetivo.GetComponent<Recurso>() : null;
                string tipo = recursoScript != null ? recursoScript.tipo.ToString() : "Unknown";
                mainBuilding.ReceiveResources(tipo, recursosTransportados);
                Debug.Log($"[{gameObject.name}] Entregó {recursosTransportados} de {tipo} al edificio principal.");
                VaciarRecursos();
                // Volver al recurso si aún queda
                if (recursoScript != null && recursoScript.cantidad > 0)
                {
                    IrARecurso(recursoObjetivo);
                }
                else
                {
                    estadoActual = EstadoUnidad.Idle;
                    recursoObjetivo = null;
                }
                edificioObjetivo = null;
                return; // Importante: salir para no ejecutar lógica de construcción
            }
        }
        // --- Construcción ---
        if (estadoActual == EstadoUnidad.Moviendose && edificioObjetivo != null && other.gameObject == edificioObjetivo)
        {
            Debug.Log($"[{gameObject.name}] Llegó al edificio para construir: {other.gameObject.name}");
            EmpezarConstruccion();
        }
        if (estadoActual == EstadoUnidad.Moviendose && recursoObjetivo != null && other.gameObject == recursoObjetivo)
        {
            Debug.Log($"[{gameObject.name}] Llegó al recurso: {other.gameObject.name}");
            // Llegó al recurso
            EmpezarRecoleccion();
        }
    }

    void EmpezarRecoleccion()
    {
        if (rutinaRecoleccion == null)
            rutinaRecoleccion = StartCoroutine(RecolectarRutina());
        estadoActual = EstadoUnidad.Recolectando;
    }

    IEnumerator RecolectarRutina()
    {
        Recurso recursoScript = recursoObjetivo != null ? recursoObjetivo.GetComponent<Recurso>() : null;
        if (recursoScript == null)
        {
            Debug.LogWarning($"{gameObject.name} no encontró un script de recurso en el objetivo.");
            estadoActual = EstadoUnidad.Idle;
            rutinaRecoleccion = null;
            yield break;
        }
        if (!recursoScript.PuedeRecolectar(faccion))
        {
            Debug.Log($"{gameObject.name} no puede recolectar este recurso por restricción de facción.");
            estadoActual = EstadoUnidad.Idle;
            rutinaRecoleccion = null;
            yield break;
        }
        while (recursosTransportados < capacidadCarga && recursoScript.cantidad > 0)
        {
            int recolectado = recursoScript.Recolectar(1); // Puedes cambiar la cantidad por ciclo
            recursosTransportados += recolectado;
            Debug.Log($"{gameObject.name} recolectó {recolectado} de {recursoScript.tipo}. Quedan: {recursoScript.cantidad}");
            if (recolectado == 0) break; // Ya no queda recurso
            yield return new WaitForSeconds(1f / velocidadRecoleccion);
        }
        // Cuando se llena, buscar el MainBuilding más cercano y entregar
        ultimoMainBuilding = BuscarMainBuildingMasCercano();
        if (recursosTransportados > 0 && ultimoMainBuilding != null)
        {
            IrAEntregarRecursos(ultimoMainBuilding);
        }
        else
        {
            estadoActual = EstadoUnidad.Idle;
        }
        rutinaRecoleccion = null;
    }

    private GameObject BuscarMainBuildingMasCercano()
    {
        MainBuilding[] buildings = GameObject.FindObjectsOfType<MainBuilding>();
        GameObject closest = null;
        float minDist = Mathf.Infinity;
        foreach (var b in buildings)
        {
            float dist = Vector3.Distance(transform.position, b.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = b.gameObject;
            }
        }
        return closest;
    }

    // --- CONSTRUCCIÓN ---
    public void IrAConstruir(GameObject edificio)
    {
        edificioObjetivo = edificio;
        movimiento.MoveTo(edificio.transform.position);
        estadoActual = EstadoUnidad.Moviendose;
    }

    void EmpezarConstruccion()
    {
        if (rutinaConstruccion == null)
            rutinaConstruccion = StartCoroutine(ConstruirRutina());
        estadoActual = EstadoUnidad.Construyendo;
    }

    IEnumerator ConstruirRutina()
    {
        ConstructionBuilding constructionBuilding = edificioObjetivo.GetComponent<ConstructionBuilding>();
        if (constructionBuilding == null)
        {
            Debug.LogWarning($"{gameObject.name} no encontró un script de construcción en el objetivo.");
            estadoActual = EstadoUnidad.Idle;
            rutinaConstruccion = null;
            yield break;
        }
        
        // Acelerar la construcción
        constructionBuilding.AccelerateConstruction(velocidadConstruccion);
        
        // Esperar hasta que la construcción esté completa
        while (!constructionBuilding.IsConstructionComplete())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log($"{gameObject.name} completó la construcción de {edificioObjetivo.name}");
        estadoActual = EstadoUnidad.Idle;
        rutinaConstruccion = null;
        edificioObjetivo = null;
    }

    // --- Métodos auxiliares ---
    public int ObtenerRecursosTransportados()
    {
        return recursosTransportados;
    }
    public void VaciarRecursos()
    {
        recursosTransportados = 0;
    }
} 