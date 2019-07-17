using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(InputField))]
public class UsernameInputField : MonoBehaviour {
    InputField field;

    private void Start()
    {
        field = GetComponent<InputField>();
        field.onValueChanged.AddListener(UpdateConfig);
        field.text = DataManager.instance.config.userName;
    }

    void UpdateConfig(string value)
    {
        DataManager.instance.config.userName = value;
    }
}
