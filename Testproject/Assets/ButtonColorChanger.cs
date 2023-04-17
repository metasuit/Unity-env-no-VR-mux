using UnityEngine;
using UnityEngine.UI;

public class ButtonColorChanger : MonoBehaviour
{
    public Color color;
    public Color colorActivated;
    Button button;
    bool isActivated = false;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ChangeColor);

        GetComponent<Image>().color = color;
    }

    void ChangeColor()
    {
        isActivated = !isActivated;
        if(isActivated == true)
        {
            GetComponent<Image>().color = colorActivated;
        }
        else
        {
            GetComponent<Image>().color = color;
        }
      
    }
}