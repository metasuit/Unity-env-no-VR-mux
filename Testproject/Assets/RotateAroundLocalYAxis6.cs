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
            List<double> calibrationValuesList = new List<double>();

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
                        Debug.Log(values);

                        double value = double.Parse(values[6]);
                        calibrationValuesList.Add(value);
                        Debug.Log("Value " + ": " + value);
                    }
                }

                yield return null;
            }
            // calculate the calibration voltages
            calibrationVoltage1 = Convert.ToSingle(calibrationValuesList.Average());
            Debug.Log("Calibration Voltage Pos 1: " + calibrationVoltage1);
            Debug.Log("Calibration Pos1 finished.");
        }
    }

    IEnumerator CalibratePos2()
    {
        if (calibrated == false)
        {

            float endTime = Time.time + calibrationTime;           
            List<double> calibrationValuesList = new List<double>(); // List of calibration values 6

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

                        double value = double.Parse(values[6]);
                        calibrationValuesList.Add(value);
                        Debug.Log("Value " + ": " + value);
                    }
                }

                yield return null;
            }
            // calculate the calibration voltages
            calibrationVoltage2 = Convert.ToSingle(calibrationValuesList.Average());
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
                    //string[] values = System.Text.Encoding.Default.GetString(test).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();
                }
            }
            // Split the string into 7 values and parse them to floats
            string[] values = measuredValue.Split(',');

            float floatValues = float.Parse(values[6]);
            float vectorRotation = (floatValues - voltageOffsetEstim) * voltagetoDegEstim;


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
                    //string[] values = System.Text.Encoding.Default.GetString(test).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();
                }
            }
            string[] values = measuredValue.Split(',');
            //Get values 3-5
            float floatValues = float.Parse(values[6]);
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
