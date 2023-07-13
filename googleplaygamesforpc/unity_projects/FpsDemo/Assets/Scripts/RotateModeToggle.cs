using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sample script that handles moving between two mouse modes. In this case, an auto-rotating camera when the mouse is
/// not captured and an orbit "mouselook" camera when it is.
/// </summary>
public class RotateModeToggle : MonoBehaviour
{
    [SerializeField] private RotateOverTime _rotateOverTime;
    [SerializeField] private Mouselook _mouselook;

    public enum RotateModes
    {
        None,
        Orbit,
        Mouselook
    }

    private RotateModes _rotateMode = RotateModes.None;

    public UnityEvent OnRotateModeChanged = new UnityEvent();
    public RotateModes RotateMode
    {
        get => _rotateMode;
        set
        {
            switch (_rotateMode)
            {
                case RotateModes.None:
                    // if going from none, disable everything
                    _rotateOverTime.enabled = false;
                    _mouselook.enabled = false;
                    break;
                case RotateModes.Orbit:
                    _rotateOverTime.enabled = false;
                    break;
                case RotateModes.Mouselook:
                    _mouselook.ReleaseInput();
                    _mouselook.enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _rotateMode = value;
            
            switch (_rotateMode)
            {
                case RotateModes.None:
                    break;
                case RotateModes.Orbit:
                    _rotateOverTime.enabled = true;
                    break;
                case RotateModes.Mouselook:
                    _mouselook.enabled = true;
                    _mouselook.CaptureInput();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            OnRotateModeChanged.Invoke();
        }
    }

    private void Reset()
    {
        _rotateOverTime = GetComponent<RotateOverTime>();
        _mouselook = GetComponent<Mouselook>();
    }
}
