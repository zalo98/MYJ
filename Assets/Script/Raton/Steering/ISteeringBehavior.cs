using UnityEngine;

public interface ISteeringBehavior
{
    Vector3 CalculateSteering(EnemySteering owner);
}
