using TMPro;
using UnityEngine;

/// <summary>
/// A label that shows the (approximate) framerate.
/// </summary>
public class FramerateLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text _framerateLabel;
    private string _formatString;

    private void Reset()
    {
        _framerateLabel = GetComponent<TMP_Text>();
    }

    private void Awake()
    {
        _formatString = _framerateLabel.text;
    }

    // Update is called once per frame
    void Update()
    {
        _framerateLabel.text = string.Format(_formatString, 1f/Time.smoothDeltaTime);
    }
}
