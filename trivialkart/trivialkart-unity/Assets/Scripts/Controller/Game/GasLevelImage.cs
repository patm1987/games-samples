using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.UI;

public class GasLevelImage : MonoBehaviour
{
    [SerializeField] private Image _gasLevelImage;
    [SerializeField] private AccessibleElement _gasAccessibleElement;


    private readonly Color32 _darkRedColor = new Color32(196, 92, 29, 255);
    private readonly Color32 _orangeColor = new Color32(255, 196, 0, 255);
    private readonly Color32 _lightGreenColor = new Color32(125, 210, 76, 255);
    private readonly Color32 _greenColor = new Color32(94, 201, 93, 255);

    public void UpdateGasLevel(Gas gas) {
        var gasLevel = gas.GasLevel;

        var gasRatio = gasLevel / Gas.FullGasLevel;
        _gasLevelImage.transform.localScale = new Vector3(gasRatio, 1, 1);
        _gasAccessibleElement.value = $"{(int)(gasRatio * 100f)} percent";

        // Change the gas bar color according to the bar length.
        if (gasLevel < Gas.LowVolumeCoefficient * Gas.FullGasLevel)
        {
            _gasLevelImage.color = _darkRedColor;
        }
        else if (gasLevel < Gas.MediumVolumeCoefficient * Gas.FullGasLevel)
        {
            _gasLevelImage.color = _orangeColor;
        }
        else if (gasLevel < Gas.HighVolumeCoefficient * Gas.FullGasLevel)
        {
            _gasLevelImage.color = _lightGreenColor;
        }
        else
        {
            _gasLevelImage.color = _greenColor;
        }
    }
}
