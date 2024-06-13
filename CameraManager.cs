using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    Vector3 vector3;//�ꎞ���p
    public static bool fix;//�J�����Œ蒆
    bool zoomTarget;//�Y�[����
    readonly float offset = 8.35f;//�v���C���[�\���ʒu�I�t�Z�b�g
    readonly float smooth = 15f;//�ؑֈʒu
    readonly float smoothTrans = 14.99f;//�ؑփ|�C���g
    readonly float zoomTime = 1f;//�Y�[���ێ�����
    public void Start0() 
    {
        vector3 = transform.position;
    }
    //�{�X�ɃY�[��
    public void Zoom(Transform _transform)
    {
        zoomTarget = true;
        DOTween.Sequence()
                .Append(transform.DOMoveX(_transform.position.x, 0.5f)
                    .OnComplete(() =>
                    {
                        StartCoroutine(GameManager.gameManager.EnemyDestroy(false));
                    }))
                .AppendInterval(zoomTime)
                .Append(transform.DOMoveX(GameManager.playerManager.transform.position.x + offset, 0.5f)
                    .OnComplete(() =>
                    {
                        zoomTarget = false;
                    }));
    }
    public void Update0()
    {
        if (fix || zoomTarget) return;
        if (GameManager.playerManager.left)
        {
            if (GameManager.playerManager.transform.position.x - transform.position.x > smooth)
            {
                //�J�����̘g����O�ꂻ���ɂȂ�����
                vector3.x = -smoothTrans + GameManager.playerManager.transform.position.x;
            }            
            else
            {
                //�X���[�Y�Ɉړ�
                vector3.x = (GameManager.playerManager.transform.position.x - offset + transform.position.x * 49f) / 50;
            }
        }
        else
        {
            if (transform.position.x - GameManager.playerManager.transform.position.x > smooth)
            {
                vector3.x = smoothTrans + GameManager.playerManager.transform.position.x;
            }
            else
            {
                vector3.x = (GameManager.playerManager.transform.position.x + offset + transform.position.x * 49f) / 50;
            }
        }
        vector3.y = transform.position.y;
        transform.position = vector3;
    }
}
