using UnityEngine;
using System.Collections;

public class Thunder : MonoBehaviour
{
    [SerializeField] ParticleSystem particleSystem0;//�G�t�F�N�g
    [SerializeField] AttackCollisionValue attackCollisionValue;//�p�����[�^
    static public bool flag;//�U�����ɂ���x�̂�
    Vector2 vector2;
    //�Z�b�g
    public void Set(Transform transform)
    {
        if (!flag) return;
        StartCoroutine(SetDelay(transform));
        flag = false;
    }
    //�U�����ɂ��Ȃ�ǂł�
    public void SetAllOk(Transform transform)
    {
        StartCoroutine(SetDelay(transform));
    }
    //�Z�b�g
    IEnumerator SetDelay(Transform transform)
    {
        yield return new WaitForSeconds(0.2f);
        vector2 = transform.position;
        vector2.y = this.transform.position.y;
        this.transform.position = vector2;
        particleSystem0.Play();
        GameManager.attackCollisionManager.SetCollision(attackCollisionValue, vector2, null);
    }
}
