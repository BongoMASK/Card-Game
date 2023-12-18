using UnityEngine;

public class Draggable : MonoBehaviour
{
    float threshold = 1;
    bool isBeingDragged = false;

    Vector3 mousePos;

    [SerializeField] BaseCard card;

    void CheckIfNearCardPlacer() {
        float min = 99999;
        CardPlacer bestCp = null;

        foreach (CardPlacer cp in GameData.instance.activeCardPlacers) {
            float dist = (transform.position - cp.transform.position).magnitude;
            if (dist < threshold) {
                if (dist < min) {
                    bestCp = cp;
                    min = dist;
                }
            }
        }

        if(bestCp == null || bestCp == card.currentCardPos) {
            card.MoveTo(Vector3.zero);
            return;
        }

        //card.ValidateMovement(bestCp);

        if (CardFunctions.instance.ValidateMovement(card, bestCp, card.cardOwner.player)) {

            PlayerMove move = new PlayerMove(card.cardID, bestCp.id, MoveType.Move);
            GameController.instance.SendMoveToMasterClient(move, false);
        }

        //CardValidator.instance.movementSystem.ValidateMovement(card.currentCardPos, bestCp, card); 
    }

    #region Drag and Drop

    Vector3 GetMousePos() {
        return Camera.main.WorldToScreenPoint(transform.position);
    }

    private void OnMouseEnter() {
        if (!isBeingDragged)
            AudioManager.instance.Play(SoundNames.cardHover);
    }

    private void OnMouseDown() {
        mousePos = Input.mousePosition - GetMousePos();
    }

    private void OnMouseDrag() {
        if (card.cardOwner.lockInput || card.hasBeenMoved)
            return;

        if(!isBeingDragged)
            AudioManager.instance.Play("card pickup");

        isBeingDragged = true;
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - mousePos);
    }
    
    private void OnMouseUp() {
        isBeingDragged = false;
        CheckIfNearCardPlacer();

        if (GameData.instance.currentSelectedCard == card) {
            GameData.instance.currentSelectedCard = null;
            return;
        }

        GameData.instance.currentSelectedCard = card;
    }

    #endregion
}
