using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    Vector3 vector3;//�ꎞ���p
    public static bool fix;//�J�����Œ蒆
    public static bool clear;//�N���A�J�����Œ蒆
     static BossName bossNameObj;//���{�X�̖��O�\��
    bool zoomTarget;//�Y�[����
    readonly float offset = 8.35f;//�v���C���[�\���ʒu�I�t�Z�b�g
    readonly float offsetClear = 12f;//�N���A�I�u�W�F�N�g�I�t�Z�b�g
    readonly float smooth = 15f;//�ؑֈʒu
    readonly float smoothTrans = 14.99f;//�ؑփ|�C���g
    readonly float zoomTime = 1.5f;//�Y�[���ێ�����
    readonly float zoomChangeTime = 0.5f;//�Y�[������
    Tweener tweener;
    [SerializeField] Warning warning;//�{�X���o
    [SerializeField] BossName bossName;//�{�X�̖��O�\��
    readonly float bossNameY = 7f;//�{�X�̖��O�̕\������
    [SerializeField] Material zoomMaterial;
    readonly int screenPosition = Shader.PropertyToID("_position");
    readonly int duration = Shader.PropertyToID("_duration");
    readonly int isZoom = Shader.PropertyToID("_isZoom");
    Sequence sequence;
    public void Start0() 
    {
        vector3 = transform.position;
        zoomMaterial.SetInt(isZoom, 0);
    }
    //�_���[�W�\��
    public void ShakeCamera(float value)
    {
        tweener?.Complete();
        tweener = transform.DOShakePosition(value * 0.2f, value * 0.4f, (int)(value * 100f))
            .SetUpdate(true);
    }
    //�����\��
    public void ZoomCamera(float value, in Vector2 worldPoint = default)
    {
        //tweener?.Complete();
        //tweener = Camera.main.DOOrthoSize(Camera.main.orthographicSize - value * 0.1f, value)
        //    .SetLoops(2, LoopType.Yoyo)
        //    .SetUpdate(true);
        sequence?.Complete();
        zoomMaterial.SetInt(isZoom, 1);
        zoomMaterial.SetVector(screenPosition, Camera.main.WorldToViewportPoint(worldPoint));
        sequence = DOTween.Sequence()
                .Append(DOTween.To(() => 0f, (x) =>
                {
                    zoomMaterial.SetFloat(duration, x);
                }, 1f, value * 0.4f))
                .AppendInterval(value * 0.2f)
                .Append(DOTween.To(() => 1f, (x) =>
                {
                    zoomMaterial.SetFloat(duration, x);
                }, 0f, value * 0.4f))
                .OnComplete(() => zoomMaterial.SetInt(isZoom, 0))
                .SetUpdate(true); 
    }
    //�{�X�ɃY�[��
    public void Zoom(Transform _transform)
    {
        GameManager.playerManager.HitStop(zoomTime * 2f + zoomChangeTime * 4f, 0.5f, false);
        zoomTarget = true;
        DOTween.Sequence()
                .Append(transform.DOMoveX(_transform.position.x, zoomChangeTime)
                    .OnComplete(() =>
                    {
                        StartCoroutine(GameManager.gameManager.EnemyDestroy(false));
                    }))
                .AppendInterval(zoomTime)
                .Append(transform.DOMoveX(GameManager.playerManager.transform.position.x + offset, zoomChangeTime))
                .OnComplete(() =>
                {
                    zoomTarget = false;
                });  
    }
    public void CreateWarning()
    {
        Instantiate(warning, transform).SetWarning(zoomTime, zoomChangeTime);
    }
    public void CreateBossName(Transform _transform, in string name, in Color color)
    {
        bossNameObj = Instantiate(bossName, new Vector3(_transform.position.x, bossNameY, 0f), Quaternion.identity);
        bossNameObj.SetText(name, color);
    }
    public void Update0()
    {
        if (fix || zoomTarget || (clear && transform.position.x + offsetClear > GameManager.clearDoor.transform.position.x)) return;
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
    public void ResetCamera()
    {
        vector3.x = GameManager.playerManager.transform.position.x + offset;
        vector3.y = transform.position.y;
        transform.position = vector3;
        Destroy(bossNameObj.gameObject);
        bossNameObj = null;
    }
}
