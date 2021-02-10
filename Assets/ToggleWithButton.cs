using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleWithButton : MonoBehaviour
{
    public string buttonName;
    Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    void Update()
    {
        if (Input.GetButtonDown(buttonName)) {
            toggle.isOn = !toggle.isOn;
        }
    }
}
