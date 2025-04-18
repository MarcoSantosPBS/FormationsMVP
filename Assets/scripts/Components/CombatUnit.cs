using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    [SerializeField] public int damage;
    [SerializeField] private float attackInterval = 2.5f;
    [SerializeField] public Unit targetUnit;

    private float lastAttackTime = -1;

    private void Update()
    {
        lastAttackTime += Time.deltaTime;

        if (targetUnit != null)
        {
            bool isEnemyDead = Attack();

            if (isEnemyDead)
            {
                targetUnit = null;
            }
        }
    }

    public bool Attack()
    {
        if (lastAttackTime > attackInterval)
        {
            lastAttackTime = 0f;
            return targetUnit.TakeDamage(damage);
        }

        return false;
    }

    public void StopAttacking()
    {
        targetUnit = null;
    }
}
