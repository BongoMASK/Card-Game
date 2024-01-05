using UnityEngine;

public abstract class AttackPassive : AppliedPassive
{
    private void OnEnable() {
        AttackPassive ap = GetComponent<AttackPassive>();

        if (ap != this)
            Destroy(this);
    }

    public virtual void Attack(BaseCard attacker, BaseCard target) {
        Debug.Log(attacker.cardStats.cardName + " trying to attack " + target.name);
    }

    public abstract bool ValidateAttack(BaseCard attacker, BaseCard target);
}
