using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour {

    public enum REFERENCE { Global, Local };
    public enum AXIS { X, Y, Z };

    [SerializeField] public bool axisConstraint = false;
    [SerializeField] public REFERENCE reference = REFERENCE.Global;
    [SerializeField] public AXIS lockedAxis = AXIS.Y;
    [SerializeField] public bool constantScreenSize = false;
    [SerializeField] public float sizeCoef = 1.0f;
    [SerializeField] public Vector3 scaleOffset = Vector3.one;

    private Vector3 direction;
    private Vector3 up;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        GetAxises();

        transform.rotation = Quaternion.LookRotation(direction, up);

        if(constantScreenSize)
        {
            float distance = (Camera.main.transform.position - transform.position).magnitude;
            transform.localScale = sizeCoef * distance * scaleOffset;
        }
	}


    void GetAxises()
    {
        // From camera to this object
        direction = transform.position - Camera.main.transform.position;
        Vector3 right = Vector3.zero;

        if (axisConstraint)
        {
            switch(reference)
            {
                case REFERENCE.Global:
                    switch(lockedAxis)
                    {
                        case AXIS.X:
                            right = Vector3.right;
                            up = Vector3.Cross(direction, right);
                            direction = Vector3.Cross(right, up);
                            break;
                        case AXIS.Y:
                            up = Vector3.up;
                            right = Vector3.Cross(up, direction);
                            direction = Vector3.Cross(right, up);
                            break;
                        case AXIS.Z:
                            right = Vector3.Cross(direction, Vector3.forward);
                            direction = Vector3.forward;
                            up = Vector3.Cross(right, direction);
                            break;
                    }
                    break;
                case REFERENCE.Local:
                    switch (lockedAxis)
                    {
                        case AXIS.X:
                            right = transform.right;
                            up = Vector3.Cross(direction, right);
                            direction = Vector3.Cross(right, up);
                            break;
                        case AXIS.Y:
                            up = transform.up;
                            right = Vector3.Cross(up, direction);
                            direction = Vector3.Cross(right, up);
                            break;
                        case AXIS.Z:
                            right = Vector3.Cross(direction, transform.forward);
                            direction = transform.forward;
                            up = Vector3.Cross(right, direction);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            // Right vector to compute the orthogonal up
            right = Camera.main.transform.right;

            // Compute the orthogonal up
            up = Vector3.Cross(direction, right);
        }
        
    }
}
