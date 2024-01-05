using UnityEngine;

public class DoubleAttack : AttackPassive
{
    public override void Attack(BaseCard attacker, BaseCard target) {
        base.Attack(attacker, target);

        AttackTwice(attacker, target);
    }

    public override bool ValidateAttack(BaseCard attacker, BaseCard target) {
        throw new System.NotImplementedException();
    }

    private void AttackTwice(BaseCard attacker, BaseCard target) {
        Debug.Log("Attadjkafdasklfjd;laskjfkldas");
    }
}
