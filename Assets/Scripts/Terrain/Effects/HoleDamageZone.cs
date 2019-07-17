using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class HoleDamageZone : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = false;
        yield return new WaitForSeconds(10.0f);
        collider.enabled = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
