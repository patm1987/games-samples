using TMPro;
using UnityEngine;

/// <summary>
/// This is a debug utility: allows you to set the target framerate to experiment with how mouselook feels at various
/// framerates.
/// </summary>
public class TargetFramerateLabel : MonoBehaviour
{
    [SerializeField] private TMP_InputField _targetFramerateInput;

    private void Reset()
    {
        _targetFramerateInput = GetComponent<TMP_InputField>();
    }

    private void Start()
    {
        _targetFramerateInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        _targetFramerateInput.text = Application.targetFrameRate.ToString();
        _targetFramerateInput.onValueChanged.AddListener(HandleValueChanged);
    }

    private void HandleValueChanged(string targetFramerateString)
    {
        if (int.TryParse(targetFramerateString, out int targetFramerate))
        {
            Application.targetFrameRate = targetFramerate;
        }
    }
}
