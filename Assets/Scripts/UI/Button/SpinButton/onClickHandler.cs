using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    using UnityEngine.UI;

public class spinButtonOnClick : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Button button = GetComponent<Button>(); // Get the Button component attached to this GameObject
        button.onClick.AddListener(OnClick); // Add listener for button click event
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Spin Button Clicked");
    }

    void OnClick()
    {
        Debug.Log("Button clicked!"); // Print message to console
    }
}
