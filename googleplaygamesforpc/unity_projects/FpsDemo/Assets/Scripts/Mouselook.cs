using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A sample simple mouselook implementation that uses Unity's built-in mechanisms when possible and falls back to the
/// native Android interface when running on an Android device.
/// </summary>
public class Mouselook : MonoBehaviour
{
    private Vector3 _eulerAngleDegrees;
    private bool _inputCaptured;

    /// <summary>
    /// By default, Unity scales "Mouse X" and "Mouse Y" by 0.1. By choosing that as the default here, we should match
    /// Unity's behaviour out of the box - but we won't pick up an updated scale if it's changed in the properties.
    /// </summary>
    [SerializeField] [Tooltip("Additional modifier to sync Android with the system preferences")]
    private float _androidSensitivityScale = 0.1f;

    public float AndroidSensitivityScale => _androidSensitivityScale;

    [SerializeField] private float _mouseSensitivity = 0.1f;
    public UnityEvent OnMouseSensitivityChanged = new UnityEvent();

    public float MouseSensitivity
    {
        get => _mouseSensitivity;
        set
        {
            _mouseSensitivity = value;
            OnMouseSensitivityChanged.Invoke();
        }
    }

    public UnityEvent OnInputCaptureChanged = new UnityEvent();

    public bool InputCaptured
    {
        get => _inputCaptured;
        set
        {
            _inputCaptured = value;
            OnInputCaptureChanged.Invoke();
        }
    }

    public void CaptureInput()
    {
        if (CanUseNativeCapture())
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            MouseCapture.EnableMouseCapture(HandleMouseCaptureFinished);
        }

        InputCaptured = true;
    }

    private void HandleMouseCaptureFinished(bool success, string message)
    {
        Debug.Log($"Mouse capture finished with {success} :: {message}");
    }

    public void ReleaseInput()
    {
        if (CanUseNativeCapture())
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            MouseCapture.DisableMouseCapture();
        }

        InputCaptured = false;
    }

    private void Start()
    {
        if (!CanUseNativeCapture())
        {
            MouseCapture.MouseCaptureChangedCallback += HandleMouseCaptureChanged;
        }
    }

    private void OnDestroy()
    {
        MouseCapture.MouseCaptureChangedCallback -= HandleMouseCaptureChanged;
    }

    private void HandleMouseCaptureChanged(bool mouseCaptured)
    {
        Debug.Log($"Mouse capture changed to {mouseCaptured}");
    }

    void Update()
    {
        if (InputCaptured)
        {
            Vector3 rotationEuler = Vector3.zero;

            // WARNING: I just copied this into TrackMouseDelta.cs. Update if you muck around in here.
            if (CanUseNativeCapture())
            {
                rotationEuler.x = Input.GetAxis("Mouse Y");
                rotationEuler.y = Input.GetAxis("Mouse X");
            }
            else
            {
                var delta = MouseCapture.GetMouseDelta() * _androidSensitivityScale;
                rotationEuler.x = delta.y;
                rotationEuler.y = delta.x;
            }

            _eulerAngleDegrees.x = (_eulerAngleDegrees.x - rotationEuler.x * _mouseSensitivity) % 360;
            _eulerAngleDegrees.y = (_eulerAngleDegrees.y + rotationEuler.y * _mouseSensitivity) % 360;
            transform.localEulerAngles = _eulerAngleDegrees;
        }
    }

    /// <summary>
    /// This lets us figure out if we need the native Android implementation (when the platform is Android and we're not
    /// in the Editor).
    /// </summary>
    /// <returns>true when not running on an Android device</returns>
    public bool CanUseNativeCapture()
    {
        return Application.isEditor || Application.platform != RuntimePlatform.Android;
    }
}