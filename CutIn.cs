using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class CutIn : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRendererBackground;//�X�L���J�b�g�C���̔w�i
    [SerializeField] TextMeshPro textMesh;//�X�L�����̕\��
    [SerializeField] SpriteRenderer spriteRendererCharacter;//�L�����N�^�[�\��
    float scaley;//�I�u�W�F�N�g�̕\���X�P�[��Y
    float position;//�L�����N�^�[�\���̕\���|�W�V����
    Vector2 startScale;//�I�u�W�F�N�g�̏����X�P�[��
    Vector2 startPosition;//�L�����N�^�[�\���̏����|�W�V����
    float preTimeScale;//�O�̃^�C���X�P�[��
    //������
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
    //�J�b�g�C���̃Z�b�g
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
    //�\���E��\��
    void Enabled(bool b)
    {
        spriteRendererBackground.enabled = b;
        textMesh.enabled = b;
        spriteRendererCharacter.enabled = b;
    }
}
