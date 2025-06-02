using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFEntity : MonoBehaviour
{
    public PFNodes endNode;
    public float reachDistance;
    public List<PFNodes> path;
    public float speed;

    public List<PFNodes> SetPath { set { path = value; } }

    // Update is called once per frame
    void Update()
    {
        if (path.Count > 0)
        {
            Vector3 dir = path[0].transform.position - transform.position;
            transform.position += dir.normalized * speed * Time.deltaTime;
            if (dir.sqrMagnitude < 0.2f)
                path.RemoveAt(0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, reachDistance);
    }
}
