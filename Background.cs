using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField] Transform[] transforms;//3�̔w�i
    Transform m_transform;//��̃g�����X�t�H�[��
    [SerializeField] float ratio;//�ړ���
    float changeX;//�ړ����̊X
    int count;//�I�t�Z�b�g����w�i�̐؂�ւ�
    public void Start0(Transform _transform)
    {
        //������
        m_transform = _transform;
        changeX = _transform.position.x;
        count = 9999;
    }
    public void Update0()
    {
        if (m_transform == null) return;
        //�w�i�̃I�t�Z�b�g
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
        //�w�i�̈ړ�
        foreach (var _transform in transforms)
        {
            _transform.position += Vector3.right * (m_transform.position.x - changeX) * ratio;
        }
        changeX = m_transform.position.x;
    }
}
