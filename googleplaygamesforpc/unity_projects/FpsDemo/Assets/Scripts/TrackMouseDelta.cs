using TMPro;
using UnityEngine;

/// <summary>
/// Helper script for debugging mouse tracking. This will give you what the various input systems return when tracking
/// mouse positions to help you tune your game for each platform you plan to support. 
/// </summary>
public class TrackMouseDelta : MonoBehaviour
{
    [SerializeField] private TMP_Text _outputLabel;
    private Vector2 _mouseDelta = Vector2.zero;
    [SerializeField] private Mouselook _mouselook;

    private enum State
    {
        NoData,
        Tracking,
        ShowResult
    }
    private State _state;

    private void Reset()
    {
        _outputLabel = GetComponent<TMP_Text>();
        _mouselook = FindAnyObjectByType<Mouselook>();
    }

    private void Start()
    {
        _outputLabel.text = "Press [space] to start tracking";
    }

    private void Update()
    {
        switch (_state)
        {
            case State.NoData:
                UpdateNoData();
                break;
            case State.Tracking:
                UpdateTracking();
                break;
            case State.ShowResult:
                UpdateShowResult();
                break;
        }
    }

    private void UpdateShowResult()
    {
        ListenForStartButton();
    }

    private void UpdateTracking()
    {
        // TODO: copy/paste from Mouselook. Refactor
        if (_mouselook.CanUseNativeCapture())
        {
            var delta = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            _mouseDelta += delta;
        }
        else
        {
            _mouseDelta += MouseCapture.GetMouseDelta();
        }
        _outputLabel.text = $"Tracking: {_mouseDelta}";
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _state = State.ShowResult;
            _outputLabel.text = $"Moved {_mouseDelta}";
        }
    }

    private void UpdateNoData()
    {
        ListenForStartButton();
    }

    private void ListenForStartButton()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _state = State.Tracking;
            _outputLabel.text = "Tracking:";
            _mouseDelta = Vector2.zero;
        }
    }
}
