using GameFolders.Scripts;
using UnityEngine;

public class Hourglass : SpecialItem
{
    [SerializeField] private float timeToAdd = 5f;
    public override void OnClickedByPlayer()
    {
        Timer.Instance.AddTime(timeToAdd);
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
    }

    protected override void StartCurveMovement()
    {

    }
    protected override void StartClickAnimation()
    {

    }
}
