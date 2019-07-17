using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HorizontalAimBeam : MonoBehaviour
{
    private LineRenderer line;

    HorizontalLaserGenerator beamGenerator;

    public Vector3 startPos;
    public Vector3 endPos;

    public bool isActive;

    Vector3 velocity = Vector3.zero;
    float rayLength = 80.0f;


    void Start()
    {
        line = GetComponent<LineRenderer>();
        beamGenerator = GetComponentInParent<HorizontalLaserGenerator>();


        line.enabled = false;
        isActive = false;
    }

    void Update()
    {

        startPos = transform.GetChild(0).GetComponent<Transform>().position;
        endPos = transform.GetChild(1).GetComponent<Transform>().position;


        //if (isStarting)
        //{
        //    Debug.Log("LAUNCH");

        //    StartCoroutine(StartLaser(beamGenerator.delay, beamGenerator.duration));
        //    StartCoroutine(IncreaseLaser());
        //    isStarting = false;
        //}

        //RaycastManager();
        PositionUpdate();
    }

    void PositionUpdate()
    {
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);

        if (isActive)
        {
            line.enabled = true;
            line.startWidth = 0.04f;
        }
        else
        {
            line.enabled = false;

        }
    }

   
}