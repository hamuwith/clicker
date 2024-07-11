using UnityEngine;

public class Boss3 : Boss2
{
    //ŒÅ—LƒXƒLƒ‹
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
