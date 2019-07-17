using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBeamGenerator : MonoBehaviour
{
    [SerializeField]
    VerticalLaserBeam[] laser = new VerticalLaserBeam[4];


    public float delay = 2.0f;
    public float duration = 10.0f;
    public float heatingIncrement;

    Vector3 velocity = Vector3.zero;

    public float smoothTime = 1.5f;
    public bool isLaunchingBeam;

    ArenaEventManager arenaEventManager;

    private void Awake()
    {
        arenaEventManager = GetComponentInParent<ArenaEventManager>();

    }

    // Use this for initialization
    void Start()
    {
        isLaunchingBeam = false;
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            laser[i] = transform.GetChild(i).GetComponent<VerticalLaserBeam>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (arenaEventManager.launchVerticalBeam)
        {
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                if (DataManager.instance.player[i].isAlive)
                {
                    laser[i].isStarting = true;
                }
            }
            arenaEventManager.launchVerticalBeam = false;
        }
       
    }


    public void StopVerticalLaser()
    {
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            laser[i].GetComponent<VerticalLaserBeam>().StopLaser(delay);
        }
    }
}
