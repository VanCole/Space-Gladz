using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNetwork : CameraCore
{
    [SerializeField] public GameObject target;
    [SerializeField] float minDistance = 25.0f;
    [SerializeField] float maxDistance = 300.0f;
    [SerializeField] float movementDamping = 0.0f;
    [SerializeField] float scrollDamping = 0.0f;
    [SerializeField] float scrollSensibility = 0.0f;

    Vector3 targetPosition = Vector3.zero;
    Vector3 position = Vector3.zero;
    Vector3 camOffset = Vector3.zero;
    Vector3 positionSpeed = Vector3.zero;
    Vector3 zoomDelta = Vector3.zero;
    float camDistance = 0.0f;
    float targetCamDistance = 0.0f;
    float distanceSpeed = 0.0f;

    private void Start()
    {
        camDistance = minDistance;
        camOffset = -transform.forward * camDistance;
    }

    private void FixedUpdate()
    {

        if (target == null)
        {
            Debug.LogWarning("Your camera has no target !");
            return;
        }

        targetPosition = target.transform.position;
        targetPosition.y = 0.0f;

        CamZoom();

        camDistance = Mathf.SmoothDamp(camDistance, targetCamDistance, ref distanceSpeed, scrollDamping);
        position = Vector3.SmoothDamp(position, targetPosition, ref positionSpeed, movementDamping);

        transform.position = position - transform.forward * camDistance + shakeOffset;
        //transform.position = Vector3.SmoothDamp(transform.position, targetPosition + camOffset, ref positionSpeed, movementDamping);

        ChangeTarget();
    }

    void CamZoom()
    {
        float zoomScrolling = -Input.mouseScrollDelta.y;
        targetCamDistance *= 1.0f + zoomScrolling * scrollSensibility;

        if (targetCamDistance > maxDistance) targetCamDistance = maxDistance;
        if (targetCamDistance < minDistance) targetCamDistance = minDistance;

        //camOffset = -transform.forward * camDistance;
    }

    // Change target when player die
    void ChangeTarget()
    {
        Player currentPlayer = target.GetComponent<Player>();
        if (currentPlayer && !currentPlayer.isAlive)
        {
            foreach (Player player in DataManager.instance.player)
            {
                if (player != null && player.isAlive)
                {
                    target = player.gameObject;
                    break;
                }
            }
        }
    }
}
