using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// A text field that lets you set the mouse sensitivity
///
/// Note that in a shipping game, you likely want a slider for a better player experience. This will help with exact
/// tuning during development.
/// </summary>
public class MouseSensitivityInput : MonoBehaviour
{
    [SerializeField] private Mouselook _mouselook;
    [SerializeField] private TMP_InputField _input;

    private void Reset()
    {
        _mouselook = FindObjectOfType<Mouselook>();
        _input = GetComponent<TMP_InputField>();
    }

    private void Start()
    {
        _input.contentType = TMP_InputField.ContentType.DecimalNumber;
        _mouselook.OnMouseSensitivityChanged.AddListener(HandleMouseSensitivityChanged);
        HandleMouseSensitivityChanged();
        _input.onValueChanged.AddListener(HandleValueChanged);
    }

    private void HandleValueChanged(string sensitivityString)
    {
        if (float.TryParse(sensitivityString, out float sensitivity))
        {
            _mouselook.MouseSensitivity = sensitivity;
        }
    }

    private void HandleMouseSensitivityChanged()
    {
        _input.text = _mouselook.MouseSensitivity.ToString(CultureInfo.InvariantCulture);
    }
}
