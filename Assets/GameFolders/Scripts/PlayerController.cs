using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private Camera mainCam;
    private float maxDistance = 100f;
    [SerializeField] private float liftOffset = 0.5f;    // Height to lift on hold
    private float holdThreshold = 0.1f;  // Time to distinguish hold vs click

    private ClickableObject currentClickable;
    private MaterialPropertyBlock mpb;
    private Dictionary<ClickableObject, float> originalHeights = new Dictionary<ClickableObject, float>();
    private float hoverStartTime;
    private bool isHolding;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DoRaycast(true);
        }
        else if (Input.GetMouseButton(0))
        {
            DoRaycast(false);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentClickable != null)
            {
                currentClickable.OnClickedByPlayer();
                ClearOutline(currentClickable);
                currentClickable = null;
                isHolding = false;
            }
        }
    }

    private void DoRaycast(bool isDown)
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, maxDistance, clickableLayer))
        {
            var obj = hit.collider.GetComponent<ClickableObject>();
            if (obj != currentClickable)
            {
                if (currentClickable != null)
                    ClearOutline(currentClickable);

                currentClickable = obj;
                hoverStartTime = Time.time;

                SetOutline(currentClickable, 1f);
                isHolding = false;
            }

            if (!isHolding)
            {
                if (Time.time - hoverStartTime >= holdThreshold)
                {
                    StartHolding(currentClickable);
                }
            }
            else
            {
                LiftObject(currentClickable);
            }
        }
        else if (isDown || isHolding)
        {
            if (currentClickable != null)
            {
                ClearOutline(currentClickable);
                currentClickable = null;
                isHolding = false;
            }
        }
    }

    private void StartHolding(ClickableObject obj)
    {
        if (obj == null) return;
        isHolding = true;
        RecordOriginalHeight(obj);
        LiftObject(obj);
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
        ResetObjectPosition(obj);
    }

    private void RecordOriginalHeight(ClickableObject obj)
    {
        if (obj != null && !originalHeights.ContainsKey(obj))
            originalHeights[obj] = obj.transform.position.y;
    }

    private void LiftObject(ClickableObject obj)
    {
        if (obj == null) return;
        Vector3 pos = obj.transform.position;
        float baseY = originalHeights[obj];
        obj.transform.position = new Vector3(pos.x, baseY + liftOffset, pos.z);
    }

    private void ResetObjectPosition(ClickableObject obj)
    {
        if (obj != null && originalHeights.TryGetValue(obj, out float baseY))
        {
            Vector3 pos = obj.transform.position;
            obj.transform.position = new Vector3(pos.x, baseY, pos.z);
            originalHeights.Remove(obj);
        }
    }
}
