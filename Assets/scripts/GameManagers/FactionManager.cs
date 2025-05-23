using UnityEngine;

public class FactionManager : MonoBehaviour
{
    [field: SerializeField] public Factions Faction { get; private set; }

    [SerializeField] private float _maxBaseHealth;
    [SerializeField] private float _currentBaseHealth;
    [SerializeField] private SquadScriptableObject[] _availableSquads;

    private void Awake()
    {
        SetupBaseToBattle();
    }

    public void TakeBaseDamage(float damage)
    {
        _currentBaseHealth -= damage;

        if (_currentBaseHealth <= 0)
        {
            Debug.Log($"Facção {Faction} foi derrotada");
        }
    }

    private void SetupBaseToBattle()
    {
        _currentBaseHealth = _maxBaseHealth;
    }

    public SquadScriptableObject[] GetAvailableSquads() => _availableSquads;
    
}
