using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollyTrackTarget : MonoBehaviour
{
    public Transform target;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            Vector3 pos = new Vector3(target.transform.position.x,
                                     target.transform.position.y + 2.0f,
                                     target.transform.position.z + 0.5f);

            transform.position = pos;
        }
    }
}
