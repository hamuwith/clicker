using UnityEngine;

public class Boss3 : Boss2
{
    //�ŗL�X�L��
    protected override void Special(int skillId)
    {
    }
    protected override void StartSub()
    {
        StartCoroutine(AfterimageInit(false));
        transform.position += Vector3.right * 24f;
        GameManager.bossSub = this;
    }
}
