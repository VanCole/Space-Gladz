using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    private void Update()
    {
        if(transform.parent.GetComponent<Player>().isShielded == false)
        {
            Destroy(gameObject);
        }
    }
}
