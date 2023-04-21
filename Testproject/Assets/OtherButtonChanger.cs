using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OtherButtonChanger : MonoBehaviour
{
    public Color color;
    public Color colorActivated;
    public Button startCallibrationButton;
    // Start is called before the first frame update
    void Start()
    {
        startCallibrationButton.onClick.AddListener(ChangeColor);
    }
    void ChangeColor()
    {
        Debug.Log("button pressed");
        if (startCallibrationButton.GetComponent<Image>().color == colorActivated)
        {
            Debug.Log("It worked");
            startCallibrationButton.GetComponent<Image>().color = color;
        }
        

    }
}
