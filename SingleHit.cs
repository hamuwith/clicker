using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SingleHit : MonoBehaviour
{
    //サブプレイヤーの通常攻撃
    public int nextNum { get; set; }
    [SerializeField] protected ParticleSystem particleSystem0;//エフェクト
    //初期化
    public void Init(int nextNum)
    {
        this.nextNum = nextNum;
    }
    //セット
    public bool Set(Vector2 vector2, bool left)
    {
        transform.position = vector2;
        particleSystem0.transform.localScale = left ? GameManager.playerManager.hanten: Vector2.one;        
        return Set();
    }
    //セット
    public virtual bool Set()
    {
        if (particleSystem0.gameObject.activeSelf) return false;
        particleSystem0.gameObject.SetActive(true);
        StartCoroutine(ParticleWorking());
        return true;
    }
    //ヒット

    public virtual void Stop(Transform transform, AttackCollisionValue _attackCollisionValue)
    {
        GameManager.playerManagerSub.FireParticlePlay(transform);
        particleSystem0.gameObject.SetActive(false);
    }
    //最終地点に到達
    IEnumerator ParticleWorking()
    { 
        yield return new WaitWhile(() => particleSystem0.IsAlive(true));
        if(particleSystem0.gameObject.activeSelf) particleSystem0.gameObject.SetActive(false);
    }
}
