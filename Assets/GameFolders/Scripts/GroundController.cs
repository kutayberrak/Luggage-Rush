using UnityEngine;

namespace GameFolders.Scripts
{
    public class GroundController : MonoBehaviour
    {
        [SerializeField] private Transform[] backgrounds;
        [SerializeField] private float backgroundWidth = 20f;
        [SerializeField] private float scrollSpeed = 2f;

        private void Update()
        {
            foreach (var bg in backgrounds)
            {
                bg.position += Vector3.right * (scrollSpeed * Time.deltaTime);
                if (bg.position.x >= backgroundWidth)
                {
                    Transform leftmost = GetLeftmostBackground();
                    bg.position = new Vector3(leftmost.position.x - backgroundWidth, bg.position.y, bg.position.z);
                }
            }
        }

        private Transform GetLeftmostBackground()
        {
            Transform leftmost = backgrounds[0];
            foreach (var bg in backgrounds)
            {
                if (bg.position.x < leftmost.position.x)
                    leftmost = bg;
            }
            return leftmost;
        }
    }
}
