using UnityEngine;

namespace GameFolders.Scripts
{

    public class ConveyorBeltController : MonoBehaviour
    {
        [SerializeField] public float conveyorSpeed = 1f;
        [SerializeField] private Rigidbody conveyorRigidbody;

        [Header("Material Animation")]
        [SerializeField] private Renderer conveyorRenderer;
        [SerializeField] private float textureScrollSpeed = 1f;

        private string texturePropertyName = "_BaseMap";
        private Material conveyorMaterial;
        private float textureOffset = 0f;

        void Start()
        {
            conveyorMaterial = conveyorRenderer.material;
        }

        void FixedUpdate()
        {
            Vector3 pos = conveyorRigidbody.position;
            conveyorRigidbody.position -= transform.right * Time.fixedDeltaTime * conveyorSpeed; // Conveyor hareketi
            conveyorRigidbody.MovePosition(pos);

            AnimateConveyorTexture();
        }

        private void AnimateConveyorTexture()
        {
            textureOffset += Time.fixedDeltaTime * textureScrollSpeed * conveyorSpeed;

            Vector2 offset = new Vector2(0, textureOffset);
            conveyorMaterial.SetTextureOffset(texturePropertyName, offset);
        }

    }
}
