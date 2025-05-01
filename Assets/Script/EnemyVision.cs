using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float directRange = 5f;
    [SerializeField] private float directFov = 50f;
    [SerializeField] private float peripheralRange = 7f;
    [SerializeField] private float peripheralFov = 120f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    private List<ITarget> directDetected = new List<ITarget>();
    public List<ITarget> peripheralDetected = new List<ITarget>();

    private Vector3? lastSeenPosition = null;
    public Vector3? LastSeenPosition => lastSeenPosition;

    public bool usePeripheralVision = true;

    public bool HasDirectDetection => directDetected.Count > 0;
    public bool HasPeripheralDetection => peripheralDetected.Count > 0;

    public void UpdateDetection()
    {
        directDetected.Clear();
        peripheralDetected.Clear();
        lastSeenPosition = null;

        FindDirectTargets();
        if (usePeripheralVision)
            FindPeripheralTargets();
    }

    public bool IsInSight(Transform target)
    {
        return directDetected.Exists(t => t.GetTransform == target) ||
               peripheralDetected.Exists(t => t.GetTransform == target);
    }

    private void FindDirectTargets()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, directRange, targetMask);

        foreach (var hitCollider in hitColliders)
        {
            ITarget target = hitCollider.GetComponent<ITarget>();
            if (target != null && CheckAngle(target.GetTransform, directFov) && CheckView(target.GetTransform))
            {
                directDetected.Add(target);
                lastSeenPosition = target.GetTransform.position;
            }
        }
    }

    private void FindPeripheralTargets()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, peripheralRange, targetMask);

        foreach (var hitCollider in hitColliders)
        {
            ITarget target = hitCollider.GetComponent<ITarget>();
            if (target != null && CheckAngle(target.GetTransform, peripheralFov) && CheckView(target.GetTransform))
            {
                peripheralDetected.Add(target);
                lastSeenPosition = target.GetTransform.position;
            }
        }
    }

    private bool CheckAngle(Transform target, float fov)
    {
        Vector3 direction = target.position - transform.position;
        float angle = Vector3.Angle(transform.forward, direction);
        return angle <= fov / 2;
    }

    private bool CheckView(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        return !Physics.Raycast(transform.position, direction.normalized, direction.magnitude, obstacleMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        Gizmos.DrawWireSphere(transform.position, directRange);
        
        DrawFovGizmo(transform.position, transform.forward, directFov, directRange);

        if (usePeripheralVision)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, peripheralRange);
            
            DrawFovGizmo(transform.position, transform.forward, peripheralFov, peripheralRange);
        }
    }

    private void DrawFovGizmo(Vector3 position, Vector3 forward, float fov, float range)
    {
        float angle = fov / 2;
        Vector3 leftBoundary = Quaternion.Euler(0, -angle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, angle, 0) * forward;

        Gizmos.DrawLine(position, position + leftBoundary * range);
        Gizmos.DrawLine(position, position + rightBoundary * range);
    }
}