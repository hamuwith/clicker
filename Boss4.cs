using UnityEngine;

public class Boss4 : Boss
{
    //�ŗL�X�L��
    protected override void Special(int skillId)
    {
    }
    //�X�L���G�t�F�N�g�̃Z�b�g
    public override void ParticlePlaySkill(int num, float cycleOffset)
    {
        if (num == 2 && GetHpRate() >= 0.2f) return;
        foreach (var particleSystem in particleSystemSkills[num].particleDelays)
        {
            particleSystem.enumerator = SkillParticlePlay(particleSystem, num, cycleOffset);
            StartCoroutine(particleSystem.enumerator);
            if (num == 0 && GetHpRate() >= 0.3f) return;
        }
    }
}
