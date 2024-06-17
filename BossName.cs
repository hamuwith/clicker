using UnityEngine;
using TMPro;
using DG.Tweening;

public class BossName : MonoBehaviour
{
    [SerializeField] TextMeshPro bossText;
    [SerializeField] SpriteRenderer bossSpriteRenderer;
    public void SetText(in string bossName, in Color color)
    {
        bossText.text = bossName;
        bossSpriteRenderer.color = color;
        transform.DOShakePosition(3f ,0.3f, 2);
    }
}
