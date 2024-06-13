using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class CutIn : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRendererBackground;//スキルカットインの背景
    [SerializeField] TextMeshPro textMesh;//スキル名の表示
    [SerializeField] SpriteRenderer spriteRendererCharacter;//キャラクター表示
    float scaley;//オブジェクトの表示スケールY
    float position;//キャラクター表示の表示ポジション
    Vector2 startScale;//オブジェクトの初期スケール
    Vector2 startPosition;//キャラクター表示の初期ポジション
    float preTimeScale;//前のタイムスケール
    //初期化
    public void Start0()
    {
        scaley = transform.localScale.y;
        startScale = transform.localScale;
        startScale.y = 0f;
        startPosition = spriteRendererCharacter.transform.localPosition;
        startPosition.x -= 10f;
        position = spriteRendererCharacter.transform.localPosition.x;
        Enabled(false);
    }
    //カットインのセット
    public IEnumerator SetCutIn(string skillName, Color color, PlayerManager playerManager)
    {
        if (Time.timeScale == 0f) yield return null;
        preTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        textMesh.text = skillName;
        color.a = 1f;
        spriteRendererBackground.color = color;
        transform.localScale = startScale;
        spriteRendererCharacter.transform.position = startPosition;
        Enabled(true);
        if(playerManager != GameManager.playerManager) spriteRendererCharacter.enabled = false;
        transform.DOScaleY(scaley,0.1f)
            .SetUpdate(true)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                spriteRendererCharacter.transform.DOLocalMoveX(position, 0.5f)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        transform.DOScaleY(0f, 0.1f)
                            .SetEase(Ease.Linear)
                            .SetUpdate(true)
                            .SetDelay(0.3f)
                            .OnComplete(() =>
                            {
                                Enabled(false); 
                                Time.timeScale = preTimeScale;
                            });
                    });
            });
    }
    //表示・非表示
    void Enabled(bool b)
    {
        spriteRendererBackground.enabled = b;
        textMesh.enabled = b;
        spriteRendererCharacter.enabled = b;
    }
}
