using UnityEngine;

public class NormalAttack : AttackPassive {

    public override void Attack(BaseCard attacker, BaseCard target) {
        base.Attack(attacker, target);

        RegularAttack(attacker, target);
    }

    public override bool ValidateAttack(BaseCard attacker, BaseCard target) {
        throw new System.NotImplementedException();
    }

    private void RegularAttack(BaseCard attacker, BaseCard target) {
        bool b = CardFunctions.instance.CheckForBlockers(target, attacker);

        if (!b) {
            GameControllerUI.instance.SetMessage(attacker.cardStats.cardName + " attacked " + target.cardStats.cardName);
            target.TakeDamage(attacker.effectiveDamage);
        }

        attacker.hasAttacked = true;
        attacker.cardOwner.UsedMana(attacker.cardStats.manaCost);

        AudioManager.instance.Play(SoundNames.attack);
    }
}