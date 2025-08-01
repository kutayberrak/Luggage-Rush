using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private Camera mainCam;
    private float maxDistance = 100f;
    [SerializeField] private float liftOffset = 0.5f;    // Height to lift on hold
    [SerializeField] private float liftForce = 15f;     // Force to lift object up
    [SerializeField] private float hoverForce = 1f;     // Force to maintain hover
    private float holdThreshold = 0.1f;  // Time to distinguish hold vs click

    private ClickableObject currentClickable;
    private MaterialPropertyBlock mpb;
    private Dictionary<ClickableObject, float> originalHeights = new Dictionary<ClickableObject, float>();
    private Dictionary<ClickableObject, float> targetHeights = new Dictionary<ClickableObject, float>();
    private Dictionary<ClickableObject, bool> isLifting = new Dictionary<ClickableObject, bool>();
    private float hoverStartTime;
    private bool isHolding;

    private float gravityY;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    private void Start()
    {
        gravityY = Mathf.Abs(Physics.gravity.y);
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

        // Apply lift or hover force to held object
        if (isHolding && currentClickable != null)
        {
            ApplyLiftOrHover(currentClickable);
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
                // Object is being held - ApplyHover will handle the physics
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
        SetTargetHeight(obj);

        // Start with lifting phase
        isLifting[obj] = true;
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

        // Clear target height when releasing object
        if (targetHeights.ContainsKey(obj))
        {
            targetHeights.Remove(obj);
        }

        // Clear lifting state
        if (isLifting.ContainsKey(obj))
        {
            isLifting.Remove(obj);
        }

        // DON'T clear original height here - let it persist until object settles
        // Original height will only be updated when object is stationary in RecordOriginalHeight
    }

    private void RecordOriginalHeight(ClickableObject obj)
    {
        if (obj == null) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Check if object is falling or moving vertically (more strict threshold)
        bool isObjectMoving = Mathf.Abs(rb.linearVelocity.y) > 0.05f;

        // If object already has an original height recorded and is still moving, don't update it
        if (originalHeights.ContainsKey(obj) && isObjectMoving)
        {
            // Keep the existing original height, don't change it
            Debug.Log($"Object {obj.name} is moving (velocity: {rb.linearVelocity.y:F3}), keeping original height: {originalHeights[obj]:F2}");
            return;
        }

        // If object is stationary (y velocity near zero) or this is the first time, record/update original height
        if (!isObjectMoving || !originalHeights.ContainsKey(obj))
        {
            float newOriginalHeight = obj.transform.position.y;
            Debug.Log($"Recording original height for {obj.name}: {newOriginalHeight:F2} (velocity: {rb.linearVelocity.y:F3})");
            originalHeights[obj] = newOriginalHeight;
        }
    }

    private void SetTargetHeight(ClickableObject obj)
    {
        if (obj != null && originalHeights.ContainsKey(obj))
        {
            float baseY = originalHeights[obj];
            targetHeights[obj] = baseY + liftOffset;
        }
    }

    private void ApplyLiftOrHover(ClickableObject obj)
    {
        if (obj == null) return;

        // Check if object is in lifting phase or hover phase
        if (isLifting.ContainsKey(obj) && isLifting[obj])
        {
            ApplyLift(obj);
        }
        else
        {
            ApplyHover(obj);
        }
    }

    private void ApplyLift(ClickableObject obj)
    {
        if (obj == null) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (!targetHeights.ContainsKey(obj)) return;

        float targetY = targetHeights[obj];
        float currentY = obj.transform.position.y;
        float heightDifference = targetY - currentY;

        // Apply constant velocity upward movement
        if (heightDifference > 0.05f)
        {
            // Set constant upward velocity instead of force
            float constantLiftSpeed = 2f; // Constant speed in units per second
            Vector3 currentVelocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(currentVelocity.x, constantLiftSpeed, currentVelocity.z);
        }
        else
        {
            // Object reached target height, switch to hover mode
            isLifting[obj] = false;
        }
    }

    private void ApplyHover(ClickableObject obj)
    {
        if (obj == null) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Apply hover force like your example - simple and direct
        if ((Mathf.Abs(rb.linearVelocity.y) < 0.1f) || rb.linearVelocity.y < 0)
        {

            float gravityForce = (rb.mass * gravityY) / hoverForce;
            rb.AddForce(Vector3.up * gravityForce, ForceMode.Force);
        }
    }
}
