using UnityEngine;

public class AttackSystem : MonoBehaviour {

    public bool ValidateAttack(BaseCard original, BaseCard target) {    

        if(original == null || target == null)
            return false;

        if (original.cardOwner != GameManager.instance.currentUser) 
            return false;

        // will work after setting up attack placers
        bool b = ValidateAttack(original.currentCardPos, target.currentCardPos);

        if(b) {
            original.Attack(target);
        }

        return b;
    }

    private bool ValidateAttack(CardPlacer original, CardPlacer target) {
        bool b = original.attackPlacers.Contains(target);

        return b;
    }
}
