using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleHelpControl : MonoBehaviour
{
    public enum Controller
    {
        Keyboard,
        Gamepad1,
        Gamepad2,
        Gamepad3
    }
    public Controller controller;

    bool isHelpOn;


    // Use this for initialization
    void Start()
    {
        isHelpOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        DisplayHelp();

        if (!DataManager.instance.isMatchDone)
        {
            switch (controller)
            {
                case Controller.Keyboard:
                    ControllerKeyboard();
                    break;
                case Controller.Gamepad1:
                    ControllerJoystick((int)Controller.Gamepad1);
                    break;
                case Controller.Gamepad2:
                    ControllerJoystick((int)Controller.Gamepad2);
                    break;
                case Controller.Gamepad3:
                    ControllerJoystick((int)Controller.Gamepad3);
                    break;
                default:
                    break;
            }
        }
    }



    void ControllerJoystick(int controllerIndex)
    {
        string ControllerToggleHelp = "Controller_" + controllerIndex + "_ButtonY";

        if (Input.GetButtonDown(ControllerToggleHelp))
        {
            Debug.Log("Manette");

            isHelpOn = !isHelpOn;
        }
    }


    void ControllerKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Clavier");
            isHelpOn = !isHelpOn;
        }

    }

    void DisplayHelp()
    {
        if (!DataManager.instance.isMatchDone)
        {
            if (isHelpOn)
            {
                GetComponent<CanvasGroup>().alpha = 1.0f;
            }
            else
            {
                GetComponent<CanvasGroup>().alpha = 0.0f;
            }
        }
        else
        {
            GetComponent<CanvasGroup>().alpha = 0.0f;
        }
    }
}
