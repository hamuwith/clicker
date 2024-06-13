using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    Vector3 vector3;//一時利用
    public static bool fix;//カメラ固定中
    bool zoomTarget;//ズーム中
    readonly float offset = 8.35f;//プレイヤー表示位置オフセット
    readonly float smooth = 15f;//切替位置
    readonly float smoothTrans = 14.99f;//切替ポイント
    readonly float zoomTime = 1f;//ズーム維持時間
    public void Start0() 
    {
        vector3 = transform.position;
    }
    //ボスにズーム
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
                //カメラの枠から外れそうになったら
                vector3.x = -smoothTrans + GameManager.playerManager.transform.position.x;
            }            
            else
            {
                //スムーズに移動
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
