using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    Vector3 vector3;
    public static bool fix;
    //Matrix4x4 mat;
    readonly float offset = 8.35f;
    readonly float y = 1.5f;
    readonly float smooth = 15f;
    readonly float smoothTrans = 14.99f;
    readonly float zoomTime = 0.6f;
    public void Start0() 
    {
        vector3 = transform.position;
    }
    //未使用
    public void Zoom()
    {
        Camera.main.DOOrthoSize(8f, zoomTime)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutExpo);
    }
    public void Update0()
    {
        if (fix) return;
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
        vector3.y = (y + transform.position.y * 49f) / 50;
        transform.position = vector3;
    }
}
