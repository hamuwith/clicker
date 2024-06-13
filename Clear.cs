using UnityEngine;
using TMPro;
using DG.Tweening;

public class Clear : MonoBehaviour
{
    //アクティブ時処理
    void OnEnable()
    {
        TextMeshPro tmpro = GetComponent<TextMeshPro>();
        DOTweenTMPAnimator tmproAnimator = new DOTweenTMPAnimator(tmpro);
        for (int i = 0; i < tmproAnimator.textInfo.characterCount; ++i)
        {
            tmproAnimator.DORotateChar(i, Vector3.up * 90, 0);
            DOTween.Sequence()
                .Append(tmproAnimator.DORotateChar(i, Vector3.zero, 0.4f))
                .AppendInterval(1f)
                .Append(tmproAnimator.DOColorChar(i, Color.white, 0.2f).SetLoops(2, LoopType.Yoyo))
                .SetDelay(0.07f * i);
        }
    }
}
