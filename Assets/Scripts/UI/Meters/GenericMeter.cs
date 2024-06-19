using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GenericMeter : MonoBehaviour
{

    public TextMeshProUGUI textObject; 

    public int currentValue = -1;

    public string meterString;

    // Start is called before the first frame update
    public virtual void Start()
    {
        SetString();
    }
    
    public virtual void SetString()
    {
        meterString = currentValue.ToString() + "";
        textObject.text = meterString;
    }

    public void SetValue(int value)
    {
        currentValue = value;
    }

    public void SetMeterValue(int value)
    {
        SetValue(value);
        SetString();
    }

}
