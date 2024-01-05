using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardPosition {
    Left,
    Mid,
    Right,
}

public class BaseCard : MonoBehaviourPun, IDamageable {

    #region Card Stats
    // Card Stats ---------------------------------------------------------------------------------------------------------------

    [SerializeField] CardStats _cardStats;

    public CardStats cardStats {
        get => _cardStats;

        set {
            _cardStats = value;
            defaultHP = _cardStats.maxHP;
        }
    }

    public int defaultHP { get; private set; } = 0;
    public int healthBuff { get; set; } = 0;
    public int damageBuff { get; set; } = 0;

    public int passiveRange = 1;

    public int effectiveDamage => damageBuff + cardStats.damage;
    public int effectiveHealth => passiveHandler.extraHealth + defaultHP;

    public bool hasBeenMoved = false;
    public bool hasAttacked = false;

    private CardPlacer _currentCardPos;

    public CardPlacer currentCardPos {
        get {
            return _currentCardPos;
        }
        set {
            if (value != null)
                _currentCardPos = value;

            transform.parent = _currentCardPos.transform;
            MoveTo(Vector3.zero);
        }
    }

    public PlayerData cardOwner;

    public static int id = -1;
    public int cardID;

    #endregion

    public delegate void EventFuncs();
    public EventFuncs OnTakeDamage;
    public EventFuncs OnDie;

    [SerializeField] PassiveHandler passiveHandler;
    [SerializeField] SpriteRenderer border;
    [SerializeField] Animator animator;

    private void Start() {
        defaultHP = cardStats.maxHP;
    }

    private void OnEnable() {
        OnDie += Die;
        OnDie += CardFunctions.instance.CheckForAllCardBuffs;
        //GameManager.instance.OnTurnBegin += HasBeenMovedOverride;
    }

    private void OnDisable() {
        OnDie -= Die;
        OnDie -= CardFunctions.instance.CheckForAllCardBuffs;
        //GameManager.instance.OnTurnBegin -= HasBeenMovedOverride;
    }

    public virtual void ApplyPassive(CardPlacer target) { }

    public static BaseCard FindCard(int id) {
        if (id < 0)
            return null;
        
        foreach (var item in FindObjectsOfType<BaseCard>()) {
            if (item.cardID == id) return item;
        }

        Debug.LogError("Card with id: " + id + " does not exist or has been destroyed");
        return null;
    }

    #region Damage and Attack (Health Stuff)

    public virtual bool IsWithinAttackRange() {
        return true;
    }

    public void TakeDamage(int damage) {
        healthBuff -= damage;

        if (healthBuff < 0) {
            defaultHP += healthBuff;
            healthBuff = 0;
        }

        OnTakeDamage?.Invoke();

        if (defaultHP <= 0)
            OnDie?.Invoke();
    }

    public void Die() {
        animator.SetTrigger("fade");
        AudioManager.instance.Play(SoundNames.burn);

        currentCardPos.OnCardRemoved(this);

        if(GameData.instance.activeCards.Contains(this)) {
            GameData.instance.activeCards.Remove(this);
        }

        Destroy(gameObject, 1);
        Invoke(nameof(CheckAllCardsBuffs), 1.1f);
    }

    public void Attack(BaseCard attackedCard) {
        passiveHandler.Attack(this, attackedCard);
    }

    public bool ValidateAttack(BaseCard attackedCard, Player sender) {
        return passiveHandler.ValidateAttack(this, attackedCard, sender);
    }

    #endregion

    #region Card UI stuff

    public void OnSelected() {
        border.color = Color.white;
        border.enabled = true;

        AudioManager.instance.Play(SoundNames.cardPress);

        if (currentCardPos.pos == Vector2.zero)
            return;

        ShowMovementPowers();
        ShowAttackPowers();
    }

    public void OnSelected(Color32 color) {
        border.enabled = true;
        border.color = color;
    }

    public void OnDeselected() {
        border.enabled = false;

        if (currentCardPos.pos == Vector2.zero)
            return;

        HideMovementPowers();
        HideAttackPowers();
    }

    protected virtual void ShowMovementPowers() {
        List<CardPlacer> cardPlacerList = GameData.instance.allCardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {
            if (cp.owner != cardOwner)
                continue;

            if (cp == currentCardPos)
                continue;

            if (CheckDifference(currentCardPos.pos, cp.pos) <= 1) {
                if (cp.currentCard == null)
                    cp.OnSelected(Color.white);
            }
        }
    }

    protected virtual void ShowAttackPowers() {
        foreach (CardPlacer cp in currentCardPos.attackPlacers) {
            if (cp.currentCard == null)
                continue;

            cp.currentCard.OnSelected(Color.red);
        }
    }

    protected virtual void HideMovementPowers() {
        List<CardPlacer> cardPlacerList = GameData.instance.allCardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {
            if (cp.owner != cardOwner)
                continue;

            if (cp == currentCardPos)
                continue;

            if (CheckDifference(currentCardPos.pos, cp.pos) <= 1) {
                cp.OnDeselected();
            }
        }
    }

    protected virtual void HideAttackPowers() {
        foreach (CardPlacer cp in currentCardPos.attackPlacers) {
            if (cp.currentCard == null)
                continue;

            cp.currentCard.border.enabled = false;
        }
    }

    #endregion

    #region Movement Stuff

    protected int CheckDifference(Vector2 v1, Vector2 v2) {
        Vector2 v = v2 - v1;
        return (int)v.magnitude;
    }

    public void MoveCard(CardPlacer target, bool canMoveCard, bool moved = true) {
        passiveHandler.MoveCard(this, target, canMoveCard);
    }

    public bool ValidateMovement(CardPlacer target, Player sender) {
        return passiveHandler.ValidateMovement(this, target, sender);
    }

    public void HasBeenMovedOverride() {
        if (currentCardPos is DeckCardPlacer) {
            hasBeenMoved = true;
            return;
        }

        hasBeenMoved = false;
        hasAttacked = false;
    }

    public bool IsWithinRange(CardPlacer target) {
        return CheckDifference(currentCardPos.pos, target.pos) <= passiveRange;
    }

    public void MoveTo(Vector3 targetPos) {
        float time = 0.4f;

        transform.DOLocalMove(targetPos, time).SetEase(Ease.OutSine);
    }

    #endregion

    #region Buffs, Debuffs

    public void ApplyHealthBuff(int buff) {
        if (buff > healthBuff)
            healthBuff = buff;
    }

    public void ApplyDamageBuff(int buff) {
        if (buff > damageBuff)
            damageBuff = buff;
    }

    void CheckAllCardsBuffs() {
        foreach (BaseCard card in GameData.instance.activeCards) {
            card.CheckForBuffs();
        }
    }

    public void CheckForBuffs() {
        // Check if buffs should be applied
        if (currentCardPos.pos == Vector2.zero)
            return;

        foreach (BaseCard card in GameData.instance.activeCards) {
            card.ApplyPassive(currentCardPos);
        }
    }

    public void RemoveAllBuffs() {
        damageBuff = 0;
        healthBuff = 0;
    }

    #endregion
}