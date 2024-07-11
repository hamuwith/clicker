using UnityEngine;

public class BackGroundManager : MonoBehaviour
{
    [SerializeField] Background[] backgrounds;
    public void Start0(Transform _transform)
    {
        foreach (var background in backgrounds)
        {
            background.Start0(_transform);
        }
    }
    public void Update0()
    {
        foreach (var background in backgrounds)
        {
            background.Update0();
        }
    }
}
