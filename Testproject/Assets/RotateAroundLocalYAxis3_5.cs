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

public class RotateAroundLocalYAxis3_5 : MonoBehaviour
{
    string measuredValue;
    bool calibrated = false;
    float calibrationAngle1;
    float calibrationAngle2;
    float calibrationVoltage1;
    float calibrationVoltage2;
    float voltageOffsetEstim = 1.7f;
    float voltagetoDegEstim = 300f;
    bool firstCalibrationDone = false;
    bool secondCalibrationDone = false;

    public bool selfsensingTesting;
    public float calibrationTime = 2;
    public int numberOfHasels = 1;
    public Button calibrateButton1;
    public Button calibrateButton2;
    public Slider calibrationSlider;
    public Button startCalibrationButton;
    public Button startApplicationButton;
    public DataProcessor dataProcessor;
    public Toggle rightLegToggle;

    private void Start()
    {

        calibrateButton1.onClick.AddListener(ButtonClick1);
        calibrateButton2.onClick.AddListener(ButtonClick2);
        startCalibrationButton.onClick.AddListener(StartCalibration);
        startApplicationButton.onClick.AddListener(EndCalibration);
    }
    void StartCalibration()
    {
        calibrated = false;
        firstCalibrationDone = false;
        secondCalibrationDone = false;
    }
    void EndCalibration()
    {
        if (firstCalibrationDone && secondCalibrationDone)
        {
            calibrated = true;
        }
        else
        {
            Console.WriteLine("Calibration didn't work");
        }

    }
    void ButtonClick1()
    {
        StartCoroutine(CalibratePos1());
    }
    void ButtonClick2()
    {
        StartCoroutine(CalibratePos2());
    }


    IEnumerator CalibratePos1()
    {
        if (calibrated == false && rightLegToggle.isOn)
        {

            calibrationAngle1 = calibrationSlider.value;
            float endTime = Time.time + calibrationTime;
            List<double>[] calibrationValuesLists = new List<double>[numberOfHasels]; // array of seven lists
            float[] calibrationVoltagesPos1 = new float[numberOfHasels]; // array of 3 calibration voltages
            for (int i = 0; i < numberOfHasels; i++)
            {
                calibrationValuesLists[i] = new List<double>(); // initialize each list
            }

            while (Time.time < endTime)
            {
                // Debug.Log("Running concurrent task...");

                // Get filtered values                
                for (int i = 0; i < numberOfHasels; i++)
                {
                    double filteredValue = dataProcessor.GetFilteredValue(i+3);
                    calibrationValuesLists[i].Add(filteredValue); // add the value to the corresponding list
                }


                yield return null;
            }
            // calculate the calibration voltages for each list
            for (int i = 0; i < numberOfHasels; i++)
            {
                calibrationVoltagesPos1[i] = Convert.ToSingle(calibrationValuesLists[i].Average());
                //Debug.Log("Calibration voltage " + (i + 1) + ": " + calibrationVoltagesPos1[i]);
            }
            calibrationVoltage1 = calibrationVoltagesPos1.Average();
            Debug.Log("Calibration Voltage Pos 1: " + calibrationVoltage1);
            Debug.Log("Calibration Pos1 finished.");

            firstCalibrationDone = true;
            StartCoroutine(ChangeButtonColor1());
        }
    }

    IEnumerator CalibratePos2()
    {
        if (calibrated == false && rightLegToggle.isOn)
        {

            calibrationAngle2 = calibrationSlider.value;
            float endTime = Time.time + calibrationTime;
            List<double>[] calibrationValuesLists = new List<double>[numberOfHasels]; // array of seven lists
            float[] calibrationVoltagesPos2 = new float[numberOfHasels]; // array of seven calibration voltages
            for (int i = 0; i < numberOfHasels; i++)
            {
                calibrationValuesLists[i] = new List<double>(); // initialize each list
            }

            while (Time.time < endTime)
            {
                Debug.Log("Running concurrent task...");

                // Get filtered values                
                for (int i = 0; i < numberOfHasels; i++)
                {
                    double filteredValue = dataProcessor.GetFilteredValue(i+3);
                    calibrationValuesLists[i].Add(filteredValue); // add the value to the corresponding list
                }

                yield return null;
            }
            for (int i = 0; i < numberOfHasels; i++)
            {
                calibrationVoltagesPos2[i] = Convert.ToSingle(calibrationValuesLists[i].Average());
                //Debug.Log("Calibration voltage " + (i + 1) + ": " + calibrationVoltagesPos1[i]);
            }
            calibrationVoltage2 = calibrationVoltagesPos2.Average();
            Debug.Log("Calibration Voltage Pos 2: " + calibrationVoltage2);
            Debug.Log("Calibration Pos2 finished.");

            if (calibrationVoltage1 - calibrationVoltage2 == 0)
            {
                Debug.Log("Calibration voltage 1 is equal to calibration voltage 2. Try to calibrate again");
                StartCoroutine(ChangeButtonColorError());
            }
            else
            {
                secondCalibrationDone = true;
                StartCoroutine(ChangeButtonColor2());
            }

        }

    }
    IEnumerator ChangeButtonColor1()
    {
        calibrateButton1.GetComponent<Image>().color = Color.green;
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 second
        calibrateButton1.GetComponent<Image>().color = Color.white; // Change the color back to white
    }

    IEnumerator ChangeButtonColor2()
    {
        calibrateButton2.GetComponent<Image>().color = Color.green;
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 second
        calibrateButton2.GetComponent<Image>().color = Color.white; // Change the color back to white
    }
    IEnumerator ChangeButtonColorError()
    {
        calibrateButton1.GetComponent<Image>().color = Color.red;
        calibrateButton2.GetComponent<Image>().color = Color.red;
        yield return new WaitForSeconds(1f); // Wait for 1 second
        calibrateButton1.GetComponent<Image>().color = Color.white; // Change the color back to white
        calibrateButton2.GetComponent<Image>().color = Color.white;

    }


    public void RotationUpdate(System.Single value)
    {
        if (calibrated == false && rightLegToggle.isOn)
        {

            Vector3 to = new Vector3(0, value, 0);

            transform.localEulerAngles = to;
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (selfsensingTesting == true)
        {
            float[] floatValues = new float[numberOfHasels];
            float vectorRotation = 0;

            //Get first 3 values
            for (int i = 0; i < numberOfHasels; i++)
            {
                floatValues[i] = (float)dataProcessor.GetFilteredValue(i+3);
                vectorRotation += (floatValues[i] - voltageOffsetEstim) * voltagetoDegEstim;
            }
            vectorRotation /= numberOfHasels;

            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
        }
        if (calibrated == true && rightLegToggle.isOn)
        {
            float[] floatValues = new float[numberOfHasels];
            float vectorRotation = 0;
            //Get first 3 values
            for (int i = 0; i < numberOfHasels; i++)
            {
                floatValues[i] = (float)dataProcessor.GetFilteredValue(i+3);
                vectorRotation += ((floatValues[i] - calibrationVoltage1) / (calibrationVoltage2 - calibrationVoltage1)) * (calibrationAngle2 - calibrationAngle1) + calibrationAngle1;
            }
            vectorRotation /= numberOfHasels;
            //Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;

        }

    }

}