using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class InputController : NetworkBehaviour
{
    public MyEventSystem eventSystem;

    float camRayLength = 100f;
    bool rightTriggerPushed = false;
    bool leftTriggerPushed = false;

    bool RBController = false;
    bool LBController = false;
    bool A_Button = false;
    bool ultimateButton = false;
    bool escape = false;
    bool cancelButton = false;


    [HideInInspector] public UnityEvent autoAttack = new UnityEvent();
    [HideInInspector] public UnityEvent spell1 = new UnityEvent();
    [HideInInspector] public UnityEvent spell2 = new UnityEvent();
    [HideInInspector] public UnityEvent spell3 = new UnityEvent();
    [HideInInspector] public UnityEvent ultimate = new UnityEvent();
    [HideInInspector] public UnityEvent interact = new UnityEvent();
    [HideInInspector] public UnityEvent dodge = new UnityEvent();
    [HideInInspector] public UnityEvent cancel = new UnityEvent();
    [HideInInspector] public UnityEvent AAReleased = new UnityEvent();
    [HideInInspector] public UnityEvent S1Released = new UnityEvent();
    [HideInInspector] public UnityEvent S2Released = new UnityEvent();
    [HideInInspector] public UnityEvent S3Released = new UnityEvent();


    bool isAutoAttack = false;
    bool isSpell1 = false;
    bool isSpell2 = false;
    bool isSpell3 = false;
    bool isUltimate = false;


    public enum Controller
    {
        Keyboard,
        Gamepad1,
        Gamepad2,
        Gamepad3
    }

    public Controller controller;
    public Vector3 direction;
    public Vector3 directionToMove;

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.isMulti && !isLocalPlayer)
        {
            return;
        }

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

    void ControllerKeyboard()
    {
        MoveKeyboard();
        AttackKeyboard();
        TurningKeyboard();
    }

    void ControllerJoystick(int controllerIndex)
    {
        MoveController(controllerIndex);
        TurningController(controllerIndex);
        AttackController(controllerIndex);
    }


    void AttackKeyboard()
    {
        // AutoAttack and spell1 if not shift pressed
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            // Left click
            if (Input.GetButtonDown("Mouse_AutoAttack"))
            {
                isAutoAttack = true;
                //Debug.Log("AutoAttack launched");
                autoAttack.Invoke();
            }

            // Right click
            if (Input.GetButtonDown("Mouse_Spell1"))
            {
                isSpell1 = true;
                //Debug.Log("Spell1 launched");
                spell1.Invoke();
            }
        }
        else // Spell 2 and 3 when shift pressed
        {
            // Shift + LeftClick
            if (Input.GetButtonDown("Mouse_Spell2"))
            {
                isSpell2 = true;
                //Debug.Log("Spell2 launched");
                spell2.Invoke();
            }

            // Shift + RightClick
            if (Input.GetButtonDown("Mouse_Spell3"))
            {
                isSpell3 = true;
                //Debug.Log("Spell3 launched");
                spell3.Invoke();
            }
        }

        // Buttons released
        if (Input.GetButtonUp("Mouse_AutoAttack") && isAutoAttack)
        {
            //Debug.Log("AutoAttack released");
            isAutoAttack = false;
            AAReleased.Invoke();
        }

        if (Input.GetButtonUp("Mouse_Spell1") && isSpell1)
        {
            //Debug.Log("Spell1 released");
            isSpell1 = false;
            S1Released.Invoke();
        }

        if (Input.GetButtonUp("Mouse_Spell2") && isSpell2)
        {
            //Debug.Log("Spell2 released");
            isSpell2 = false;
            S2Released.Invoke();
        }

        if (Input.GetButtonUp("Mouse_Spell3") && isSpell3)
        {
            //Debug.Log("Spell3 released");
            isSpell3 = false;
            S3Released.Invoke();
        }


        // A
        if (Input.GetButtonDown("Keyboard_Ultimate"))
        {
            //Debug.Log("ULTIMAAAAAAATE !");
            ultimate.Invoke();
        }


        // E
        if (Input.GetButtonDown("Keyboard_Interact"))
        {
            //Debug.Log("Interact");
            interact.Invoke();
        }

        // Espace
        if (Input.GetButton("Keyboard_Dodge"))
        {
            if (escape == false)
            {
                //Debug.Log("Esquive");
                dodge.Invoke();
                escape = true;
            }
        }
        else
        {
            escape = false;
        }
    }

    void AttackController(int controllerIndex)
    {
        string ControllerBasicAttack = "Controller_" + controllerIndex.ToString() + "_RightTrigger";
        string ControllerSpell1 = "Controller_" + controllerIndex.ToString() + "_RightButton";
        string ControllerSpell2 = "Controller_" + controllerIndex.ToString() + "_LeftButton";
        string ControllerSpell3 = "Controller_" + controllerIndex.ToString() + "_LeftTrigger";
        //string ControllerUltimate = "Controller_" + controllerIndex.ToString() + "_LeftTrigger";
        string ControllerDodge = "Controller_" + controllerIndex.ToString() + "_LeftJoystickButton";
        string ControllerInteract = "Controller_" + controllerIndex.ToString() + "_ButtonA";
        string ControllerCancel = "Controller_" + controllerIndex.ToString() + "_ButtonB";




        // LT + RT (ULTIMATE) ==> true
        if (Input.GetAxis(ControllerSpell3) < 0.0f && Input.GetAxis(ControllerBasicAttack) < 0.0f && ultimateButton == false)
        {
            //Debug.Log("ULTIMATE");
            ultimate.Invoke();
            ultimateButton = true;
        }
        // RT ==> true
        else if (Input.GetAxis(ControllerBasicAttack) < 0.0f && Input.GetAxis(ControllerSpell3) >= 0.0f)
        {
            if (rightTriggerPushed == false)
            {
                //Debug.Log("AutoAttackC :: axis value " + Input.GetAxis("AutoAttackC"));
                rightTriggerPushed = true;
                isAutoAttack = true;
                autoAttack.Invoke();
            }
        }
        // LT ==> true
        else if (Input.GetAxis(ControllerSpell3) < 0.0f && Input.GetAxis(ControllerBasicAttack) >= 0.0f)
        {
            if (leftTriggerPushed == false)
            {
                //Debug.Log("Spell 3 :: axis value " + Input.GetAxis("Spell3C"));
                leftTriggerPushed = true;
                isSpell3 = true;
                spell3.Invoke();
            }
        }
        else
        {
            // LT ==> false
            if (leftTriggerPushed == true)
            {
                isSpell3 = false;
                leftTriggerPushed = false;
                S3Released.Invoke();
            }

            // RT ==> false
            if (rightTriggerPushed == true)
            {
                isAutoAttack = false;
                rightTriggerPushed = false;
                AAReleased.Invoke();
            }

            // LT + RT (ULTIMATE) ==> false
            if (ultimateButton == true)
                ultimateButton = false;
        }

        // LB 
        if (Input.GetButtonDown(ControllerSpell2))
        {
            if (LBController == false)
            {
                //Debug.Log("Spell2C");
                isSpell2 = true;
                LBController = true;
                spell2.Invoke();
            }
        }
        else if (Input.GetButtonUp(ControllerSpell2))
        {
            isSpell2 = false;
            LBController = false;
            S2Released.Invoke();
        }

        // RB 
        if (Input.GetButtonDown(ControllerSpell1))
        {
            isSpell1 = true;
            if (RBController == false)
            {
                //Debug.Log("Spell1C");
                RBController = true;
                spell1.Invoke();
            }
        }
        else if (Input.GetButtonUp(ControllerSpell1))
        {
            isSpell1 = false;
            RBController = false;
            S1Released.Invoke();
        }

        // A
        if (Input.GetButton(ControllerInteract))
        {
            if (A_Button == false)
            {
                //Debug.Log("Interact");
                interact.Invoke();
                A_Button = true;
            }
        }
        else
        {
            A_Button = false;
        }

        // L3
        if (Input.GetButton(ControllerDodge))
        {
            if (escape == false)
            {
                //Debug.Log("Esquive");
                dodge.Invoke();
                escape = true;
            }
        }
        else
        {
            escape = false;
        }

        // R3
        if (Input.GetButton(ControllerCancel))
        {
            if (cancelButton == false)
            {
                //Debug.Log("Cancel");
                cancel.Invoke();
                cancelButton = true;
            }
        }
        else
        {
            cancelButton = false;
        }
    }

    //MOUVEMENT =========================================================================================
    void MoveKeyboard()
    {
        directionToMove.x = Input.GetAxisRaw("KeyboardX");
        directionToMove.z = Input.GetAxisRaw("KeyboardZ");
        directionToMove.Normalize();
    }

    void MoveController(int controllerIndex)
    {
        string controllerX = "Controller_" + controllerIndex.ToString() + "_X";
        string controllerZ = "Controller_" + controllerIndex.ToString() + "_Z";

        directionToMove.x = Input.GetAxis(controllerX);
        directionToMove.z = Input.GetAxis(controllerZ);
    }


    void TurningController(int controllerIndex)
    {
        string controllerX = "Controller_" + controllerIndex.ToString() + "_RightStickX";
        string controllerY = "Controller_" + controllerIndex.ToString() + "_RightStickY";

        direction = new Vector3(Input.GetAxis(controllerX), 0, Input.GetAxis(controllerY));
    }

    void TurningKeyboard()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float hitdist = 0.0f;

        if (playerPlane.Raycast(ray, out hitdist))
        {
            Vector3 targetPoint = ray.GetPoint(hitdist);
            direction = (targetPoint - transform.position).normalized;
        }
        Cursor.visible = true;


    }

}
