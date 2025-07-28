using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayer; // sadece bu layer'� tarar

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Sol t�klama
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, clickableLayer))
        {
            ClickableObject clickable = hitInfo.collider.GetComponent<ClickableObject>();
            if (clickable != null)
            {
                clickable.OnClickedByPlayer();
            }
        }
    }
}
