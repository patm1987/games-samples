using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// Main script controlling a number of UI elements (various speed input fields) to control a rotate over time script
/// </summary>
public class RotateOverTimeUi : MonoBehaviour
{
    [SerializeField] private RotateOverTime _rotateOverTime;
    [SerializeField] private TMP_InputField _dxInputField;
    [SerializeField] private TMP_InputField _dyInputField;
    [SerializeField] private TMP_InputField _dzInputField;
    
    void Start()
    {
        _dxInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        _dyInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        _dzInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        
        _dxInputField.onValueChanged.AddListener(OnDxUpdated);
        _dyInputField.onValueChanged.AddListener(OnDyUpdated);
        _dzInputField.onValueChanged.AddListener(OnDzUpdated);
        
        _rotateOverTime.OnAngularVelocityUpdated.AddListener(OnAngularVelocityUpdated);
        OnAngularVelocityUpdated();
    }

    private void OnDxUpdated(string dxString)
    {
        var angularVelocity = _rotateOverTime.AngularVelocityDegrees;
        if (float.TryParse(dxString, out var dx) && dx != angularVelocity.x)
        {
            angularVelocity.x = dx;
            _rotateOverTime.AngularVelocityDegrees = angularVelocity;
        }
    }

    private void OnDyUpdated(string dyString)
    {
        var angularVelocity = _rotateOverTime.AngularVelocityDegrees;
        if (float.TryParse(dyString, out var dy) && dy != angularVelocity.y)
        {
            angularVelocity.y = dy;
            _rotateOverTime.AngularVelocityDegrees = angularVelocity;
        }
    }

    private void OnDzUpdated(string dzString)
    {
        var angularVelocity = _rotateOverTime.AngularVelocityDegrees;
        if (float.TryParse(dzString, out var dz) && dz != angularVelocity.z)
        {
            angularVelocity.z = dz;
            _rotateOverTime.AngularVelocityDegrees = angularVelocity;
        }
    }

    private void OnAngularVelocityUpdated()
    {
        _dxInputField.text = _rotateOverTime.AngularVelocityDegrees.x.ToString(CultureInfo.InvariantCulture);
        _dyInputField.text = _rotateOverTime.AngularVelocityDegrees.y.ToString(CultureInfo.InvariantCulture);
        _dzInputField.text = _rotateOverTime.AngularVelocityDegrees.z.ToString(CultureInfo.InvariantCulture);
    }
}
