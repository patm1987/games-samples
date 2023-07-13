using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A button that, when clicked, gains exclusive control over the mouse cursor. This also handles cancelling capture
/// with the escape button.
///
/// Note that this works best if it's attached to the desired button, but that's not necessary
/// </summary>
public class CaptureMouseButton : MonoBehaviour
{
    /// <summary>
    /// Button to listen to clicks on
    /// </summary>
    [SerializeField] private Button _captureMouseButton;
    
    /// <summary>
    /// A text object to update with the state when the button is clicked. Best when this is the Button's label.
    /// </summary>
    [SerializeField] private TMP_Text _captureMouseText;
    
    /// <summary>
    /// Element that controls the rotate mode of the camera
    /// </summary>
    [SerializeField] private RotateModeToggle _rotateModeToggle;

    private void Reset()
    {
        _captureMouseButton = GetComponent<Button>();
        _captureMouseText = _captureMouseButton.GetComponentInChildren<TMP_Text>();
        _rotateModeToggle = FindObjectOfType<RotateModeToggle>();
    }

    private void Start()
    {
        _rotateModeToggle.OnRotateModeChanged.AddListener(HandleRotateModeChanged);
        HandleRotateModeChanged();
        
        _captureMouseButton.onClick.AddListener(HandleButtonClicked);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && _rotateModeToggle.RotateMode == RotateModeToggle.RotateModes.Mouselook)
        {
            _rotateModeToggle.RotateMode = RotateModeToggle.RotateModes.Orbit;
        }
    }

    private void HandleButtonClicked()
    {
        if (_rotateModeToggle.RotateMode != RotateModeToggle.RotateModes.Mouselook)
        {
            _rotateModeToggle.RotateMode = RotateModeToggle.RotateModes.Mouselook;
        }
    }

    private void HandleRotateModeChanged()
    {
        switch (_rotateModeToggle.RotateMode)
        {
            case RotateModeToggle.RotateModes.None:
            case RotateModeToggle.RotateModes.Orbit:
                _captureMouseText.text = "Capture Mouse";
                break;
            case RotateModeToggle.RotateModes.Mouselook:
                _captureMouseText.text = "Press Esc";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
