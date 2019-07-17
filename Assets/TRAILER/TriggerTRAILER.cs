using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TriggerTRAILER : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MainCamera")
        {
            StartCoroutine(IncreaseCam());
        }
    }

    IEnumerator IncreaseCam()
    {
        while (GameObject.Find("LookAtCart").GetComponent<CinemachineDollyCart>().m_Speed < 20.0f)
        {
            GameObject.Find("LookAtCart").GetComponent<CinemachineDollyCart>().m_Speed +=  0.5f;

            yield return new WaitForSeconds(0.1f);

        }
    }
}
