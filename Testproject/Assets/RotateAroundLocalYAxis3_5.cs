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
    string path = @"c:\tmp\MyTest.txt";
    string measuredValue;
    bool calibrated = false;
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
    public Button startCalibrationButton;
    public Button startApplicationButton;

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

    IEnumerator CalibratePos1()
    {
        if (calibrated == false)
        {

            float endTime = Time.time + calibrationTime;
            List<double>[] calibrationValuesLists = new List<double>[3]; // array of seven lists
            float[] calibrationVoltagesPos1 = new float[3]; // array of 3 calibration voltages (3-5)
            for (int i = 0; i < 3; i++)
            {
                calibrationValuesLists[i] = new List<double>(); // initialize each list
            }

            while (Time.time < endTime)
            {
                Debug.Log("Running concurrent task...");

                //
                using (var fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        byte[] test = memoryStream.ToArray();
                        string[] values = System.Text.Encoding.Default.GetString(test).Split(','); // split the string into an array of 7 values
                        //string[] values = System.Text.Encoding.Default.GetString(test).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();

                        for (int i = 0; i < 3; i++) // Get elements 3-5
                        {
                            double value = double.Parse(values[i + 3]); //offset for values 3-5
                            calibrationValuesLists[i].Add(value); // add the value to the corresponding list
                            //Debug.Log("Value " + (i + 1) + ": " + value);
                        }
                    }
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
        if (calibrated == false)
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

                //
                using (var fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        byte[] test = memoryStream.ToArray();
                        string[] values = System.Text.Encoding.Default.GetString(test).Split(','); // split the string into an array of values
                        //string[] values = System.Text.Encoding.Default.GetString(test).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();
                        for (int i = 0; i < 3; i++)
                        {
                            double value = double.Parse(values[i + 3]); //offset for values 3-5
                            calibrationValuesLists[i].Add(value); // add the value to the corresponding list
                            //Debug.Log("Value " + (i + 1) + ": " + value);
                        }
                    }
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
        if (calibrated == false)
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
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    byte[] test = memoryStream.ToArray();
                    measuredValue = System.Text.Encoding.Default.GetString(test);
                }
            }
            // Split the string into 7 values and parse them to floats
            string[] values = measuredValue.Split(',');
            float[] floatValues = new float[3];
            float vectorRotation = 0;


            for (int i = 0; i < 3; i++)
            {
                floatValues[i] = float.Parse(values[i]);
                vectorRotation += (floatValues[i] - voltageOffsetEstim) * voltagetoDegEstim;
            }
            vectorRotation /= 3;

            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, vectorRotation, 0);

            transform.localEulerAngles = to;
        }
        if (calibrated == true)
        {
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    byte[] test = memoryStream.ToArray();
                    measuredValue = System.Text.Encoding.Default.GetString(test);
                    //Debug.Log((float.Parse(measuredValue) * 100).ToString());
                }
            }
            string[] values = measuredValue.Split(',');
            float[] floatValues = new float[3];
            float vectorRotation = 0;
            //Get values 3-5
            for (int i = 0; i < 3; i++)
            {
                floatValues[i] = float.Parse(values[i + 3]); //+3 -> offset values 3-6
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
