using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    public float rangoAtaque = 2f;
    public float velocidadAtaque = 1f; // Para controlar la velocidad de la animación
    private Animator animator;
    private GameObject objetivo;
    private UnitMovement movement;
    private bool atacando = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<UnitMovement>();
    }

    void Update()
    {
        if (objetivo != null)
        {
            float distancia = Vector3.Distance(transform.position, objetivo.transform.position);
            if (distancia > rangoAtaque)
            {
                // Caminar hacia el objetivo
                movement.MoveTo(objetivo.transform.position);
                animator.SetBool("Walk", true);
            }
            else
            {
                // Detenerse y atacar
                movement.Stop();
                animator.SetBool("Walk", false);
                Vector3 lookDir = objetivo.transform.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);
                if (!atacando)
                {
                    animator.SetFloat("AttackSpeed", velocidadAtaque);
                    animator.SetBool("Attack", true);
                    atacando = true;
                }
            }
        }
    }

    public void AttackTarget(GameObject target)
    {
        objetivo = target;
        atacando = false;
    }

    // Llama este método al final de la animación de ataque (Animation Event)
    public void EndAttack()
    {
        animator.SetBool("Attack", false);
        atacando = false;
    }

    public void CancelAttack()
    {
        objetivo = null;
        animator.SetBool("Attack", false);
        atacando = false;
    }
} 