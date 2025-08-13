using GameFolders.Scripts;
using UnityEngine;

public class Bomb : SpecialItem
{
    [SerializeField] private float timeToRemove = 5f;
    [SerializeField] private float explosionForce = 250f;
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private GameObject explosionParticlePrefab;

    public override void OnClickedByPlayer()
    {
        Timer.Instance.RemoveTime(timeToRemove);

        Timer.Instance.FlashTimerColor(Color.red);

        Explode();

        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
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

        AudioManager.Instance.PlaySFX("etfx_explosion_grenade");

        if (explosionParticlePrefab != null)
        {
            GameObject particle = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            Destroy(particle, ps.main.duration);
        }
    }
}