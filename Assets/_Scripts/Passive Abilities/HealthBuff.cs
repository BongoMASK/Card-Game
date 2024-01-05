public class HealthBuff : AppliedPassive
{
    public BaseCard card;

    public int buff = 1;

    public void ApplyPassive(BaseCard card) {
        if (buff > card.healthBuff)
            card.healthBuff = buff;
        else
            Destroy(this);
    }
}
