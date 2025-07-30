using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private float maxDistance = 100f;

    private ClickableObject currentHover;
    private MaterialPropertyBlock mpb;
    public Camera mainCam;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            UpdateHover();
        }

        else if (Input.GetMouseButtonUp(0))
        {
            if (currentHover != null)
            {
                currentHover.OnClickedByPlayer();
                ClearOutline(currentHover);
                currentHover = null;
            }
        }
    }

    private void UpdateHover()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, maxDistance, clickableLayer))
        {
            var now = hit.collider.GetComponent<ClickableObject>();
            if (now != currentHover)
            {
                if (currentHover != null)
                    ClearOutline(currentHover);

                currentHover = now;
                if (currentHover != null)
                    SetOutline(currentHover, 1f);
            }
        }
        else
        {
            if (currentHover != null)
            {
                ClearOutline(currentHover);
                currentHover = null;
            }
        }
    }

    private void SetOutline(ClickableObject obj, float value)
    {
        var rend = obj.GetComponent<Renderer>();
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_OutlineMultiplier", value);
        rend.SetPropertyBlock(mpb);
    }

    private void ClearOutline(ClickableObject obj)
    {
        SetOutline(obj, 0f);
    }
}
