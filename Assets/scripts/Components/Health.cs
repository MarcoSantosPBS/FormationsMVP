using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] public float maxHealth;
    [SerializeField] GameObject healthBar;
    

    public Action OnDeath;
    [SerializeField] private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        //healthBar.transform.localScale = new Vector3(GetHealthPercentage(), 1, 1); 
    }

    public bool TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke();
            return true;
        }

        return false;
    }

    private float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
}
