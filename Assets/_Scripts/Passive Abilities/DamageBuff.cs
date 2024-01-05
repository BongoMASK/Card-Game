public class DamageBuff : AppliedPassive
{
    public BaseCard card;

    public int buff = 1;

    public void ApplyPassive(BaseCard card) {
        if (buff > card.damageBuff)
            card.damageBuff = buff;
        else
            Destroy(this);
    }
} 