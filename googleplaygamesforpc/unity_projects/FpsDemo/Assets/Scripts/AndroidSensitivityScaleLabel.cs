using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Label that shows the additional scale applied to the Android mouse. Only updated at start.
///
/// Whatever the text is of the attached label, that will be used as a format string with {0} being replaced with the
/// current Android sensitivity scale.
/// </summary>
public class AndroidSensitivityScaleLabel : MonoBehaviour
{
    [SerializeField] private Mouselook _mouselook;
    [SerializeField] private TMP_Text _label;

    void Reset()
    {
        _mouselook = FindObjectOfType<Mouselook>();
        _label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        _label.text = string.Format(_label.text, _mouselook.AndroidSensitivityScale);
    }
}
