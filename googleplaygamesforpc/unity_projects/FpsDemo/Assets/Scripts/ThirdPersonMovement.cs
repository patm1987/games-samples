using UnityEngine;

/// <summary>
/// Implements a very simple mouselook driven movement system
/// </summary>
public class ThirdPersonMovement : MonoBehaviour
{
    [SerializeField] [Tooltip("Character controller used to move")]
    private CharacterController _characterController;

    [SerializeField] private float _speed = 1f;

    [SerializeField] [Tooltip("Camera whose perspective is used to base movement")]
    private Camera _camera;

    [SerializeField] [Tooltip("Axis used for moving forward and back")]
    private string _forwardAxisName = "Vertical";

    [SerializeField] [Tooltip("Axis used for moving horizontally/strafing")]
    private string _horizontalAxisName = "Horizontal";

    private void Reset()
    {
        _characterController = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // var velocity = new Vector3(0, _rigidbody.velocity.y, 0);
        var velocity = Vector3.zero;

        // Get our movement axis. Note that Unity's normalize
        // appears to check for zero.
        var forward = _camera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        velocity +=
            forward * Input.GetAxis(_forwardAxisName) * _speed * Time.deltaTime;

        var right = _camera.transform.right;
        right.y = 0;
        right.Normalize();
        velocity +=
            right * Input.GetAxis(_horizontalAxisName) * _speed * Time.deltaTime;

        _characterController.Move(velocity);
        _characterController.Move(Physics.gravity * Time.deltaTime);
    }
}