using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAI : MonoBehaviour
{
    [SerializeField] private float range = 5f;
    [SerializeField] private float fov = 50f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float downwardAngle = 15f;

    private List<ITarget> targetDetected = new List<ITarget>();
    private Vector3 lastSeenPosition = Vector3.zero;
    private bool playerDetected = false;
    
    public event Action<Vector3> OnPlayerSeen;

    private void Update()
    {
        FindTarget();
        RotateCamera();
    
        if (targetDetected.Count >= 1)
        {
            Debug.Log("Player detected by camera");
            OnPlayerSeen?.Invoke(lastSeenPosition);
        }
    }

    private void FindTarget()
    {
        targetDetected.Clear();
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range, targetMask);

        foreach (var hitCollider in hitColliders)
        {
            ITarget target = hitCollider.GetComponent<ITarget>();

            if (target != null && CheckAngle(target.GetTransform) && CheckView(target.GetTransform))
            {
                targetDetected.Add(target);
                lastSeenPosition = target.GetTransform.position;
                Debug.Log("Player last seen at: " + lastSeenPosition);
            }
        }
    }

    private void RotateCamera()
    {
        transform.rotation = Quaternion.Euler(downwardAngle, transform.rotation.eulerAngles.y, 0);
    }

    private bool CheckAngle(Transform target)
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, fov / 2, 0) * transform.forward * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -fov / 2, 0) * transform.forward * range);
    }
}