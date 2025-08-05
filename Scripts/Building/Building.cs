using UnityEngine;

public abstract class Building : MonoBehaviour
{
    public string buildingName;
    public int maxHealth;
    public int currentHealth;
    public Vector3 position;

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    public virtual void DestroyBuilding()
    {
        // Lógica de destrucción (animación, efectos, etc.)
        Destroy(gameObject);
    }
} 