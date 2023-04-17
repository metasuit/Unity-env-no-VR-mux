using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FCButtonScript : MonoBehaviour
{
    Text textField;
    int number;
    // Start is called before the first frame update
    void Start()
    {
        textField = GameObject.Find("CalibStatus").GetComponent<Text>();

    }
    public void changeText()
    {
        number += 10;
        textField.text = "" + number;
    }
    // Update is called once per frame

}
