using System.Collections;
using UnityEngine;

public class ShowHand : MonoBehaviour
{
    Vector2 targetPos;
    Vector2 startPos;

    Coroutine currentCoroutine;

    [SerializeField] Vector2 targetPosOffset;

    private void Start() {
        startPos = transform.localPosition;  
        
        targetPos = transform.localPosition;
        targetPos += targetPosOffset;
        //targetPos.y = -3.54f;
    }

    private void OnMouseEnter() {
        if(currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(DoAnimation(targetPos));
    }

    private void OnMouseExit() {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(DoAnimation(startPos));
    }

    IEnumerator DoAnimation(Vector3 targetPos) {
        StopCoroutine(DoAnimation(targetPos));
        while ((transform.localPosition - targetPos).magnitude > 0.1f) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 0.2f);
            yield return null;
        }
    }
}
