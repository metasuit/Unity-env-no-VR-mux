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
    bool firstCalibrationDone = false;
    bool secondCalibrationDone = false;
    float calibrationAngle1;
    float calibrationAngle2;
    float calibrationVoltage1;
    float calibrationVoltage2;
    float voltageOffsetEstim = 1.7f;
    float voltagetoDegEstim = 300f;

    public bool selfsensingTesting;
    public float calibrationTime = 2;
    public Button calibrateButton1;
    public Button calibrateButton2;
    public Slider calibrationSlider;
    public Button startApplicationButton;
    public Button startCalibrationButton;
    public DataProcessor dataProcessor;
    public Toggle spineToggle;

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
        if(firstCalibrationDone && secondCalibrationDone)
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
        if (calibrated == false && spineToggle.isOn)
        {

            calibrationAngle1 = calibrationSlider.value;
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

            firstCalibrationDone = true;
            StartCoroutine(ChangeButtonColor1());

        }
    }

    IEnumerator CalibratePos2()
    {
        if (calibrated == false && spineToggle.isOn)
        {

            calibrationAngle2 = calibrationSlider.value;
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
        if (calibrated == false && spineToggle.isOn)
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
            float floatValues = (float)dataProcessor.GetFilteredValue(6);
            float vectorRotation = (floatValues - voltageOffsetEstim) * voltagetoDegEstim;


            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
        }
        if (calibrated == true && spineToggle.isOn)
        {
            //Get value 6
            float floatValues = (float)dataProcessor.GetFilteredValue(6);
  
           
            float vectorRotation = ((floatValues - calibrationVoltage1) / (calibrationVoltage2 - calibrationVoltage1)) * (calibrationAngle2 - calibrationAngle1) + calibrationAngle1;
            Debug.Log("vectorRotation:" + vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
        }

    }
}
