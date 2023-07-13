using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// If we don't disable input after clicking the capture mouse button, pressing W or D
/// will start navigating the side panel instead of letting us walk.
/// </summary>
public class DisableEventsOnMouseCapture : MonoBehaviour
{
    [SerializeField] private Mouselook _mouselook;
    [SerializeField] private BaseInputModule _inputModule;

    private void Reset()
    {
        _mouselook = FindObjectOfType<Mouselook>();
        _inputModule = FindObjectOfType<BaseInputModule>();
    }

    private void Start()
    {
        _mouselook.OnInputCaptureChanged.AddListener(HandlePointerCaptureChanged);
        HandlePointerCaptureChanged();
    }

    private void OnDestroy()
    {
        _mouselook.OnInputCaptureChanged.RemoveListener(HandlePointerCaptureChanged);
    }

    private void HandlePointerCaptureChanged()
    {
        if (_mouselook.InputCaptured)
        {
            _inputModule.DeactivateModule();
        }
        else
        {
            _inputModule.ActivateModule();
        }
    }
}
