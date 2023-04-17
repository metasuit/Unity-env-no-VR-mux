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

public class RotateAroundZAxis : MonoBehaviour
{   
    string path = @"c:\tmp\Average.txt";
    string measuredValue;
    bool calibrated = false;
    float calibrationAngleSlider = 0;
    float calibrationAngle1;
    float calibrationAngle2;
    float calibrationVoltage1;
    float calibrationVoltage2;

    public bool selfsensingTesting;
    public float calibrationTime = 0.5f;
    public Button calibrateButton1;
    public Button calibrateButton2;
    public Slider calibrationSlider;
    public Button startCalibrationButton;
    public Button endStartCalibrationButton;
    
    private void Start()
    {
       
        calibrateButton1.onClick.AddListener(ButtonClick1);
        calibrateButton2.onClick.AddListener(ButtonClick2);
        startCalibrationButton.onClick.AddListener(StartCalibration);
        endStartCalibrationButton.onClick.AddListener(EndCalibration);
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
        StartCoroutine(Calibrate1());
    }
    void ButtonClick2()
    {
        StartCoroutine(Calibrate2());
    }

    IEnumerator Calibrate1()
    {
        if(calibrated==false)
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
                        measuredValue = System.Text.Encoding.Default.GetString(test);
                        calibrationValuesList.Add(double.Parse(measuredValue));
                        Debug.Log((float.Parse(measuredValue) * 100).ToString());
                    }
                }

                yield return null;
            }

            calibrationVoltage1 = Convert.ToSingle(calibrationValuesList.Average());
            calibrationAngle1 = calibrationAngleSlider;
            Debug.Log("Concurrent task finished." + calibrationVoltage1.ToString());
        }
     
    }

    IEnumerator Calibrate2()
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
                        measuredValue = System.Text.Encoding.Default.GetString(test);
                        calibrationValuesList.Add(double.Parse(measuredValue));
                        Debug.Log((float.Parse(measuredValue) * 100).ToString());
                    }
                }

                yield return null;
            }

            calibrationVoltage2 = Convert.ToSingle(calibrationValuesList.Average());
            calibrationAngle2 = calibrationAngleSlider;

            Debug.Log("Concurrent task finished." + calibrationVoltage2.ToString());
        }

    }



    public void RotationUpdate(System.Single value)
    {
        if (calibrated == false)
        {
            //Debug.Log("value is" + value);
            Vector3 to = new Vector3(0, 0, value);

            transform.eulerAngles = to;
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
                    Debug.Log((float.Parse(measuredValue) * 100).ToString());
                }
            }
            float vectorRotation = ((float.Parse(measuredValue) - 1.7f) * 600f);
            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, 0, vectorRotation);

            transform.eulerAngles = to;
        }

        if (calibrated==true)
        {
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    byte[] test = memoryStream.ToArray();
                    measuredValue = System.Text.Encoding.Default.GetString(test);
                    Debug.Log((float.Parse(measuredValue) * 100).ToString());
                }
            }
            float vectorRotation = ((float.Parse(measuredValue) - calibrationVoltage1) / (calibrationVoltage2 - calibrationVoltage1)) * (calibrationAngle2 - calibrationAngle1) + calibrationAngle1;
            Debug.Log(vectorRotation.ToString());
            Vector3 to = new Vector3(0, 0, vectorRotation);

            transform.eulerAngles = to;
        }
       
    }

}
