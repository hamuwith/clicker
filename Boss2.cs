using UnityEngine;
using System.Collections;

public class Boss2 : Boss
{
    protected Vector2 vector2Special1;//初期特殊1スキル座標
    [SerializeField] ParticleDelay particleDelayPreSkill0;
    //初期化
    public override void Start0(Transform spawnTransform, Transform deathPosition)
    {
        base.Start0(spawnTransform, deathPosition);
        vector2Special1 = particleSystemSpecial4.transform.localPosition;
    }
    //固有スキル
    protected override void Special(int skillId)
    {
        if (skillId == 1)
        {
            StartCoroutine(Special1(right));
        }
    }
    //アニメーション開始時
    public override void AnimationStart(int hash, int skillId, float cooltime, Afterimage.Condition condition, float invincibleTime, Animator animator, float cycleOffset)
    {
        if(skillId == 5 && this.animator == animator) ParticlePlayPreSkill();
        base.AnimationStart(hash, skillId, cooltime, condition, invincibleTime, animator, cycleOffset);
    }
    void ParticlePlayPreSkill()
    {
        particleDelayPreSkill0.enumerator = SkillParticlePlay(particleDelayPreSkill0, -1, 0f);
        StartCoroutine(particleDelayPreSkill0.enumerator);
    }
    //スキルエフェクトの停止
    public override void ParticlesStopSkill(int num)
    {
        if (num == 5)
        {
            if (particleDelayPreSkill0.enumerator != null) StopCoroutine(particleDelayPreSkill0.enumerator);
            if (particleDelayPreSkill0.dependence && particleDelayPreSkill0.particleSystem.IsAlive()) particleDelayPreSkill0.particleSystem.Stop();
        }
        if (num < 0 || particleSystemSkills.Length <= num) return;
        foreach (var particleSystem in particleSystemSkills[num].particleDelays)
        {
            if (particleSystem.enumerator != null) StopCoroutine(particleSystem.enumerator);
            if (particleSystem.dependence && particleSystem.particleSystem.IsAlive()) particleSystem.particleSystem.Stop();
        }
    }
    IEnumerator Special1(bool right0)
    {
        vector2 = particleSystemSpecial4.transform.position;
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 8; i++)
        {
            vector2.x -= right0 ? 4f : -4f;
            particleSystemSpecial4.transform.position = vector2;
            particleSystemSpecial4.transform.localScale = right0 ? Vector2.one : inversionVector2;
            particleSystemSpecial4.Play();
            AttackCollisionValueSpecial4.left = !right0;
            GameManager.attackCollisionManagerBoss.SetCollision(AttackCollisionValueSpecial4, vector2, null);
            yield return new WaitForSeconds(0.15f);
        }
        particleSystemSpecial4.transform.localPosition = vector2Special1;
    }
}
