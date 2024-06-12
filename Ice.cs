using UnityEngine;
using System.Collections;

public class Ice : MonoBehaviour
{
    [SerializeField] ParticleSystem particleSystem0;
    [SerializeField] SpriteRenderer spriteRenderer;
    Vector2 vector2;
    readonly float offset = 4.77f;
    readonly float offsetPosition = 2f;
    //�G�t�F�N�g�̃T�C�Y�Z�b�g
    public void SetSpriteSize(Transform _transform, bool left)
    {
        if (_transform != null)
        {
            //�Ώۂ�����Ƃ�
            vector2.x = Mathf.Abs(_transform.position.x - GameManager.playerManagerSub.transform.position.x) - offset;
            vector2.y = spriteRenderer.size.y;
            spriteRenderer.size = vector2;
            vector2.x = _transform.position.x + (left ? offsetPosition : -offsetPosition);
            vector2.y = transform.position.y;
            transform.position = vector2;
            if (!particleSystem0.gameObject.activeSelf) particleSystem0.gameObject.SetActive(true);
        }
        else
        {
            //�Ώۂ����Ȃ��Ƃ�
            vector2.x = 0f;
            vector2.y = spriteRenderer.size.y;
            spriteRenderer.size = vector2;
            if (particleSystem0.gameObject.activeSelf) particleSystem0.gameObject.SetActive(false);
        }
    }
    //�X�L������
    public void Play()
    {
        spriteRenderer.enabled = true; 
        vector2.x = 0f;
        vector2.y = spriteRenderer.size.y;
        spriteRenderer.size = vector2;
    }
    //�X�L����~
    public void Stop()
    {
        spriteRenderer.enabled = false;
        if (particleSystem0.gameObject.activeSelf) particleSystem0.gameObject.SetActive(false);
    }
}
