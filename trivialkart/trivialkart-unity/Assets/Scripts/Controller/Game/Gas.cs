// Copyright 2022 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A component script for the car game object.
/// It updates the gas level of the car and
/// makes changes to the gas indicator image accordingly. 
/// </summary>
public class Gas : MonoBehaviour
{
    public GameObject noGasText;

    [SerializeField] private GasLevelImage _gasLevelImage;

    private const float FullGasolineLevel = 5.0f;
    private const float Mpg = 0.1f;
    public const float LowVolumeCoefficient = 0.2f;
    public const float MediumVolumeCoefficient = 0.4f;
    public const float HighVolumeCoefficient = 0.6f;
    private float _gasLevel = 5.0f;
    private float _totalDistanceDriven = 0f;

    public float GasLevel => _gasLevel;

    public static float FullGasLevel => FullGasolineLevel;

    // Check if there is gas left in the tank.
    public bool HasGas()
    {
        if (GasLevel > 0)
        {
            return true;
        }
        else
        {
            noGasText.SetActive(true);
            return false;
        }
    }

    // Reset the gas level when the player fills up.
    public void FilledGas()
    {
        _gasLevel = FullGasLevel;
        noGasText.SetActive(false);
    }


    // Set the gas level bar length and color according to the distance the car has traveled.
    public void SetGasLevel(float curTotalDistanceDriven)
    {
        if (GasLevel > 0)
        {
            var consumedGas = (curTotalDistanceDriven - _totalDistanceDriven) * Mpg;
            _gasLevel = GasLevel - consumedGas;
            SetGasLevelHelper();
        }
        // Update the total distance driven.
        _totalDistanceDriven = curTotalDistanceDriven;
    }


    public void SetGasLevelHelper()
    {
        _gasLevelImage.UpdateGasLevel(this);
    }
}