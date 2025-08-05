using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    private static List<UnitMovement> allUnits = new List<UnitMovement>();
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Animator animator;
    private float speed;

    void Start()
    {
        targetPosition = transform.position;
        animator = GetComponent<Animator>();
        // Obtener la velocidad desde UnitStats
        var stats = GetComponent<UnitStats>();
        if (stats != null)
            speed = stats.velocidadMovimiento;
        else
            speed = 5f; // valor por defecto
    }

    void OnEnable()
    {
        allUnits.Add(this);
    }
    void OnDisable()
    {
        allUnits.Remove(this);
    }

    void Update()
    {
        float distancia = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(targetPosition.x, targetPosition.z)
        );

        float arrivalThreshold = 1.0f; // Umbral aumentado para considerar llegada

        // --- Separación automática entre unidades ---
        Vector3 separation = Vector3.zero;
        int neighborCount = 0;
        float separationRadius = 1.5f; // Ajusta este valor según el tamaño de tus unidades
        foreach (var other in allUnits)
        {
            if (other != this)
            {
                float dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist < separationRadius && dist > 0.01f)
                {
                    separation += (transform.position - other.transform.position) / dist;
                    neighborCount++;
                }
            }
        }
        if (neighborCount > 0)
        {
            separation /= neighborCount;
            separation.y = 0;
            // Aplica una pequeña fuerza de separación
            transform.position += separation * 0.5f * Time.deltaTime;
        }
        // --- Fin separación automática ---

        if (distancia > arrivalThreshold)
        {
            isMoving = true;
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            isMoving = false;
        }

        if (animator != null)
        {
            animator.SetBool("Walk", isMoving);
        }
    }
    
    // Llama este método para mover la unidad a una posición
    public void MoveTo(Vector3 position)
    {
        if (targetPosition != position)
        {
            Debug.Log($"MoveTo llamado con destino: {position}");
            targetPosition = position;
            }
        }

    // Llama este método para animar el ataque
    public void Attack()
    {
        if (animator != null)
        {
            animator.SetBool("Attack", true);
        }
    }

    // Llama este método al final de la animación de ataque (desde un Animation Event)
    public void EndAttack()
    {
        if (animator != null)
        {
            animator.SetBool("Attack", false);
        }
    }

    public void Stop()
    {
        targetPosition = transform.position;
        isMoving = false;
    }
}
