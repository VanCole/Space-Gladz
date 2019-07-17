using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(InputField))]
public class IpAddressInputField : MonoBehaviour {
    InputField field;

    private void Start()
    {
        field = GetComponent<InputField>();
        field.onValueChanged.AddListener(UpdateConfig);
        field.text = DataManager.instance.config.ipAddress;
    }

    void UpdateConfig(string value)
    {
        DataManager.instance.config.ipAddress = value;
    }
}
