using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailFollow : MonoBehaviour
{
    public Rail rail;

    int currentSeg;
    float transition;
    bool iscompleted;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!rail)
        {
            return;
        }

        if (!iscompleted)
        {
            FollowTrack();
        }      
    }

    void FollowTrack()
    {
        transition += Time.deltaTime * 1 / 2.5f;

        if (transition>1)
        {
            transition = 0;
            currentSeg++;
        }
        else if (transition < 0)
        {
            transition = 1;
            currentSeg++;
        }

        transform.position = rail.LinearPosition(currentSeg, transition);
        transform.rotation = rail.Orientation(currentSeg, transition);
    }
}
