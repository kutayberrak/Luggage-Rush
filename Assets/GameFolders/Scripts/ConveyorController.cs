using UnityEngine;

namespace GameFolders.Scripts
{

    public class ConveyorBeltController : MonoBehaviour
    {
        [SerializeField] private float conveyorSpeed = 1f;
        [SerializeField] private Rigidbody conveyorRigidbody;

        void FixedUpdate()
        {
            Vector3 pos = conveyorRigidbody.position;
            conveyorRigidbody.position -= transform.right * Time.fixedDeltaTime * conveyorSpeed; // Conveyor hareketi
            conveyorRigidbody.MovePosition(pos);
        }
    }
}
