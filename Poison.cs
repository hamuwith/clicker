using UnityEngine;
//�ŃX�L��
public class Poison : SingleHit
{
    [SerializeField] AttackCollisionValue attackCollisionValue;
    [SerializeField] ParticleSystem particleSystemEnd;
    //�q�b�g������
    public override void Stop(Transform transform, AttackCollisionValue _attackCollisionValue)
    {
        particleSystemEnd.transform.position = transform.position + (_attackCollisionValue.left ? Vector3.left : Vector3.right) * 6.5f;
        particleSystemEnd.Play();
        //�ǉ��U��
        particleSystem0.gameObject.SetActive(false);
        attackCollisionValue.left = _attackCollisionValue.left;
        GameManager.attackCollisionManager.SetCollision(attackCollisionValue, transform.position, null);
    }
}
