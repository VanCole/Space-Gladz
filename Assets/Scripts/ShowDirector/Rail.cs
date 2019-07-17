using System.Collections;
using System.Collections.Generic;

//using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class Rail : MonoBehaviour
{
    private Transform[] nodes;

    public Vector3 LinearPosition(int seg, float ratio)
    {
        Vector3 p1 = nodes[seg].position;
        Vector3 p2 = nodes[seg + 1].position;

        return Vector3.Lerp(p1, p2, ratio);
    }


    public Quaternion Orientation(int seg, float ratio)
    {
        Quaternion q1 = nodes[seg].rotation;
        Quaternion q2 = nodes[seg +1].rotation;

        return Quaternion.Lerp(q1, q2, ratio);
    }

    // Use this for initialization
    void Start()
    {
        nodes = GetComponentsInChildren<Transform>();
    }


    private void OnDrawGizmos()
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            //Handles.DrawDottedLine(nodes[i].position, nodes[i + 1].position, 3.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
