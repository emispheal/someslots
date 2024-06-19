using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class onClickButtonTest : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    int counter;

    public void onPress()
    {
        counter++;
        numberText.text = counter + "";
    }
}
