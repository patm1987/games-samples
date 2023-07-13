using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Rotates this node over time (seconds) in degrees per axis.
/// </summary>
public class RotateOverTime : MonoBehaviour
{
    [SerializeField] private Vector3 _angularVelocityDegrees = Vector3.zero;
    public UnityEvent OnAngularVelocityUpdated = new UnityEvent();

    public Vector3 AngularVelocityDegrees
    {
        get => _angularVelocityDegrees;
        set
        {
            _angularVelocityDegrees = value;
            OnAngularVelocityUpdated.Invoke();
        }
    }
    
    void Update()
    {
        transform.Rotate(_angularVelocityDegrees * Time.deltaTime);
    }
}
