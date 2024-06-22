using UnityEngine;
using DG.Tweening;
using TMPro;

public class Warning : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] fastSpriteRenderers;
    [SerializeField] SpriteRenderer[] normalSpriteRenderers;
    [SerializeField] SpriteRenderer[] slowSpriteRenderers;
    [SerializeField] Transform fastTransform;
    [SerializeField] Transform normalTransform;
    [SerializeField] Transform slowTransform;
    [SerializeField] TextMeshPro warningText;
    [SerializeField] GameObject baseGameObject;
    readonly float endPoint = 60f;
    readonly float deisplayTime = 2.5f;
    public void SetWarning(float zoomTime, float zoomChangeTime)
    {
        float f = zoomTime + 2 * zoomChangeTime;
        DOTween.Sequence()
                .Append(fastTransform.DOLocalMoveX(fastTransform.position.x - endPoint, deisplayTime))
                .Join(normalTransform.DOLocalMoveX(normalTransform.position.x - endPoint / 1.2f, deisplayTime))
                .Join(slowTransform.DOLocalMoveX(slowTransform.position.x + endPoint / 1.6f, deisplayTime))
                .Join(warningText.transform.DOLocalMoveX(warningText.transform .position.x + endPoint / 2f, deisplayTime))
                //.Join(warningText.transform.DOScale(1.1f, deisplayTime / 6))
                    //.SetLoops(6, LoopType.Yoyo))
                .SetEase(Ease.Linear);
        DOTween.Sequence()
                .Append(fastSpriteRenderers[0].DOFade(1f, deisplayTime / 2))
                .Join(normalSpriteRenderers[0].DOFade(1f, deisplayTime / 2))
                .Join(slowSpriteRenderers[0].DOFade(1f, deisplayTime / 2))
                .Join(fastSpriteRenderers[1].DOFade(1f, deisplayTime / 2))
                .Join(normalSpriteRenderers[1].DOFade(1f, deisplayTime / 2))
                .Join(slowSpriteRenderers[1].DOFade(1f, deisplayTime / 2))
                .Join(warningText.DOFade(1f, deisplayTime / 2))
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => Destroy(gameObject));
    }
}
