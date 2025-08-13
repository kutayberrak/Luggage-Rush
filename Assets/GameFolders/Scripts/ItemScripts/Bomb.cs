using GameFolders.Scripts;
using UnityEngine;

public class Bomb : SpecialItem
{
    [SerializeField] private float timeToRemove = 5f;
    [SerializeField] private float explosionForce = 250f;
    [SerializeField] private float explosionRadius = 10f;

    public override void OnClickedByPlayer()
    {
        Timer.Instance.RemoveTime(timeToRemove);

        Explode();

        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
    }

    protected override void StartCurveMovement()
    {
    }

    protected override void StartClickAnimation()
    {
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.attachedRigidbody;
            if (rb != null && rb != this.GetComponent<Rigidbody>())
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }
}