using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraSmooth : CameraCore
{
    enum TargetType { BoundingBoxCenter, AverageCenter };

    [SerializeField] public List<GameObject> targets = new List<GameObject>();

    [SerializeField] TargetType centerType = TargetType.BoundingBoxCenter;
    [SerializeField] float minDistance = 25.0f;
    [SerializeField] float maxDistance = 300.0f;
    [SerializeField] float dampingMin = 0.1f;
    [SerializeField] float dampingMax = 1.0f;
    [SerializeField] float angleMargin = 5.0f;
    
    Vector3 targetPosition = Vector3.zero;
    Vector3 camOffset = Vector3.zero;
    Vector3 zero = Vector3.zero;
    Vector3 zoomDelta = Vector3.zero;
    Vector3 camPosition = Vector3.zero;
    float camDistance = 0.0f;
    float damping = 0.0f;
    
    Camera cam;


    private void Start()
    {
        cam = Camera.main;
        camDistance = minDistance;
        camOffset = -transform.forward * camDistance;
        camPosition = transform.position;
    }

    private void FixedUpdate()
    {

        if (targets != null)
        {
            if (targets.Count == 0)
            {
                Debug.LogWarning("Your camera has no target !");
                return;
            }
        }

        Vector3 min = targets[0].transform.position, max = targets[0].transform.position;
        min.y = 0;
        max.y = 0;

        Vector3 sum = Vector3.zero;

        foreach (GameObject target in targets)
        {
            Vector3 temp = target.transform.position;
            max.x = Mathf.Max(max.x,temp.x);
            min.x = Mathf.Min(min.x,temp.x);
            max.z = Mathf.Max(max.z,temp.z);
            min.z = Mathf.Min(min.z,temp.z);

            sum += temp;
        }

        if (centerType == TargetType.BoundingBoxCenter)
        {
            targetPosition = (min + max) * 0.5f;
        }
        else
        {
            sum /= targets.Count;
            sum.y = 0;
            targetPosition = sum;
        }

        CamZoom();

        camPosition = Vector3.SmoothDamp(camPosition, targetPosition + camOffset, ref zero, damping);
        transform.position = camPosition + shakeOffset;
    }

    void CamZoom()
    {
        Quaternion rot = transform.rotation;
        Vector3 pos = targetPosition - camDistance * transform.forward;
        Matrix4x4 mat = Matrix4x4.TRS(pos, rot, Vector3.one);
        mat = mat.inverse;
        
        Vector3 t = mat.MultiplyPoint(targetPosition);
        float fovVertical = (cam.fieldOfView - 2 * angleMargin) * Mathf.Deg2Rad ;
        float fovHorizontal = 2 * Mathf.Atan(cam.aspect * Mathf.Tan((fovVertical) / 2));

        // Vertical
        float sinAlphaV = Mathf.Sin(0.5f * fovVertical);
        float cosAlphaV = Mathf.Cos(0.5f * fovVertical);
        float localOffsetV = float.PositiveInfinity;

        // Horizontal
        float sinAlphaH = Mathf.Sin(0.5f * fovHorizontal);
        float cosAlphaH = Mathf.Cos(0.5f * fovHorizontal);
        float localOffsetH = float.PositiveInfinity;

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 tempPos = mat.MultiplyPoint(targets[i].transform.position);

            float tempValue = tempPos.z - Mathf.Abs(cosAlphaV * tempPos.y / sinAlphaV);
            localOffsetV = Mathf.Min(localOffsetV, tempValue);

            tempValue = tempPos.z - Mathf.Abs(cosAlphaH * tempPos.x / sinAlphaH);
            localOffsetH = Mathf.Min(localOffsetH, tempValue);
        }

        // Final position
        if (localOffsetH * cam.aspect < localOffsetV)
        {
            camDistance -= localOffsetH;
            damping = Mathf.Lerp(dampingMin, dampingMax, 1.0f + 1.0f * (localOffsetH - 0.5f));
        }
        else
        {
            camDistance -= localOffsetV;
            damping = Mathf.Lerp(dampingMin, dampingMax, 1.0f + 1.0f * (localOffsetV - 0.5f));
        }

        if (camDistance > maxDistance) camDistance = maxDistance;
        if (camDistance < minDistance) camDistance = minDistance;

        camOffset = -transform.forward * camDistance;
    }
}
