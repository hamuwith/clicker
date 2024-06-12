using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField] Transform[] transforms;//3つの背景
    Transform m_transform;//基準のトランスフォーム
    [SerializeField] float ratio;//移動率
    float changeX;//移動率の基準X
    int count;//オフセットする背景の切り替え
    public void Start0(Transform _transform)
    {
        //初期化
        m_transform = _transform;
        changeX = _transform.position.x;
        count = 9999;
    }
    public void Update0()
    {
        if (m_transform == null) return;
        //背景のオフセット
        if ((transforms[count % transforms.Length].position.x + transforms[(count + 1) % transforms.Length].position.x) / 2 < m_transform.position.x)
        {
            float offset = transforms[(count + 1) % transforms.Length].position.x - transforms[count % transforms.Length].position.x;
            transforms[(count + 2) % transforms.Length].position = Vector3.right * offset + transforms[(count + 1) % transforms.Length].position;
            count += 1;
        }
        else if ((transforms[count % transforms.Length].position.x + transforms[(count + 2) % transforms.Length].position.x) / 2 >= m_transform.position.x)
        {
            float offset = transforms[(count + 2) % transforms.Length].position.x - transforms[count % transforms.Length].position.x;
            transforms[(count + 1) % transforms.Length].position = Vector3.right * offset + transforms[(count + 2) % transforms.Length].position;
            count -= 1;
        }
        //背景の移動
        foreach (var _transform in transforms)
        {
            _transform.position += Vector3.right * (m_transform.position.x - changeX) * ratio;
        }
        changeX = m_transform.position.x;
    }
}
