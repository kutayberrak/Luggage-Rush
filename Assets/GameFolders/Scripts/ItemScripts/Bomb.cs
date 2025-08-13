using GameFolders.Scripts;
using UnityEngine;

public class Bomb : SpecialItem
{
    [SerializeField] private float timeToRemove = 5f;
    public override void OnClickedByPlayer()
    {
        Timer.Instance.RemoveTime(timeToRemove);
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
    }

    protected override void StartCurveMovement()
    {

    }
    protected override void StartClickAnimation()
    {

    }
}