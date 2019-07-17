using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    GameObject bar;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bar.transform.Rotate(new Vector3(0.0f, 0.0f, -Time.deltaTime * 300.0f));
    }
}
