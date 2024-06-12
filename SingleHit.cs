using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SingleHit : MonoBehaviour
{
    //�T�u�v���C���[�̒ʏ�U��
    public int nextNum { get; set; }
    [SerializeField] protected ParticleSystem particleSystem0;//�G�t�F�N�g
    //������
    public void Init(int nextNum)
    {
        this.nextNum = nextNum;
    }
    //�Z�b�g
    public bool Set(Vector2 vector2, bool left)
    {
        transform.position = vector2;
        particleSystem0.transform.localScale = left ? GameManager.playerManager.hanten: Vector2.one;        
        return Set();
    }
    //�Z�b�g
    public virtual bool Set()
    {
        if (particleSystem0.gameObject.activeSelf) return false;
        particleSystem0.gameObject.SetActive(true);
        StartCoroutine(ParticleWorking());
        return true;
    }
    //�q�b�g

    public virtual void Stop(Transform transform, AttackCollisionValue _attackCollisionValue)
    {
        GameManager.playerManagerSub.FireParticlePlay(transform);
        particleSystem0.gameObject.SetActive(false);
    }
    //�ŏI�n�_�ɓ��B
    IEnumerator ParticleWorking()
    { 
        yield return new WaitWhile(() => particleSystem0.IsAlive(true));
        if(particleSystem0.gameObject.activeSelf) particleSystem0.gameObject.SetActive(false);
    }
}
