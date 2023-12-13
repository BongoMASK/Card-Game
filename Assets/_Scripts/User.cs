using TMPro;
using UnityEngine;

public class User : MonoBehaviour
{
    public string username;
    
    public bool lockInput { get; set; } = true;

    [Header("Mana Stuff")]

    public int maxMana = 12;

    private int _currentMaxRoundMana = 0;
    public int currentMaxRoundMana {
        get {
            return _currentMaxRoundMana;
        }
        set {
            if (hasRoundMana)
                return;

            _currentMaxRoundMana = value;
            manaCount.text = _currentRoundMana + "/" + _currentMaxRoundMana;

            hasRoundMana = true;
        }
    }

    private int _currentRoundMana = 0;
    public int currentRoundMana {
        get {
            return _currentRoundMana;
        }
        set {
            _currentRoundMana = value;
            manaCount.text = _currentRoundMana + "/" + currentMaxRoundMana;
        }
    }

    public bool hasRoundMana { get; private set; }

    public bool hasPlacedCard { get; set; } = false;

    [Header("Assignables")]

    [SerializeField] TMP_Text manaCount;

    private void Start() {
        currentRoundMana = 0;
        GameManager.instance.OnTurnBegin += RefillMana;
    }

    public void PlayTurn() {
        AudioManager.instance.Play(SoundNames.nextRound);
        Debug.Log(username + "'s Turn");

        lockInput = false;
    }

    void RefillMana() {
        currentRoundMana = currentMaxRoundMana;
        hasRoundMana = false;
        hasPlacedCard = false;
    }

    public void UsedMana(int amount) {
        currentRoundMana -= amount;
        GameManager.instance.FindMoreManaUser();
    }
}
