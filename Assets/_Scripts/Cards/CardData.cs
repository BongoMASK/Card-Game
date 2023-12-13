using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardData : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text sacrificeText;
    [SerializeField] TMP_Text manaText;
    [SerializeField] TMP_Text descriptionText;

    public void SetUpCardData(BaseCard card) {
        if(card == null) {
            image.color = new Color(1, 1, 1, 0);
            healthText.text = "Click on a card \nto select it.";
            
            damageText.text = "";
            sacrificeText.text = "";
            manaText.text = "";
            descriptionText.text = "";

            return;
        }

        image.color = new Color(1, 1, 1, 1);

        image.sprite = card.cardStats.sprite;

        healthText.text = "Health: " + card.defaultHP + " + " + card.healthBuff;
        damageText.text = "Damage: " + card.cardStats.damage + " + " + card.damageBuff;
        sacrificeText.text = "Sacrifice: " + card.cardStats.sacrificeCost;
        manaText.text = "Cost: " + card.cardStats.manaCost;
        descriptionText.text = card.cardStats.passiveDescription;
    }
}
