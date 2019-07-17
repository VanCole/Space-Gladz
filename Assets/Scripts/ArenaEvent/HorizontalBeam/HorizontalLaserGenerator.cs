using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalLaserGenerator : MonoBehaviour
{
    [SerializeField]
    HorizontalLaserBeam[] laser = new HorizontalLaserBeam[6];

    [SerializeField]
    Transform arenaEdge;

    public int rngNumberNet;
    public int[] rngLaserNet = new int[3];

    public float delay = 2.0f;
    public float duration = 20.0f;
    public float heatingIncrement;

    Vector3 velocity = Vector3.zero;

    public bool isLaunchingBeam;

    float timeToRotate = 10.0f;
    float targetRotation;
    float startRotation;

    int randomLaser;

    ArenaEventManager arenaEventManager;

    private void Awake()
    {
        arenaEventManager = GetComponentInParent<ArenaEventManager>();

    }

    // Use this for initialization
    void Start()
    {
        arenaEdge = GameObject.Find("Pivot ArenaEdge").transform;
        isLaunchingBeam = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (arenaEventManager.launchHorizontalBeam)
        {
            RandomLaserActivation();

            startRotation = transform.rotation.y;
            targetRotation = transform.rotation.y + 360.0f;

            StartCoroutine(RotateBeam(startRotation, targetRotation, duration + 2.0f));

            arenaEventManager.launchHorizontalBeam = false;
        }


        //if (Input.GetKeyDown(KeyCode.P))
        //{

        //    startRotation = transform.rotation.y;
        //    targetRotation = transform.rotation.y + 360.0f;

        //    StartCoroutine(RotateBeam(startRotation, targetRotation, duration + 2.0f));
        //}
    }

    void RandomLaserActivation()
    {
        arenaEventManager.PlayRandomEventSound();

        int rngNumber = Random.Range(0, 3);
        if (DataManager.instance.isMulti)
            rngNumber = rngNumberNet;
        //Debug.Log(rngNumber);

        for (int i = 0; i <= rngNumber; i++)
        {
            int rngLaser = Random.Range(0, 6);
            if (DataManager.instance.isMulti)
                rngLaser = rngLaserNet[i];
            laser[rngLaser].isStarting = true;
        }
    }


    public IEnumerator RotateBeam(float start, float target, float rotateTime)
    {
        float speedRotate = 0.0f;
        while (speedRotate < 1)
        {
            speedRotate += Time.deltaTime / rotateTime;
            float dampRotation = Mathf.Lerp(start, target, speedRotate);
            transform.rotation = Quaternion.Euler(0.0f, dampRotation, 0.0f);
            arenaEdge.transform.rotation = Quaternion.Euler(0.0f, dampRotation, 0.0f);

            yield return null;
        }
    }

    public void StopHorizontalLaser()
    {
        for (int i = 0; i < laser.Length; i++)
        {
            laser[i].GetComponent<HorizontalLaserBeam>().StopLaser(delay);
        }
    }

}
