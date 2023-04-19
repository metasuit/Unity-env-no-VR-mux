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

public class RotateAroundLocalYAxis : MonoBehaviour
{
    string measuredValue;
    bool calibrated = false;
    bool torso_calibration_active = false;
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
    public Button startLegsCalibrationButton;
    public Button startTorsoCalibrationButton;
    public Button startApplicationButton;
    public DataProcessor dataProcessor;

    private void Start()
    {
       
        calibrateButton1.onClick.AddListener(ButtonClick1);
        calibrateButton2.onClick.AddListener(ButtonClick2);
        startLegsCalibrationButton.onClick.AddListener(StartCalibration);
        startApplicationButton.onClick.AddListener(EndCalibration);
        startTorsoCalibrationButton.onClick.AddListener(TorsoCalibrationActive);
    }   
    void StartCalibration()
    {
        calibrated = false;
    }
    void EndCalibration()
    {
        calibrated = true;
        torso_calibration_active = false;
    }
    void ButtonClick1()
    {
        StartCoroutine(CalibratePos1());
    }
    void ButtonClick2()
    {
        StartCoroutine(CalibratePos2());
    }

    void TorsoCalibrationActive()
    {
        torso_calibration_active = true;
    }

    IEnumerator CalibratePos1()
    {
        if (calibrated == false &&!torso_calibration_active)
        {

            float endTime = Time.time + calibrationTime;
            List<double>[] calibrationValuesLists = new List<double>[3]; // array of seven lists
            float[] calibrationVoltagesPos1 = new float[3]; // array of 3 calibration voltages
            for (int i = 0; i < 3; i++)
            {
                calibrationValuesLists[i] = new List<double>(); // initialize each list
            }

            while (Time.time < endTime)
            {
                Debug.Log("Running concurrent task...");

                // Get filtered values                
                for(int i = 0; i < 3; i++)
                {
                    double filteredValue = dataProcessor.GetFilteredValue(i);
                    calibrationValuesLists[i].Add(filteredValue); // add the value to the corresponding list
                }
                

                yield return null;
            }
            // calculate the calibration voltages for each list
            for (int i = 0; i < 3; i++)
            {
                calibrationVoltagesPos1[i] = Convert.ToSingle(calibrationValuesLists[i].Average());
                //Debug.Log("Calibration voltage " + (i + 1) + ": " + calibrationVoltagesPos1[i]);
            }
            calibrationVoltage1 = calibrationVoltagesPos1.Average();
            Debug.Log("Calibration Voltage Pos 1: " + calibrationVoltage1);
            Debug.Log("Calibration Pos1 finished.");
        }
    }

    IEnumerator CalibratePos2()
    {
        if (calibrated == false && !torso_calibration_active)
        {

            float endTime = Time.time + calibrationTime;
            List<double>[] calibrationValuesLists = new List<double>[3]; // array of seven lists
            float[] calibrationVoltagesPos2 = new float[3]; // array of seven calibration voltages
            for (int i = 0; i < 3; i++)
            {
                calibrationValuesLists[i] = new List<double>(); // initialize each list
            }

            while (Time.time < endTime)
            {
                Debug.Log("Running concurrent task...");

                // Get filtered values                
                for (int i = 0; i < 3; i++)
                {
                    double filteredValue = dataProcessor.GetFilteredValue(i);
                    calibrationValuesLists[i].Add(filteredValue); // add the value to the corresponding list
                }

                yield return null;
            }
            for (int i = 0; i < 3; i++)
            {
                calibrationVoltagesPos2[i] = Convert.ToSingle(calibrationValuesLists[i].Average());
                //Debug.Log("Calibration voltage " + (i + 1) + ": " + calibrationVoltagesPos1[i]);
            }
            calibrationVoltage2 = calibrationVoltagesPos2.Average();
            Debug.Log("Calibration Voltage Pos 2: " + calibrationVoltage2);
            Debug.Log("Calibration Pos2 finished.");
        }

    }

    public void RotationUpdate(System.Single value)
    {
        if (calibrated == false && !torso_calibration_active)
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
            float[] floatValues = new float[3];
            float vectorRotation = 0;

            //Get first 3 values
            for (int i = 0; i < 3; i++)
            {
                floatValues[i] = (float)dataProcessor.GetFilteredValue(i);
                vectorRotation += (floatValues[i] - voltageOffsetEstim) * voltagetoDegEstim;
            }
            vectorRotation /= 3;
            
            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
        }
        if(calibrated==true && !torso_calibration_active)
        {
            float[] floatValues = new float[3];
            float vectorRotation = 0;
            //Get first 3 values
            for (int i = 0; i < 3; i++)
            {
                floatValues[i] = (float)dataProcessor.GetFilteredValue(i);
                if (calibrationVoltage1 - calibrationVoltage2 == 0)
                {
                    throw new DivideByZeroException("Calibration voltage 1 is equal to calibration voltage 2. Division by zero is not allowed.");
                }
                vectorRotation += ((floatValues[i] - calibrationVoltage1) / (calibrationVoltage2 - calibrationVoltage1)) * (calibrationAngle2 - calibrationAngle1) + calibrationAngle1;
            }
            vectorRotation /= 3;
            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
            
        }
       
    }

}
