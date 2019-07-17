using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePadChecker : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Get Joystick Names
        string[] temp = Input.GetJoystickNames();

        //Check whether array contains anything
        if (temp.Length > 0)
        {
            //Iterate over every element
            for (int i = 0; i < temp.Length; ++i)
            {
                //Check if the string is empty or not
                if (!string.IsNullOrEmpty(temp[i]))
                {
                    if (!DataManager.instance.isMulti)
                    {
                        AddPlayer(i + 1);
                    }
                    //Not empty, controller temp[i] is connected
                    //Debug.Log("Controller " + i + " is connected using: " + temp[i]);
                }
                else
                {
                    //If it is empty, controller i is disconnected
                    //where i indicates the controller number
                    //Debug.Log("Controller: " + i + " is disconnected.");
                }
            }
        }
    }

    void AddPlayer(int gamePadIndex)
    {
        string ControllerStart = "Controller_" + gamePadIndex + "_Start";
        Debug.Log("Addplayer");


        if (Input.GetButtonDown(ControllerStart))
        {
            Debug.Log("Addplayer");

            if (gamePadIndex == 1 && DataManager.instance.currentNbrPlayer == 1)
            {
                DataManager.instance.currentNbrPlayer = 2;
            }

            if (gamePadIndex == 2 && DataManager.instance.currentNbrPlayer == 2)
            {
                DataManager.instance.currentNbrPlayer = 3;
            }

            if (gamePadIndex == 3 && DataManager.instance.currentNbrPlayer == 3)
            {
                DataManager.instance.currentNbrPlayer = 4;
            }

            //Debug.Log("Controller_" + gamePadIndex);
        }
    }
}
