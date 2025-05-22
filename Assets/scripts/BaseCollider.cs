using System;
using UnityEngine;

public class BaseCollider : MonoBehaviour
{
    [SerializeField] private SquadFriendlyType _baseType;
    [SerializeField] private Factions _faction;

    public event Action<Factions> OnCollisionDetected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out SquadController squad))
        {
            if (squad.Faction != _faction)
            {
                OnCollisionDetected?.Invoke(_faction);
                Destroy(squad.gameObject);
            }
        }
    }

}
