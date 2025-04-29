using UnityEngine;

public interface ILineOfSight
{
    void Initialize();
    bool IsInSight(Transform target);
}
