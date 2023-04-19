using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using Button = UnityEngine.UI.Button;

public class RotateAroundLocalYAxis6 : MonoBehaviour
{
    string measuredValue;
    bool calibrated = false;
    bool legs_calibration_active = true;

    float calibrationAngleSlider = 0.5f;
    float calibrationAngle1 = 90;
    float calibrationAngle2 = 0;
    float calibrationVoltage1;
    float calibrationVoltage2;
    float voltageOffsetEstim = 1.7f;
    float voltagetoDegEstim = 300f;

    public bool selfsensingTesting;
    public float calibrationTime = 2;
    public Button calibrateButton1;
    public Button calibrateButton2;
    public Slider calibrationSlider;
    public Button startTorsoCalibrationButton;
    public Button startApplicationButton;
    public Button startLegCalibrationButton;
    public DataProcessor dataProcessor;

    private void Start()
    {

        calibrateButton1.onClick.AddListener(ButtonClick1);
        calibrateButton2.onClick.AddListener(ButtonClick2);
        startLegCalibrationButton.onClick.AddListener(LegCalibrationActive);
        startTorsoCalibrationButton.onClick.AddListener(StartCalibration);
        startApplicationButton.onClick.AddListener(EndCalibration);
        
        
    }
    void StartCalibration()
    {
        calibrated = false;
        legs_calibration_active = false;
    }
    void EndCalibration()
    {
        calibrated = true;
    }
    void ButtonClick1()
    {
        StartCoroutine(CalibratePos1());
    }
    void ButtonClick2()
    {
        StartCoroutine(CalibratePos2());
    }

    void LegCalibrationActive()
    {
        legs_calibration_active = true;
    }

    IEnumerator CalibratePos1()
    {
        if (calibrated == false && !legs_calibration_active)
        {

            float endTime = Time.time + calibrationTime;
            List<double> calibrationValuesList = new List<double>();

            while (Time.time < endTime)
            {
                Debug.Log("Running concurrent task...");

                // Get filtered value
                double filteredValue = dataProcessor.GetFilteredValue(6);
                calibrationValuesList.Add(filteredValue);
                //Debug.Log("Value " + ": " + value);
                yield return null;
            }
            // calculate the calibration voltages
            calibrationVoltage1 = Convert.ToSingle(calibrationValuesList.Average());
            //calibrationVoltage1 = Convert.ToSingle(calibrationValuesList.Min());
            Debug.Log("Calibration Voltage Pos 1: " + calibrationVoltage1);
            Debug.Log("Calibration Pos1 finished.");
        }
    }

    IEnumerator CalibratePos2()
    {
        if (calibrated == false && !legs_calibration_active)
        {

            float endTime = Time.time + calibrationTime;           
            List<double> calibrationValuesList = new List<double>(); // List of calibration values 6

            while (Time.time < endTime)
            {
                Debug.Log("Running concurrent task...");

                //
                double filteredValue = dataProcessor.GetFilteredValue(6);
                calibrationValuesList.Add(filteredValue);

                yield return null;
            }
            // calculate the calibration voltages
            calibrationVoltage2 = Convert.ToSingle(calibrationValuesList.Average());
            //calibrationVoltage2 = Convert.ToSingle(calibrationValuesList.Max());
            Debug.Log("Calibration Voltage Pos 2: " + calibrationVoltage2);
            Debug.Log("Calibration Pos2 finished.");
        }

    }

    public void RotationUpdate(System.Single value)
    {
        if (calibrated == false && !legs_calibration_active)
        {

            Vector3 to = new Vector3(0, value, 0);

            transform.localEulerAngles = to;
            calibrationAngleSlider = value;
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (selfsensingTesting == true)
        {
            float floatValues = (float)dataProcessor.GetFilteredValue(6);
            float vectorRotation = (floatValues - voltageOffsetEstim) * voltagetoDegEstim;


            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
        }
        if (calibrated == true && !legs_calibration_active)
        {
            //Get values 3-5
            float floatValues = (float)dataProcessor.GetFilteredValue(6);
            //Debug.Log("floatValues " + floatValues);
            //Debug.Log("vectorRotation: " + ((floatValues - calibrationVoltage1) / (calibrationVoltage2 - calibrationVoltage1)));
            //if(calibrationVoltage1 - calibrationVoltage2 == 0)
            if (calibrationVoltage1 - calibrationVoltage2 == 0)
            {
                throw new DivideByZeroException("Calibration voltage 1 is equal to calibration voltage 2. Division by zero is not allowed.");
            }
            float vectorRotation = ((floatValues - calibrationVoltage1) / (calibrationVoltage2 - calibrationVoltage1)) * (calibrationAngle2 - calibrationAngle1) + calibrationAngle1;
            Debug.Log("vectorRotation:" + vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
        }

    }
}
