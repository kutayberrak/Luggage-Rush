using System.Collections;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public static DebugScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void SpawnBlock()
    {
        GameObject block = ObjectPoolManager.Instance.GetRandomObjectFromPool(new Vector3(0, 0, 0), transform.rotation);
        StartCoroutine(DeactivateObject(block));
    }

    private IEnumerator DeactivateObject(GameObject block)
    {
        yield return new WaitForSeconds(3f);
        ObjectPoolManager.Instance.ReturnObjectToPool(block);
    }
}
